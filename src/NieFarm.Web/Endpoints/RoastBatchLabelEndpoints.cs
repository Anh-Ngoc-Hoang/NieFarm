using MediatR;
using Microsoft.AspNetCore.Authorization;
using NieFarm.Application.Features.Admin.RoastBatches.Queries;
using NieFarm.Web.Content;
using NieFarm.Web.Services.Traceability;
using IResult = Microsoft.AspNetCore.Http.IResult;

namespace NieFarm.Web.Endpoints;

/// <summary>
/// Admin downloads for a batch (R-26): QR as PNG or SVG and the printable label as PDF. Plain GETs without side
/// effects, never anonymous. The QR always points at <c>PublicUrls</c> (configuration), never at the request host, so a
/// forged Host header cannot end up on a printed label.
/// </summary>
public static class RoastBatchLabelEndpoints
{
    public static IEndpointRouteBuilder MapRoastBatchLabelEndpoints(this IEndpointRouteBuilder app)
    {
        var admin = new AuthorizeAttribute { Roles = "Admin,SuperAdmin" };

        app.MapGet("/api/admin/batches/{id:int}/qr.png", (int id, ISender sender, PublicUrls urls, HttpContext http, CancellationToken ct) =>
            HandleAsync(id, "png", sender, urls, http, ct, (url, _) => ("image/png", QrMatrix.Create(url).ToPng(20))))
            .RequireAuthorization(admin);

        app.MapGet("/api/admin/batches/{id:int}/qr.svg", (int id, ISender sender, PublicUrls urls, HttpContext http, CancellationToken ct) =>
            HandleAsync(id, "svg", sender, urls, http, ct, (url, _) => ("image/svg+xml", System.Text.Encoding.UTF8.GetBytes(QrMatrix.Create(url).ToSvg()))))
            .RequireAuthorization(admin);

        app.MapGet("/api/admin/batches/{id:int}/label.pdf", async (
                int id, string? layout, int? copies, int? skip, ISender sender, PublicUrls urls, HttpContext http, CancellationToken ct) =>
            {
                LabelLayout parsed;
                if (string.IsNullOrEmpty(layout) || layout.Equals("single", StringComparison.OrdinalIgnoreCase))
                    parsed = LabelLayout.Single;
                else if (layout.Equals("a4", StringComparison.OrdinalIgnoreCase))
                    parsed = LabelLayout.A4;
                else
                    return Results.BadRequest("Unknown layout. Use 'single' or 'a4'.");

                return await HandleAsync(id, "pdf", sender, urls, http, ct, (url, label) =>
                {
                    urls.TryGetShortEntryText(out var shortText);
                    var bytes = RoastBatchLabelPdf.Render(label, url, shortText, parsed, copies ?? 1, skip ?? 0);
                    return ("application/pdf", bytes);
                });
            })
            .RequireAuthorization(admin);

        return app;
    }

    private static async Task<IResult> HandleAsync(
        int id, string extension, ISender sender, PublicUrls urls, HttpContext http, CancellationToken ct,
        Func<string, NieFarm.Application.Features.Admin.RoastBatches.Dtos.RoastBatchLabelDto, (string ContentType, byte[] Bytes)> render)
    {
        // Program.cs re-executes empty 4xx answers through "/not-found" (a Blazor page); this is an API, keep the raw status.
        http.Features.Get<Microsoft.AspNetCore.Diagnostics.IStatusCodePagesFeature>()?.Enabled = false;

        var label = await sender.Send(new GetRoastBatchLabelQuery(id), ct);
        if (label is null)
            return Results.NotFound();

        if (!urls.TryGetTraceUrl(label.Code, out var url))
            return Results.Conflict("Site:PublicBaseUrl is not configured.");

        var (contentType, bytes) = render(url, label);

        http.Response.Headers.CacheControl = "no-store";
        http.Response.Headers.XContentTypeOptions = "nosniff";
        return Results.File(bytes, contentType, $"niefarm-lo-{label.Code}.{extension}");
    }
}
