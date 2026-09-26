using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using NieFarm.Application.Common;
using NieFarm.Application.Common.Images;
using NieFarm.Application.Common.Interfaces;
using NieFarm.Infrastructure.Common;
using NieFarm.Infrastructure.Data;
using NieFarm.Infrastructure.Identity;
using NieFarm.Web.Components;
using NieFarm.Web.Middleware;
using NieFarm.Web.Services.Admin;
using NieFarm.Web.Services.Cart;
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

    // Per-circuit cart state — merges the guest cart, seeds the demo cart and holds the live
    // item count for the header badge.
    builder.Services.AddScoped<CartState>();

    var app = builder.Build();

    if (!app.Environment.IsDevelopment())
    {
        app.UseExceptionHandler("/Error", createScopeForErrors: true);
        app.UseHsts();
    }

    app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
    app.UseHttpsRedirection();

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
