namespace NieFarm.Application.Features.Carts.Dtos;

/// <summary>Outcome of one abandoned-cart cleanup run. Carries counts only, never cart data.</summary>
public sealed record CartPurgeResult(int CartsDeleted, int ItemsDeleted, int Batches, bool MoreRemaining);
