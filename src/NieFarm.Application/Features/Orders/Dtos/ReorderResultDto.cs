namespace NieFarm.Application.Features.Orders.Dtos;

/// <summary>Why one line of an old order could not be put back in the cart by "buy again".</summary>
public enum ReorderSkipReason
{
    /// <summary>The product was deleted, hidden, or the order line never pointed at a product.</summary>
    NoLongerSold = 1,

    /// <summary>The option labels the line was bought with no longer resolve to a variant of the product.</summary>
    OptionsChanged = 2,

    /// <summary>The variant still exists but is currently marked unavailable.</summary>
    Unavailable = 3,

    /// <summary>The cart already holds the maximum number of distinct lines.</summary>
    CartFull = 4
}

/// <summary>
/// One order line that was left out. <paramref name="ProductName"/> is the order's own snapshot text
/// (name plus the variant label), never a value typed by the caller; render it as plain text.
/// </summary>
public record ReorderSkippedLineDto(string ProductName, ReorderSkipReason Reason);

/// <summary>
/// Outcome of "buy again". <paramref name="AddedLines"/> is how many order lines were put into the cart,
/// <paramref name="CartItemCount"/> the cart's total quantity afterwards (what the header badge shows).
/// No price travels here: the cart prices itself from the catalog.
/// </summary>
public record ReorderResultDto(int AddedLines, int CartItemCount, IReadOnlyList<ReorderSkippedLineDto> Skipped);
