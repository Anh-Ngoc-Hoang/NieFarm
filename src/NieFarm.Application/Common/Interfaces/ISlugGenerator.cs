namespace NieFarm.Application.Common.Interfaces;

/// <summary>
/// Turns Vietnamese titles into URL-safe slugs. Diacritics are folded to ASCII so
/// "Cà phê Drip Bag" becomes "ca-phe-drip-bag".
/// </summary>
public interface ISlugGenerator
{
    string Generate(string text);
}
