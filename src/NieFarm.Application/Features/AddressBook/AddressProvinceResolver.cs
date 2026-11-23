using NieFarm.Application.Features.Shipping;

namespace NieFarm.Application.Features.AddressBook;

/// <summary>
/// Normalises the province of a saved address through <see cref="VietnamProvinceCatalog"/> (R-16 D-13).
/// A code, when given, must be a catalog code and the catalog supplies the name; with no code (the
/// manual-entry mode used when the lookup API is down) the typed name must resolve. Stricter than
/// checkout on purpose: an address book is long-lived data, so a typo must not be saved.
/// </summary>
internal static class AddressProvinceResolver
{
    public static (int Code, string Name)? Resolve(int? code, string? name)
    {
        if (code is { } given)
        {
            return VietnamProvinceCatalog.Find(given) is { } entry ? (entry.Code, entry.Name) : null;
        }

        if (VietnamProvinceCatalog.TryResolve(name) is { } resolved
            && VietnamProvinceCatalog.Find(resolved) is { } found)
        {
            return (found.Code, found.Name);
        }

        return null;
    }
}
