namespace NieFarm.Application.Common.Interfaces;

public interface IHtmlSanitizer
{
    /// <summary>
    /// Strips everything outside the editor's allow-list — scripts, event handlers, embedded
    /// frames and unknown attributes — and returns markup that is safe to store and to render
    /// through <c>MarkupString</c> later.
    ///
    /// Sanitise on the way in: rendering must never have to trust a row written months earlier.
    /// The deliberate exception is article bodies, which are sanitised again on read (the public
    /// detail page and the admin edit form) because older rows were saved through a raw textarea
    /// before any sanitiser ran; the function is idempotent, so this is cheap and safe. Returns an empty string for null or blank
    /// input, and may legitimately return empty if the input was entirely disallowed.
    /// </summary>
    string Sanitize(string? html);
}
