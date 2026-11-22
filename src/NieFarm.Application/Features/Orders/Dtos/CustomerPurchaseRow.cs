namespace NieFarm.Application.Features.Orders.Dtos;

/// <summary>
/// Internal projection row: one Completed order of <paramref name="CustomerId"/> and the subset of its
/// line product ids that was asked about. Never leaves the Application layer — see
/// <see cref="NieFarm.Application.Features.Reviews.VerifiedPurchases"/>.
/// </summary>
public record CustomerPurchaseRow(string CustomerId, List<int> ProductIds);
