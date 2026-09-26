using Ardalis.Specification;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using NieFarm.Application.Common.Interfaces;
using NieFarm.Infrastructure.Data;
using NieFarm.Infrastructure.Data.Repositories;
using NieFarm.Infrastructure.Identity;
using NieFarm.Infrastructure.Services;
using NieFarm.Infrastructure.Services.Addresses;

namespace NieFarm.Infrastructure.Common;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection");

        // Shared scoped context — backs the read-write repository.
        services.AddDbContext<AppDbContext>(options =>
            options.UseSqlServer(
                connectionString,
                sql => sql.MigrationsAssembly(typeof(AppDbContext).Assembly.FullName)));

        // Factory — backs ReadOnlyRepository, one fresh context per instance.
        services.AddDbContextFactory<AppDbContext>(options =>
            options.UseSqlServer(connectionString),
            ServiceLifetime.Scoped);

        services.AddIdentityCore<ApplicationUser>(options =>
        {
            options.Password.RequiredLength = 8;
            options.Password.RequireNonAlphanumeric = false;
            options.User.RequireUniqueEmail = true;
            options.SignIn.RequireConfirmedEmail = false;
        })
        .AddRoles<IdentityRole>()
        .AddEntityFrameworkStores<AppDbContext>()
        .AddSignInManager()
        .AddClaimsPrincipalFactory<AppUserClaimsPrincipalFactory>()
        .AddDefaultTokenProviders();

        services.AddScoped(typeof(IRepositoryBase<>), typeof(Repository<>));
        services.AddTransient(typeof(IReadRepositoryBase<>), typeof(ReadOnlyRepository<>));

        services.AddScoped<IIdentityService, IdentityService>();
        services.AddScoped<ICurrentUser, CurrentUser>();
        services.AddScoped<IImageStorage, LocalFileImageStorage>();
        services.AddSingleton<ISlugGenerator, SlugGenerator>();
        services.AddSingleton<IHtmlSanitizer, HtmlSanitizerAdapter>();

        services.AddMemoryCache();   // TryAdd-based; safe even if something else already registered it

        services.Configure<AddressLookupOptions>(
            configuration.GetSection(AddressLookupOptions.SectionName));

        services.AddHttpClient(AddressLookupOptions.SectionName, (sp, client) =>
        {
            var opts = sp.GetRequiredService<IOptions<AddressLookupOptions>>().Value;
            client.BaseAddress = new Uri(opts.BaseUrl);          // must end in "/" for relative paths to work
            client.Timeout = TimeSpan.FromSeconds(opts.TimeoutSeconds);
            client.DefaultRequestHeaders.UserAgent.ParseAdd("NieFarm/1.0");
        });

        // Singleton so the de-duplication gate and the last-known-good snapshots are shared process-wide.
        services.AddSingleton<IAddressLookupService, ProvincesOpenApiAddressLookupService>();

        return services;
    }
}
