using Ardalis.Specification;
using MediatR;
using NieFarm.Application.Features.Admin.RoastBatches.Dtos;
using NieFarm.Application.Features.Traceability;
using NieFarm.Application.Features.Traceability.Specifications;
using NieFarm.Domain.Entities;

namespace NieFarm.Application.Features.Admin.RoastBatches.Queries;

public class GetRoastBatchForEditQueryHandler(IReadRepositoryBase<RoastBatch> repository)
    : IRequestHandler<GetRoastBatchForEditQuery, RoastBatchEditDto?>
{
    public async Task<RoastBatchEditDto?> Handle(
        GetRoastBatchForEditQuery request, CancellationToken cancellationToken)
    {
        var b = await repository.FirstOrDefaultAsync(
            new RoastBatchByIdWithChildrenSpec(request.Id, noTracking: true), cancellationToken);
        if (b is null)
            return null;

        return new RoastBatchEditDto(
            b.Id,
            b.Code,
            BatchCode.Format(b.Code),
            b.ProductId,
            b.IsPublished,
            new RoastBatchGeneralInput(
                b.RoastDate, b.BestBefore, b.PeakFromDays, b.PeakToDays,
                b.NetWeightGrams, b.PackCount, b.InternalNote),
            new RoastBatchOriginInput(
                b.OriginProvince, b.OriginCommune, b.OriginLocality, b.AltitudeMinMeters, b.AltitudeMaxMeters,
                b.Variety, b.FarmName, b.FarmStory, b.FarmImageUrl,
                b.HarvestSeason, b.HarvestedFrom, b.HarvestedTo, b.HarvestMethod),
            new RoastBatchProcessInput(
                b.ProcessingMethod, b.ProcessingNote, b.FermentationHours, b.DryingDays, b.DryingMethod,
                b.RoastLevel, b.RoasterName, b.RoastNote, b.PackedOn,
                b.HasOneWayValve, b.IsNitrogenFlushed, b.PackagingNote),
            new RoastBatchQualityInput(
                b.CuppingScore, b.CuppedBy, b.FlavourNotes,
                b.AcidityLevel, b.BodyLevel, b.SweetnessLevel, b.BitternessLevel,
                b.MoisturePercent, b.ScreenSize, b.DefectRatePercent, b.LabReportImageUrl,
                b.Certifications.OrderBy(c => c.SortOrder)
                    .Select(c => new RoastBatchCertificationInput(c.Name, c.Number)).ToList()),
            new RoastBatchBrewingInput(
                b.StorageTip,
                b.BrewGuides.OrderBy(g => g.SortOrder)
                    .Select(g => new RoastBatchBrewGuideInput(g.Method, g.Ratio, g.WaterTempCelsius, g.BrewTime, g.Note))
                    .ToList()));
    }
}
