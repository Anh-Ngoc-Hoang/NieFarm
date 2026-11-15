using System.Text.Json.Serialization;
using Ardalis.Result;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using NieFarm.Application.Common.Interfaces;
using NieFarm.Application.Features.Payments.Commands;
using IResult = Microsoft.AspNetCore.Http.IResult;

namespace NieFarm.Web.Endpoints;

/// <summary>
/// The SePay body. Only the fields we use are bound; everything else SePay sends is ignored.
/// // verify: field names per SePay docs read 2026-11-08 (plan D14); re-check against the live docs.
/// </summary>
public sealed class SePayWebhookPayload
{
    [JsonPropertyName("id")] public long Id { get; set; }
    [JsonPropertyName("accountNumber")] public string? AccountNumber { get; set; }
    [JsonPropertyName("code")] public string? Code { get; set; }
    [JsonPropertyName("content")] public string? Content { get; set; }
    [JsonPropertyName("transferType")] public string? TransferType { get; set; }
    [JsonPropertyName("transferAmount")] public decimal TransferAmount { get; set; }
    [JsonPropertyName("referenceCode")] public string? ReferenceCode { get; set; }
}

/// <summary>
/// POST /api/payments/sepay. SePay posts every incoming transfer here; the join key to an order is the
/// order code in the transfer content. Anonymous by design (SePay has no cookie), protected by an HMAC-SHA256
/// signature over the raw body, checked in constant time inside IPaymentWebhookAuthenticator.
/// </summary>
public static class SePayWebhookEndpoint
{
    public const string RateLimitPolicy = "sepay-webhook";
    private const string Provider = "SePay";
    private const string SignatureHeader = "X-SePay-Signature";
    private const string TimestampHeader = "X-SePay-Timestamp";
    private const int MaxBodyBytes = 16 * 1024;

    public static IEndpointRouteBuilder MapSePayWebhook(this IEndpointRouteBuilder app)
    {
        app.MapPost("/api/payments/sepay", HandleAsync)
            .AllowAnonymous()
            .DisableAntiforgery()
            .RequireRateLimiting(RateLimitPolicy);

        return app;
    }

    // Body limit 16 KB. EndpointRoutingMiddleware enforces the attribute metadata.
    [RequestSizeLimit(16 * 1024)]
    private static async Task<IResult> HandleAsync(
        HttpContext httpContext,
        IPaymentWebhookAuthenticator authenticator,
        ISender sender,
        ILoggerFactory loggerFactory,
        CancellationToken ct)
    {
        // Program.cs re-executes empty 4xx answers through "/not-found" (a Blazor page), which for a POST
        // turns into an antiforgery 400 and masks the real 401/404. This is an API: keep the raw status.
        httpContext.Features.Get<Microsoft.AspNetCore.Diagnostics.IStatusCodePagesFeature>()?.Enabled = false;

        // Disabled or weakly configured: pretend the endpoint does not exist.
        if (!authenticator.IsEnabled)
            return Results.NotFound();

        // The body is read as BYTES exactly once, before any parsing, so the signature is checked against
        // what SePay actually sent and the same bytes are then deserialised. Over the limit: refused.
        var body = await ReadBodyAsync(httpContext.Request, ct);
        if (body is null)
            return Results.StatusCode(StatusCodes.Status413PayloadTooLarge);

        // No detail on failure.
        var headers = httpContext.Request.Headers;
        if (!authenticator.Authenticate(
                headers[SignatureHeader].ToString(), headers[TimestampHeader].ToString(), body))
            return Results.Unauthorized();

        // Bound by hand, after authentication, so an unauthenticated caller learns nothing from the
        // binder (a malformed body would otherwise answer 400 before the 404/401 checks).
        SePayWebhookPayload? payload;
        try
        {
            payload = System.Text.Json.JsonSerializer.Deserialize<SePayWebhookPayload>(body);
        }
        catch (System.Text.Json.JsonException)
        {
            return Results.BadRequest();
        }

        // A missing "id" binds to 0; without this every id-less delivery would collapse into one ExternalId.
        if (payload is null || payload.Id <= 0)
            return Results.BadRequest();

        try
        {
            var result = await sender.Send(new ConfirmPaymentFromGatewayCommand(
                Provider,
                payload.Id.ToString(System.Globalization.CultureInfo.InvariantCulture),
                payload.TransferType ?? "",
                payload.AccountNumber,
                payload.TransferAmount,
                payload.Content,
                payload.Code,
                payload.ReferenceCode), ct);

            if (result.IsInvalid())
                return Results.BadRequest();

            // A concurrency conflict is a real failure: 500 makes SePay retry, and the retry then
            // finds the duplicate or the settled state.
            if (result.Status == ResultStatus.Conflict || !result.IsSuccess)
                return Failure();

            return Acknowledge();
        }
        catch (Exception ex)
        {
            // Never log the body, account number or key: only the exception and the external id.
            loggerFactory.CreateLogger("NieFarm.Web.Endpoints.SePayWebhook")
                .LogError(ex, "SePay webhook {ExternalId} failed", payload.Id);
            return Failure();
        }
    }

    /// <summary>Reads at most 16 KB; null when the body is larger.</summary>
    private static async Task<byte[]?> ReadBodyAsync(HttpRequest request, CancellationToken ct)
    {
        if (request.ContentLength > MaxBodyBytes)
            return null;

        try
        {
            using var buffer = new MemoryStream();
            var chunk = new byte[4096];
            int read;
            while ((read = await request.Body.ReadAsync(chunk, ct)) > 0)
            {
                if (buffer.Length + read > MaxBodyBytes)
                    return null;
                buffer.Write(chunk, 0, read);
            }

            return buffer.ToArray();
        }
        catch (BadHttpRequestException)
        {
            // The server's own limit (RequestSizeLimit) tripped mid-read.
            return null;
        }
    }

    // SePay treats only HTTP 200/201 WITH a body of {"success": true} as delivered. A 200 without
    // success:true is recorded as failed and is NOT retried, so the answers are built in these two
    // places only. Every business outcome (including ignored ones) is acknowledged; retrying an
    // unmatched transfer would never help.
    private static IResult Acknowledge() => Results.Json(new { success = true });

    private static IResult Failure() =>
        Results.Json(new { success = false }, statusCode: StatusCodes.Status500InternalServerError);
}
