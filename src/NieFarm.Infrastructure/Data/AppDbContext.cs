using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using NieFarm.Domain.Entities;
using NieFarm.Infrastructure.Identity;

namespace NieFarm.Infrastructure.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options)
    : IdentityDbContext<ApplicationUser>(options)
{
    public DbSet<ProductCategory> ProductCategories => Set<ProductCategory>();
    public DbSet<Product> Products => Set<Product>();
    public DbSet<ArticleCategory> ArticleCategories => Set<ArticleCategory>();
    public DbSet<Article> Articles => Set<Article>();
    public DbSet<Activity> Activities => Set<Activity>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderItem> OrderItems => Set<OrderItem>();
    public DbSet<OrderStatusChange> OrderStatusChanges => Set<OrderStatusChange>();
    public DbSet<OrderPaymentChange> OrderPaymentChanges => Set<OrderPaymentChange>();
    public DbSet<GatewayTransaction> GatewayTransactions => Set<GatewayTransaction>();
    public DbSet<Cart> Carts => Set<Cart>();
    public DbSet<CartItem> CartItems => Set<CartItem>();
    public DbSet<Review> Reviews => Set<Review>();
    public DbSet<Testimonial> Testimonials => Set<Testimonial>();
    public DbSet<Crop> Crops => Set<Crop>();
    public DbSet<CropProduct> CropProducts => Set<CropProduct>();
    public DbSet<CoffeeRegion> CoffeeRegions => Set<CoffeeRegion>();
    public DbSet<CoffeeRegionArea> CoffeeRegionAreas => Set<CoffeeRegionArea>();
    public DbSet<Partner> Partners => Set<Partner>();
    public DbSet<ShippingSettings> ShippingSettings => Set<ShippingSettings>();
    public DbSet<ShippingZone> ShippingZones => Set<ShippingZone>();
    public DbSet<ShippingZoneProvince> ShippingZoneProvinces => Set<ShippingZoneProvince>();
    public DbSet<Coupon> Coupons => Set<Coupon>();
    public DbSet<CustomerAddress> CustomerAddresses => Set<CustomerAddress>();
    public DbSet<WishlistItem> WishlistItems => Set<WishlistItem>();
    public DbSet<RoastBatch> RoastBatches => Set<RoastBatch>();
    public DbSet<RoastBatchCertification> RoastBatchCertifications => Set<RoastBatchCertification>();
    public DbSet<RoastBatchBrewGuide> RoastBatchBrewGuides => Set<RoastBatchBrewGuide>();
    public DbSet<RoastBatchScanStat> RoastBatchScanStats => Set<RoastBatchScanStat>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);

        base.OnModelCreating(modelBuilder);
    }
}
