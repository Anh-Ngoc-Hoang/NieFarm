using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using NieFarm.Application.Common;
using NieFarm.Application.Common.Images;
using NieFarm.Application.Common.Interfaces;
using NieFarm.Infrastructure.Common;
using NieFarm.Infrastructure.Data;
using NieFarm.Infrastructure.Identity;
using System.Threading.RateLimiting;
using NieFarm.Web.Components;
using NieFarm.Web.Endpoints;
using NieFarm.Web.Middleware;
using NieFarm.Web.Services.Admin;
using NieFarm.Web.Services.Cart;
using NieFarm.Web.Services.Http;
using NieFarm.Web.Services.Orders;
using NieFarm.Web.Services.Traceability;
using Serilog;

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Warning()
    .WriteTo.Console()
    .WriteTo.File("logs/startup-.log", rollingInterval: RollingInterval.Day)
    .CreateBootstrapLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);

    builder.Host.UseSerilog((ctx, services, cfg) => cfg
        .ReadFrom.Configuration(ctx.Configuration)
        .ReadFrom.Services(services)
        .Enrich.FromLogContext()
        .WriteTo.Console()
        .WriteTo.File("logs/app-.log", rollingInterval: RollingInterval.Day));

    builder.Services.AddRazorComponents()
        .AddInteractiveServerComponents();

    // The admin sign-in and sign-out endpoints are Razor Pages: SignInManager needs to write
    // the auth cookie to a real HTTP response, which a Blazor circuit cannot do.
    builder.Services.AddRazorPages();

    builder.Services.AddApplication();
    builder.Services.AddInfrastructure(builder.Configuration);

    builder.Services.AddAuthentication(options =>
    {
        options.DefaultScheme = IdentityConstants.ApplicationScheme;
        options.DefaultSignInScheme = IdentityConstants.ApplicationScheme;
    })
    .AddIdentityCookies();

    builder.Services.ConfigureApplicationCookie(options =>
    {
        options.LoginPath = "/admin/login";          // fallback only
        options.AccessDeniedPath = "/admin/login";   // fallback only
        options.ExpireTimeSpan = TimeSpan.FromHours(8);
        options.SlidingExpiration = true;

        // One cookie serves both audiences, so the challenge target depends on the path:
        // /admin/* goes to the admin sign-in, everything else to the customer sign-in.
        options.Events.OnRedirectToLogin = ctx =>
        {
            ctx.Response.Redirect(LoginTarget(ctx.Request));
            return Task.CompletedTask;
        };

        options.Events.OnRedirectToAccessDenied = ctx =>
        {
            ctx.Response.Redirect(LoginTarget(ctx.Request));
            return Task.CompletedTask;
        };

        static string LoginTarget(HttpRequest request) =>
            request.Path.StartsWithSegments("/admin")
                ? "/admin/login"
                : $"/dang-nhap?returnUrl={Uri.EscapeDataString(request.Path + request.QueryString)}";
    });

    builder.Services.AddAuthorization();
    builder.Services.AddCascadingAuthenticationState();
    builder.Services.AddHttpContextAccessor();

    // Per-circuit badge state for the admin sidebar.
    builder.Services.AddScoped<NewOrderCountState>();
    builder.Services.AddScoped<PendingReviewCountState>();

    // One write at a time on the circuit's shared write context (wishlist hearts, cart buttons, guest-cart merge).
    builder.Services.AddScoped<NieFarm.Web.Services.CircuitWriteGate>();

    // Per-circuit cart state — merges the guest cart into the account cart at sign-in and holds the
    // live item count for the header badge.
    builder.Services.AddScoped<CartState>();

    // R-16: per-circuit wishlist ids shared by every heart button; serialises the writes.
    builder.Services.AddScoped<NieFarm.Web.Services.Wishlist.WishlistState>();

    // R-23: periodic deletion of abandoned guest carts (starts after app.Run(), i.e. after seeding).
    builder.Services.Configure<CartCleanupOptions>(
        builder.Configuration.GetSection(CartCleanupOptions.SectionName));
    builder.Services.AddHostedService<AbandonedCartCleanupService>();

    // Circuit-scoped hand-off of the just-placed order to the success page (never queried by code).
    builder.Services.AddScoped<PlacedOrderState>();

    // Live payment status: one in-memory notifier is both the publisher the Application layer calls
    // and the subscribe-side the pages use (singleton, shared across circuits).
    builder.Services.AddSingleton<PaymentStatusNotifier>();
    builder.Services.AddSingleton<NieFarm.Application.Common.Interfaces.IPaymentStatusPublisher>(
        sp => sp.GetRequiredService<PaymentStatusNotifier>());

    // R-26 traceability: site address printed on QR codes, tuning knobs, and the proxy trust list.
    builder.Services.Configure<SiteOptions>(builder.Configuration.GetSection(SiteOptions.SectionName));
    builder.Services.Configure<TraceabilityOptions>(builder.Configuration.GetSection(TraceabilityOptions.SectionName));
    builder.Services.Configure<ReverseProxyOptions>(builder.Configuration.GetSection(ReverseProxyOptions.SectionName));
    builder.Services.AddSingleton<PublicUrls>();
    builder.Services.AddSingleton<TraceabilitySettingsProvider>();
    builder.Services.AddSingleton<BatchScanRecorder>();
    builder.Services.AddHostedService<BatchScanFlushService>();
    builder.Services.AddScoped<ScanAlertCountState>();

    // The per-minute limit of the public traceability pages is fixed at start-up (a change needs a restart);
    // every other Traceability value is re-read at use. An out-of-range value falls back to its default.
    var traceabilitySettings = TraceabilitySettings.Resolve(
        builder.Configuration.GetSection(TraceabilityOptions.SectionName).Get<TraceabilityOptions>()
            ?? new TraceabilityOptions(),
        (name, value, range, fallback) => Log.Warning(
            "Traceability:{Name} must be {Range}; got {Value}. Using {Default}.", name, range, value, fallback));

    // The SePay webhook is retried by SePay on any non-2xx, including 429, so the limit is generous.
    // Both policies are keyed by the client address, which is the real caller only when ReverseProxy is
    // configured (see UseForwardedHeaders below); otherwise, behind a proxy, every caller shares one bucket.
    builder.Services.AddRateLimiter(options =>
    {
        options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
        // Runs before the endpoint can opt out of the status-code-pages re-execute, so do it here and
        // give the 429 a body (SePay-style JSON), otherwise it would be re-executed through /not-found.
        // The public traceability pages get a short Vietnamese HTML answer instead.
        options.OnRejected = async (context, token) =>
        {
            if (TraceabilityRateLimit.IsTraceabilityRequest(context.HttpContext))
            {
                await TraceabilityRateLimit.WriteRejectedAsync(context, token);
                return;
            }

            var http = context.HttpContext;
            var feature = http.Features.Get<Microsoft.AspNetCore.Diagnostics.IStatusCodePagesFeature>();
            if (feature is not null)
                feature.Enabled = false;
            http.Response.StatusCode = StatusCodes.Status429TooManyRequests;
            http.Response.ContentType = "application/json";
            await http.Response.WriteAsync("{\"success\":false}", token);
        };
        options.AddTraceabilityPolicy(traceabilitySettings.RateLimitPerMinute);
        options.AddPolicy(SePayWebhookEndpoint.RateLimitPolicy, httpContext =>
            RateLimitPartition.GetFixedWindowLimiter(
                ClientIpKey.For(httpContext),
                _ => new FixedWindowRateLimiterOptions
                {
                    PermitLimit = 120,
                    Window = TimeSpan.FromMinutes(1),
                    QueueLimit = 0
                }));
    });

    // R-26: the batch-label PDFs draw with the embedded Be Vietnam Pro (PDFsharp finds fonts only through this resolver).
    PdfSharp.Fonts.GlobalFontSettings.FontResolver = new EmbeddedFontResolver();

    var app = builder.Build();

    // R-26 D17: trust X-Forwarded-For / -Proto only from the configured proxy ranges, and do it first so
    // everything after it (rate limiting, HTTPS redirection, scan counting) sees the real client.
    var forwarded = (builder.Configuration.GetSection(ReverseProxyOptions.SectionName).Get<ReverseProxyOptions>()
            ?? new ReverseProxyOptions())
        .ToForwardedHeadersOptions(message => Log.Warning("{Message}", message));
    if (forwarded is not null)
        app.UseForwardedHeaders(forwarded);
    else if (app.Environment.IsProduction())
        Log.Warning("ReverseProxy is not enabled. Behind Cloudflare or any proxy every caller then shares one rate-limit bucket " +
                    "and one scan-dedupe key: set ReverseProxy__Enabled and ReverseProxy__TrustedNetworks (see docs/production-checklist.md).");

    if (!app.Environment.IsDevelopment())
    {
        app.UseExceptionHandler("/Error", createScopeForErrors: true);
        app.UseHsts();
    }

    app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
    app.UseHttpsRedirection();

    // Before the cart cookie middleware so a 429 never carries a Set-Cookie (R-26 D16).
    app.UseRateLimiter();

    app.UseMiddleware<CartCookieMiddleware>();

    app.UseAuthentication();
    app.UseAuthorization();

    app.UseAntiforgery();

    // Admin rich-text editor image drops. Authorized separately from the Blazor circuit
    // because it is a plain multipart POST, not a component call.
    app.MapPost("/api/admin/upload-image",
        async (IFormFile file, string folder, IImageStorage imageStorage, CancellationToken ct) =>
        {
            if (file.Length > ManagedImages.MaxImageBytes)
                return Results.BadRequest(new { error = "File too large (max 5 MB)." });

            if (!ManagedImages.AllowedContentTypes.Contains(file.ContentType))
                return Results.BadRequest(new { error = "Unsupported file type." });

            // Never trust the caller's folder name: only known feature subfolders are allowed.
            var subfolder = folder switch
            {
                "products" => "products",
                "articles" => "articles",
                _ => null
            };

            if (subfolder is null)
                return Results.BadRequest(new { error = "Unknown upload folder." });

            await using var stream = file.OpenReadStream();
            var url = await imageStorage.SaveAsync(stream, file.FileName, subfolder, ct);

            return Results.Ok(new { url });
        })
        .RequireAuthorization(new AuthorizeAttribute { Roles = $"{AdminRoles.Admin},{AdminRoles.SuperAdmin}" })
        .DisableAntiforgery();

    app.MapSePayWebhook();
    app.MapRoastBatchLabelEndpoints();

    app.MapStaticAssets();
    app.MapRazorPages();
    app.MapRazorComponents<App>()
        .AddInteractiveServerRenderMode();

    try
    {
        await Seeder.SeedAsync(app.Services, builder.Configuration["Seed:SuperAdminPassword"]);
    }
    catch (Exception ex)
    {
        // Most of the public site still runs on fixtures, so a database that has not been
        // migrated yet must not take the whole app down. The admin will surface the same error
        // when a page queries, which is where it belongs.
        Log.Error(ex,
            "Could not seed identity or content data. If the database has not been created yet, run: " +
            "dotnet ef migrations add InitialCreate --project src/NieFarm.Infrastructure " +
            "--startup-project src/NieFarm.Web, then dotnet ef database update with the same arguments.");
    }

    app.Run();
}
catch (HostAbortedException)
{
    // Thrown intentionally by EF Core design-time tooling (e.g. `dotnet ef migrations add`)
    // right after the host is built, to stop before app.Run(). Not a real startup failure.
    throw;
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application start-up failed");
    throw;
}
finally
{
    Log.CloseAndFlush();
}
