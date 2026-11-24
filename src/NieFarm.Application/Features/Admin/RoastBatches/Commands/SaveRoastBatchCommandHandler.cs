using Ardalis.Result;
using Ardalis.Specification;
using MediatR;
using NieFarm.Application.Common.Interfaces;
using NieFarm.Application.Features.Admin.RoastBatches.Dtos;
using NieFarm.Application.Features.Traceability;
using NieFarm.Application.Features.Traceability.Specifications;
using NieFarm.Domain.Entities;

namespace NieFarm.Application.Features.Admin.RoastBatches.Commands;

public class SaveRoastBatchCommandHandler(
    IRepositoryBase<RoastBatch> repository,
    IReadRepositoryBase<RoastBatch> readRepository,
    IReadRepositoryBase<Product> products,
    IWriteContextRefresher contextRefresher,
    IBatchTraceCache cache,
    IImageStorage imageStorage)
    : IRequestHandler<SaveRoastBatchCommand, Result<int>>
{
    private const int MaxCodeAttempts = 5;

    public async Task<Result<int>> Handle(SaveRoastBatchCommand request, CancellationToken cancellationToken)
    {
        // Only the existence of the product matters; the entity is discarded straight away.
        if (await products.GetByIdAsync(request.ProductId, cancellationToken) is null)
            return Result<int>.Invalid(new ValidationError("ProductId", "Product no longer exists."));

        try
        {
            return request.Id is { } id
                ? await UpdateAsync(id, request, cancellationToken)
                : await CreateAsync(request, cancellationToken);
        }
        catch (Exception ex) when (ex is ArgumentException or InvalidOperationException)
        {
            // The validator rejects these first; the domain guards are the last line of defence.
            return Result<int>.Error(ex.Message);
        }
    }

    private async Task<Result<int>> CreateAsync(SaveRoastBatchCommand request, CancellationToken ct)
    {
        string? code = null;
        for (var attempt = 0; attempt < MaxCodeAttempts; attempt++)
        {
            var candidate = BatchCode.Generate();
            if (!await readRepository.AnyAsync(new RoastBatchCodeExistsSpec(candidate), ct))
            {
                code = candidate;
                break;
            }
        }

        if (code is null)
            return Result<int>.Error("Could not generate a unique batch code. Please try again.");

        var batch = RoastBatch.Create(code, request.ProductId, request.General.RoastDate);
        Apply(batch, request);

        await repository.AddAsync(batch, ct);
        cache.Invalidate(batch.Code);
        return Result<int>.Success(batch.Id);
    }

    private async Task<Result<int>> UpdateAsync(int id, SaveRoastBatchCommand request, CancellationToken ct)
    {
        // The write context lives as long as the admin circuit: load against the database, not a stale copy.
        contextRefresher.DiscardTrackedEntities();

        var batch = await repository.FirstOrDefaultAsync(new RoastBatchByIdWithChildrenSpec(id), ct);
        if (batch is null)
            return Result<int>.NotFound();

        var previousImages = new[] { batch.FarmImageUrl, batch.LabReportImageUrl };

        try
        {
            Apply(batch, request);

            // The entity is tracked, so SaveChanges writes exactly what changed (the child rows included).
            await repository.SaveChangesAsync(ct);
        }
        catch
        {
            // A domain guard can throw half-way through Apply, leaving the tracked batch half-edited on the circuit's
            // shared write context. Throw that copy away so the next operation starts clean (a plain
            // DiscardTrackedEntities would refuse: it throws on unsaved changes). The caller still sees the failure.
            contextRefresher.ForceDiscardTrackedEntities();
            throw;
        }

        cache.Invalidate(batch.Code);

        var current = new[] { batch.FarmImageUrl, batch.LabReportImageUrl };
        var removable = previousImages
            .Where(RoastBatchImageLimits.IsManagedImage)
            .Where(old => !current.Contains(old, StringComparer.OrdinalIgnoreCase))
            .ToList();
        if (removable.Count > 0)
            await imageStorage.DeleteManyAsync(removable, ct);

        return Result<int>.Success(batch.Id);
    }

    /// <summary>The order matters: the roast date (general) is checked by the origin and roasting blocks.</summary>
    private static void Apply(RoastBatch batch, SaveRoastBatchCommand r)
    {
        var g = r.General;
        batch.UpdateGeneral(
            r.ProductId, g.RoastDate, g.BestBefore, g.PeakFromDays, g.PeakToDays,
            g.NetWeightGrams, g.PackCount, g.InternalNote);

        var o = r.Origin;
        batch.UpdateOrigin(
            o.Province, o.Commune, o.Locality, o.AltitudeMinMeters, o.AltitudeMaxMeters,
            o.Variety, o.FarmName, o.FarmStory, o.FarmImageUrl,
            o.HarvestSeason, o.HarvestedFrom, o.HarvestedTo, o.HarvestMethod);

        var p = r.Process;
        batch.UpdateProcessing(p.ProcessingMethod, p.ProcessingNote, p.FermentationHours, p.DryingDays, p.DryingMethod);
        batch.UpdateRoasting(
            p.RoastLevel, p.RoasterName, p.RoastNote, p.PackedOn,
            p.HasOneWayValve, p.IsNitrogenFlushed, p.PackagingNote);

        var q = r.Quality;
        batch.UpdateQuality(
            q.CuppingScore, q.CuppedBy, q.FlavourNotes,
            q.AcidityLevel, q.BodyLevel, q.SweetnessLevel, q.BitternessLevel,
            q.MoisturePercent, q.ScreenSize, q.DefectRatePercent, q.LabReportImageUrl);
        batch.SetCertifications(
            q.Certifications.Select(c => new CertificationSpec(c.Name, c.Number)).ToList());

        var b = r.Brewing;
        batch.UpdateBrewing(b.StorageTip);
        batch.SetBrewGuides(
            b.Guides.Select(x => new BrewGuideSpec(x.Method, x.Ratio, x.WaterTempCelsius, x.BrewTime, x.Note)).ToList());

        batch.SetPublished(r.IsPublished);
    }
}
