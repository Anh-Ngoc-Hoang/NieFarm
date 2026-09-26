using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NieFarm.Infrastructure.Data.Seed;
using NieFarm.Infrastructure.Identity;

namespace NieFarm.Infrastructure.Data;

public static class Seeder
{
    /// <summary>
    /// Ensures the admin roles, a first SuperAdmin, and starter content all exist. Idempotent —
    /// safe on every start. Each step guards itself, so identity already existing does not skip
    /// content seeding (and vice versa).
    /// </summary>
    public static async Task SeedAsync(IServiceProvider services, string? superAdminPassword)
    {
        using var scope = services.CreateScope();

        // Sequential — both steps use the same scoped AppDbContext (data-access.md).
        await SeedIdentityAsync(scope, superAdminPassword);
        await SeedActivitiesAsync(scope);
        await SeedProductsAsync(scope);
    }

    /// <summary>
    /// Creates the admin roles and, if absent, the first SuperAdmin. The bootstrap password is
    /// read from configuration ("Seed:SuperAdminPassword"); without it no account is created, so
    /// a deployment can never accidentally ship a known password.
    /// </summary>
    private static async Task SeedIdentityAsync(IServiceScope scope, string? superAdminPassword)
    {
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var logger = scope.ServiceProvider.GetRequiredService<ILoggerFactory>().CreateLogger(typeof(Seeder));

        foreach (var role in AdminRoles.All)
        {
            if (!await roleManager.RoleExistsAsync(role))
                await roleManager.CreateAsync(new IdentityRole(role));
        }

        const string superAdminEmail = "superadmin@niefarm.vn";

        if (await userManager.FindByEmailAsync(superAdminEmail) is not null)
            return;

        if (string.IsNullOrWhiteSpace(superAdminPassword))
        {
            logger.LogWarning(
                "No SuperAdmin account exists and Seed:SuperAdminPassword is not configured, " +
                "so none was created. Set it in appsettings.Development.json or an environment " +
                "variable, then restart.");
            return;
        }

        var superAdmin = new ApplicationUser
        {
            UserName = superAdminEmail,
            Email = superAdminEmail,
            FullName = "Super Administrator",
            EmailConfirmed = true
        };

        var created = await userManager.CreateAsync(superAdmin, superAdminPassword);
        if (!created.Succeeded)
        {
            logger.LogError(
                "Failed to create the seed SuperAdmin: {Errors}",
                string.Join("; ", created.Errors.Select(e => e.Description)));
            return;
        }

        await userManager.AddToRoleAsync(superAdmin, AdminRoles.SuperAdmin);
        logger.LogInformation("Seeded SuperAdmin account {Email}", superAdminEmail);
    }

    /// <summary>
    /// Populates the Activities table with the original design fixture the first time it is
    /// empty. "Table is empty" is the idempotency guard — an admin who deletes every activity on
    /// purpose will get them re-seeded on the next restart, which is acceptable and matches how
    /// the identity seeding treats "does this row already exist".
    /// </summary>
    private static async Task SeedActivitiesAsync(IServiceScope scope)
    {
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var logger = scope.ServiceProvider.GetRequiredService<ILoggerFactory>().CreateLogger(typeof(Seeder));

        if (await db.Activities.AnyAsync())
            return;

        var activities = ActivitySeedData.Build();
        db.Activities.AddRange(activities);
        await db.SaveChangesAsync();

        logger.LogInformation("Seeded {Count} activities", activities.Count);
    }

    /// <summary>
    /// Populates the Products table with the original design fixture (the /cua-hang cards plus
    /// the "Sản phẩm liên quan" band), one missing slug at a time — unlike
    /// <see cref="SeedActivitiesAsync"/>'s whole-table-empty guard, because
    /// <see cref="ProductSeedData"/> has already grown once after an earlier partial seed ran,
    /// and an admin may also have created real products before every fixture slug has landed.
    /// Any category the fixture needs that does not already exist (matched by name) is created
    /// alongside it, since a Product cannot be saved without a CategoryId — this is the one seed
    /// step with a same-request dependency, unlike Activities.
    ///
    /// This is per-slug idempotent, never an upsert: on a database where these slugs already
    /// exist (from an earlier seed run predating specs/features/options/gallery/OldPrice), this
    /// step will not backfill the newer showcase content onto <c>robusta-honey-dac-biet</c> —
    /// that has to happen through <c>/admin/products</c> or a one-off manual backfill instead.
    /// </summary>
    private static async Task SeedProductsAsync(IServiceScope scope)
    {
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var logger = scope.ServiceProvider.GetRequiredService<ILoggerFactory>().CreateLogger(typeof(Seeder));

        string[] neededCategories = ["Robusta", "Arabica", "Blend", "Tiện lợi", "Cold Brew"];
        var categoryIdByName = new Dictionary<string, int>();

        var sortOrder = await db.ProductCategories.CountAsync();
        foreach (var name in neededCategories)
        {
            var category = await db.ProductCategories.FirstOrDefaultAsync(c => c.Name == name);
            if (category is null)
            {
                category = Domain.Entities.ProductCategory.Create(name, name, isVisible: true, sortOrder++);
                db.ProductCategories.Add(category);
                await db.SaveChangesAsync();
                logger.LogInformation("Seeded product category {Name}", name);
            }

            categoryIdByName[name] = category.Id;
        }

        var existingSlugs = await db.Products.Select(p => p.Slug).ToListAsync();
        var missing = ProductSeedData.Build(categoryIdByName)
            .Where(p => !existingSlugs.Contains(p.Slug))
            .ToList();

        if (missing.Count == 0)
            return;

        db.Products.AddRange(missing);
        await db.SaveChangesAsync();

        logger.LogInformation("Seeded {Count} products", missing.Count);
    }
}
