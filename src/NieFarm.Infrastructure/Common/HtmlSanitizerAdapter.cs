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
///
/// Quill 2 writes bullet lists as <c>&lt;ol&gt;&lt;li data-list="bullet"&gt;</c> with a
/// <c>span.ql-ui</c> marker. <c>PostProcessDom</c> rewrites those into semantic <c>ul</c>/<c>ol</c>
/// runs and drops the markers, so stored HTML reloads into the editor with the right list types.
/// The result is idempotent: sanitising already-sanitised markup returns it unchanged.
/// <c>class</c> is restricted to Quill's own alignment/indent classes, so an admin cannot attach
/// classes that site scripts bind to by global selector.
/// </summary>
public class HtmlSanitizerAdapter : IAppHtmlSanitizer
{
    private static readonly HtmlSanitizer Sanitizer = Build();

    public string Sanitize(string? html)
        => string.IsNullOrWhiteSpace(html) ? string.Empty : Sanitizer.Sanitize(NormaliseStructure(html));

    private static readonly HashSet<string> BlockTags = new(StringComparer.Ordinal)
    {
        "p", "ul", "ol", "li", "blockquote", "h1", "h2", "h3", "h4", "h5", "h6", "div", "section",
        "article", "header", "footer", "main", "center", "table", "hr"
    };

    /// <summary>
    /// Runs before the allow-list. Ganss removes a forbidden tag together with all its children, so
    /// legacy bodies typed into a plain textarea and wrapped in a harmless structural tag would lose
    /// all their text. Only these are rescued: <c>div/section/article/header/footer/main/center</c>
    /// become a <c>p</c> when they hold only inline content and are unwrapped when they hold blocks;
    /// <c>pre</c> is treated as an inline container (its text survives as a <c>p</c>; <c>code</c>/<c>kbd</c>/<c>samp</c>
    /// inside are unwrapped so their text is not dropped). The <c>style</c> and <c>class</c> of a div turned
    /// into a <c>p</c> are copied across so the allow-list can keep e.g. <c>text-align:center</c>.
    /// <c>font</c> is unwrapped; <c>h1</c> becomes <c>h2</c> and <c>h4-h6</c> become <c>h3</c>.
    /// Everything else (script, style, iframe, object, embed, form, svg, table ...) is still removed
    /// with its content by the sanitiser (tables lose their text on purpose: no toolbar button
    /// produces them). Idempotent, because the output contains none of these tags.
    /// </summary>
    private static string NormaliseStructure(string html)
    {
        var document = new AngleSharp.Html.Parser.HtmlParser().ParseDocument("<!DOCTYPE html><body>" + html);
        var body = document.Body;
        if (body is null)
            return html;

        // Reverse document order: descendants are handled before their ancestors.
        var targets = body.QuerySelectorAll("div,section,article,header,footer,main,center,font,code,kbd,samp,pre,h1,h4,h5,h6")
            .Reverse()
            .ToList();

        foreach (var element in targets)
        {
            var parent = element.Parent;
            if (parent is null)
                continue;

            var name = element.LocalName;
            string? rename = name switch
            {
                "h1" => "h2",
                "h4" or "h5" or "h6" => "h3",
                "font" or "code" or "kbd" or "samp" => null,
                _ => element.Children.Any(c => BlockTags.Contains(c.LocalName)) ? null : "p"
            };

            if (rename is null)
            {
                foreach (var child in element.ChildNodes.ToList())
                    parent.InsertBefore(child, element);
            }
            else if (element.HasChildNodes)
            {
                var replacement = document.CreateElement(rename);
                foreach (var child in element.ChildNodes.ToList())
                    replacement.AppendChild(child);
                foreach (var attribute in new[] { "style", "class" })
                {
                    var value = element.GetAttribute(attribute);
                    if (value is not null && name is not ("h1" or "h4" or "h5" or "h6"))
                        replacement.SetAttribute(attribute, value);
                }
                parent.InsertBefore(replacement, element);
            }

            element.Remove();
        }

        return body.InnerHtml;
    }

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
        foreach (var attribute in new[] { "href", "title", "target", "rel", "style", "class", "data-list" })
            sanitizer.AllowedAttributes.Add(attribute);

        // Quill writes colours inline and alignment as a ql-align-* class; nothing else is kept.
        sanitizer.AllowedCssProperties.Clear();
        foreach (var property in new[] { "color", "background-color", "text-align" })
            sanitizer.AllowedCssProperties.Add(property);

        sanitizer.AllowedClasses.Clear();
        foreach (var cls in new[] { "ql-align-center", "ql-align-right", "ql-align-justify" })
            sanitizer.AllowedClasses.Add(cls);
        for (var i = 1; i <= 8; i++)
            sanitizer.AllowedClasses.Add($"ql-indent-{i}");

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

        sanitizer.PostProcessDom += (_, e) =>
        {
            NormaliseLists(e.Document);
            RemoveEmptyParagraphs(e.Document);
        };

