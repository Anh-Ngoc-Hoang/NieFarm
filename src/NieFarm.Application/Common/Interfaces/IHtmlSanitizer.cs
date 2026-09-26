namespace NieFarm.Application.Common.Interfaces;

public interface IHtmlSanitizer
{
    /// <summary>
    /// Strips everything outside the editor's allow-list — scripts, event handlers, embedded
    /// frames and unknown attributes — and returns markup that is safe to store and to render
    /// through <c>MarkupString</c> later.
    ///
    /// Sanitise on the way in, not on the way out: rendering is the one place that must never
    /// have to trust a row written months earlier. Returns an empty string for null or blank
    /// input, and may legitimately return empty if the input was entirely disallowed.
    /// </summary>
    string Sanitize(string? html);
}
