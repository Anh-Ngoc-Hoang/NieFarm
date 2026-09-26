using Ganss.Xss;

// Ganss.Xss declares its own IHtmlSanitizer; the alias keeps it clear which one is implemented.
using IAppHtmlSanitizer = NieFarm.Application.Common.Interfaces.IHtmlSanitizer;

namespace NieFarm.Infrastructure.Common;

/// <summary>
/// Wraps Ganss.Xss behind the Application's interface, so the sanitiser package stays an
/// Infrastructure concern.
///
/// The allow-list mirrors the rich-text toolbar exactly — a button whose output is not listed
/// here loses its formatting on save, so the two must be changed together. Deliberately absent:
/// <c>img</c>, <c>iframe</c>, <c>video</c> and <c>script</c>. Product photography belongs to the
/// image fields, which validate content type and size.
/// </summary>
public class HtmlSanitizerAdapter : IAppHtmlSanitizer
{
    private static readonly HtmlSanitizer Sanitizer = Build();

    public string Sanitize(string? html)
        => string.IsNullOrWhiteSpace(html) ? string.Empty : Sanitizer.Sanitize(html);

    private static HtmlSanitizer Build()
    {
        var sanitizer = new HtmlSanitizer();

        sanitizer.AllowedTags.Clear();
        foreach (var tag in new[]
        {
            "p", "br", "strong", "b", "em", "i", "u", "s",
            "h2", "h3", "ul", "ol", "li", "blockquote", "span", "a"
        })
        {
            sanitizer.AllowedTags.Add(tag);
        }

        sanitizer.AllowedAttributes.Clear();
        foreach (var attribute in new[] { "href", "title", "target", "rel", "style", "class" })
            sanitizer.AllowedAttributes.Add(attribute);

        // Quill writes colours inline and alignment as a ql-align-* class; nothing else is kept.
        sanitizer.AllowedCssProperties.Clear();
        foreach (var property in new[] { "color", "background-color", "text-align" })
            sanitizer.AllowedCssProperties.Add(property);

        sanitizer.AllowedSchemes.Clear();
        sanitizer.AllowedSchemes.Add("http");
        sanitizer.AllowedSchemes.Add("https");
        sanitizer.AllowedSchemes.Add("mailto");

        sanitizer.AllowDataAttributes = false;

        // Anything opening a new tab must not hand the opener over with it.
        sanitizer.PostProcessNode += (_, e) =>
        {
            if (e.Node is AngleSharp.Html.Dom.IHtmlAnchorElement anchor
                && anchor.GetAttribute("target") is "_blank")
            {
                anchor.SetAttribute("rel", "noopener noreferrer");
            }
        };

        return sanitizer;
    }
}