        return sanitizer;
    }

    /// <summary>
    /// Drops <c>&lt;p&gt;&lt;/p&gt;</c> left behind when the sanitiser removed all of a paragraph's content.
    /// A paragraph that still holds a <c>br</c> (Quill's blank line, <c>&lt;p&gt;&lt;br&gt;&lt;/p&gt;</c>) or any
    /// text is kept.
    /// </summary>
    private static void RemoveEmptyParagraphs(AngleSharp.Dom.IDocument document)
    {
        if (document.Body is null)
            return;
        foreach (var p in document.Body.QuerySelectorAll("p").ToList())
        {
            if (p.QuerySelector("br") is null
                && (p.TextContent ?? string.Empty).Trim(' ', '\t', '\r', '\n').Length == 0)
                p.Remove();
        }
    }

    /// <summary>
    /// Quill 2 stores nested lists FLAT: one <c>ol</c> whose <c>li</c> carry <c>data-list</c>
    /// (bullet/ordered) and <c>ql-indent-N</c>. This rebuilds real nesting: an item indented deeper
    /// than the previous one opens a child <c>ul</c>/<c>ol</c> INSIDE the previous <c>li</c>; an item
    /// at the same level joins the current list when the type matches and starts a sibling list when
    /// it does not (so numbering restarts only where the type really changes, never because a
    /// deeper bullet sat in between). <c>data-list</c> and the indent classes are dropped afterwards
    /// since the nesting now carries them, together with the empty marker spans left by
    /// <c>span.ql-ui</c>. Already-nested output has neither attribute and is left alone, which makes
    /// the whole thing idempotent.
    /// Levels are clamped to at most one deeper than the previous item (and the first item to 0), so a
    /// jump <c>[0,2,1]</c> yields one nested list instead of two sibling lists in one <c>li</c>.
    /// Known limit: a first item that is already indented (<c>[2,0]</c>) loses that indent, since a
    /// nested list needs a host <c>li</c> to hang in.
    /// </summary>
    private static void NormaliseLists(AngleSharp.Dom.IDocument document)
    {
        var body = document.Body;
        if (body is null)
            return;

        foreach (var list in body.QuerySelectorAll("ol").ToList())
        {
            var items = list.Children.ToList();
            if (items.Count == 0
                || items.Any(i => !i.LocalName.Equals("li", StringComparison.Ordinal))
                || !items.Any(i => i.HasAttribute("data-list") || IndentOf(i) > 0))
                continue;

            var roots = new List<AngleSharp.Dom.IElement>();
            // Level, bullet, the list element, and the li it hangs in (null for a top-level list).
            var stack = new List<(int Level, bool Bullet, AngleSharp.Dom.IElement List, AngleSharp.Dom.IElement? Host)>();

            var previousLevel = -1;
            foreach (var item in items)
            {
                var level = Math.Min(IndentOf(item), previousLevel + 1);
                previousLevel = level;
                var bullet = item.GetAttribute("data-list") == "bullet";

                while (stack.Count > 0 && stack[^1].Level > level)
                    stack.RemoveAt(stack.Count - 1);

                if (stack.Count > 0 && stack[^1].Level < level)
                {
                    var host = stack[^1].List.Children.LastOrDefault();
                    var child = document.CreateElement(bullet ? "ul" : "ol");
                    if (host is null) { stack.Clear(); }
                    else
                    {
                        host.AppendChild(child);
                        stack.Add((level, bullet, child, host));
                    }
                }

                if (stack.Count > 0 && stack[^1].Bullet != bullet)
                {
                    var (lvl, _, _, host) = stack[^1];
                    stack.RemoveAt(stack.Count - 1);
                    var sibling = document.CreateElement(bullet ? "ul" : "ol");
                    if (host is null) roots.Add(sibling); else host.AppendChild(sibling);
                    stack.Add((lvl, bullet, sibling, host));
                }

                if (stack.Count == 0)
                {
                    var root = document.CreateElement(bullet ? "ul" : "ol");
                    roots.Add(root);
                    stack.Add((level, bullet, root, null));
                }

                stack[^1].List.AppendChild(item);
            }

            foreach (var root in roots)
                list.Parent?.InsertBefore(root, list);

            list.Remove();
        }

        foreach (var tagged in body.QuerySelectorAll("[data-list]"))
            tagged.RemoveAttribute("data-list");

        foreach (var li in body.QuerySelectorAll("li[class]"))
        {
            foreach (var cls in li.ClassList.Where(c => c.StartsWith("ql-indent-", StringComparison.Ordinal)).ToList())
                li.ClassList.Remove(cls);
            if (li.ClassList.Length == 0)
                li.RemoveAttribute("class");
        }

        foreach (var marker in body.QuerySelectorAll("li > span").ToList())
        {
            if (!marker.HasChildNodes && marker.Attributes.Length == 0)
                marker.Remove();
        }
    }

    private static int IndentOf(AngleSharp.Dom.IElement item)
    {
        foreach (var cls in item.ClassList)
        {
            if (cls.StartsWith("ql-indent-", StringComparison.Ordinal)
                && int.TryParse(cls.AsSpan("ql-indent-".Length), out var n))
                return n;
        }
        return 0;
    }
}
