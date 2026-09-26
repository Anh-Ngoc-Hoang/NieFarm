namespace NieFarm.Application.Features.Admin.Dashboard.Dtos;

public record DashboardStatsDto(
    int ProductCount,
    int ArticleCount,
    int OrderCount,
    int NewOrderCount,
    decimal CompletedRevenue,
    List<DashboardOrderRow> RecentOrders);

public record DashboardOrderRow(
    int Id,
    string Code,
    string CustomerName,
    string Status,
    decimal Total,
    DateTime PlacedAt);
