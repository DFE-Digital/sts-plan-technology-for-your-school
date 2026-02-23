using System.Text;
using System.Text.RegularExpressions;
using Dfe.PlanTech.Core.Contentful.Models;

namespace Dfe.PlanTech.Application.Rendering.Markdown;

public class MarkdownRenderer
{
    // Contentful has some unusual non-breaking space characters
    private static readonly Regex NonBreakingSpaceRegex = new(
        "[\u00A0\u2007\u202F]",
        RegexOptions.Compiled,
        TimeSpan.FromMilliseconds(200)
    );

    // Whitespace could be spaces or tabs
    private static readonly Regex Whitespace = new(
        "(?<!^)(?<!\r?\n)[ \t]{2,}",
        RegexOptions.Compiled,
        TimeSpan.FromMilliseconds(200)
    );

    private readonly RenderContext _renderContext = new();

    private readonly List<string> _lines = [];

    private readonly List<string> _knownLevelNodeTypes =
    [
        "blockquote",
        "document",
        "heading-1",
        "heading-2",
        "heading-3",
        "heading-4",
        "heading-5",
        "heading-6",
        "hr",
        "list-item",
        "ordered-list",
        "paragraph",
        "unordered-list",
    ];

    private readonly List<string> _knownInlineNodeTypes =
    [
        "asset-hyperlink",
        "entry-hyperlink",
        "hyperlink",
        "line-break",
        "text",
    ];

    public static string Render(RichTextContentField textBody)
    {
        return new MarkdownRenderer().RenderText(textBody);
    }

    public string RenderText(RichTextContentField textBody)
    {
        RenderNode(textBody);

        // Remove multiple trailing blank lines, but ensure at least one blank line at the end if there is any field.
        while (_lines.Count > 0 && _lines[^1] == "")
            _lines.RemoveAt(_lines.Count - 1);

        if (_lines.Count == 0)
            return "";

        return string.Join(Environment.NewLine, _lines) + Environment.NewLine;
    }

    private void RenderNode(RichTextContentField field)
    {
        if (!_knownLevelNodeTypes.Contains(field.NodeType))
        {
            // Unknown / unsupported block field: best-effort render children
            RenderChildren(field);
            return;
        }

        if (field.NodeType.StartsWith("heading"))
        {
            RenderHeading(field);
            return;
        }

        switch (field.NodeType)
        {
            case "blockquote":
                RenderBlockQuote(field);
                break;
            case "document":
                RenderChildren(field);
                break;
            case "hr":
                EmitBlock(["---"]);
                break;
            case "ordered-list":
                RenderOrderedList(field);
                break;
            case "paragraph":
                RenderParagraph(field);
                break;
            case "unordered-list":
                RenderUnorderedList(field);
                break;
        }
        ;
    }

    private void RenderBlockQuote(RichTextContentField field)
    {
        var innerLines = RenderBlockChildrenToLines(field.Content)
            .Select(CleanText)
            .Where(s => !string.IsNullOrEmpty(s))
            .Select(ln => $"> {ln}")
            .ToList();

        if (innerLines.Count != 0)
        {
            EmitBlock(innerLines);
        }
    }

    private void RenderChildren(RichTextContentField field)
    {
        foreach (var child in field.Content)
            RenderNode(child);
    }

    private void RenderHeading(RichTextContentField field)
    {
        var level = 2;
        var hyphenIndex = field.NodeType.IndexOf('-', StringComparison.Ordinal);

        var hasHyphenatedLevel = hyphenIndex >= 0 && hyphenIndex != field.NodeType.Length - 1;
        if (hasHyphenatedLevel)
            int.TryParse(field.NodeType[(hyphenIndex + 1)..], out level);

        var hashes = level == 1 ? "#" : "##";
        var text = CleanText(RenderInlines(field));
        if (!string.IsNullOrEmpty(text))
            EmitBlock([$"{hashes} {text}"]);
    }

    private void RenderOrderedList(RichTextContentField field)
    {
        _renderContext.ListStack.Add("ol");
        _renderContext.ListNestingLevels.Add(0);

        var block = new List<string>();
        foreach (var item in field.Content)
            block.AddRange(RenderListItem(item, ordered: true));

        _renderContext.ListStack.RemoveAt(_renderContext.ListStack.Count - 1);
        _renderContext.ListNestingLevels.RemoveAt(_renderContext.ListNestingLevels.Count - 1);

        if (block.Count > 0)
            EmitBlock(block);
    }

    private void RenderParagraph(RichTextContentField field)
    {
        var text = CleanText(RenderInlines(field));
        if (!string.IsNullOrEmpty(text))
            EmitBlock([text]);
        return;
    }

    private void RenderUnorderedList(RichTextContentField field)
    {
        _renderContext.ListStack.Add("ul");
        _renderContext.ListNestingLevels.Add(0);

        var block = new List<string>();
        foreach (var item in field.Content)
        {
            block.AddRange(RenderListItem(item, ordered: false));
        }

        _renderContext.ListStack.RemoveAt(_renderContext.ListStack.Count - 1);
        _renderContext.ListNestingLevels.RemoveAt(_renderContext.ListNestingLevels.Count - 1);

        if (block.Count > 0)
        {
            EmitBlock(block);
        }
    }

    private static string CleanText(string s)
    {
        // NOTE: We intentionally do NOT trim, as whitespace must be preserved.

        if (string.IsNullOrEmpty(s))
            return "";

        s = NonBreakingSpaceRegex.Replace(s, " ");
        s = Whitespace.Replace(s, " ");

        return s;
    }

    private void EmitBlock(IEnumerable<string> blockLines)
    {
        while (_lines.Count > 0 && _lines[^1] == "")
            _lines.RemoveAt(_lines.Count - 1);

        if (_lines.Count > 0)
            _lines.Add("");

        _lines.AddRange(blockLines);
    }

    private List<string> RenderBlockChildrenToLines(IEnumerable<RichTextContentField> children)
    {
        var temporaryRenderer = new MarkdownRenderer();
        temporaryRenderer._renderContext.ListStack.AddRange(_renderContext.ListStack);
        temporaryRenderer._renderContext.ListNestingLevels.AddRange(
            _renderContext.ListNestingLevels
        );

        foreach (var child in children)
            temporaryRenderer.RenderNode(child);

        return temporaryRenderer._lines.Where(ln => ln is not null).ToList();
    }

    private List<string> RenderListItem(RichTextContentField field, bool ordered)
    {
        if (field.NodeType != "list-item")
            return [];

        string bullet;
        if (ordered)
        {
            _renderContext.ListNestingLevels[^1] = _renderContext.ListNestingLevels[^1] + 1;
            bullet = $"{_renderContext.ListNestingLevels[^1]}.";
        }
        else
        {
            bullet = "*";
        }

        var indentLevel = Math.Max(_renderContext.ListStack.Count - 1, 0);
        var indent = new string(' ', indentLevel * 2);

        string? firstText = null;
        var trailingBlocks = new List<string>();

        foreach (var content in field.Content)
        {
            if (content.NodeType == "paragraph")
            {
                var text = CleanText(RenderInlines(content));
                if (!string.IsNullOrEmpty(text))
                {
                    if (firstText == null)
                        firstText = text;
                    else
                        trailingBlocks.Add(text);
                }
            }
            else if (content.NodeType is "unordered-list" or "ordered-list")
            {
                trailingBlocks.AddRange(RenderListAsLines(content));
            }
            else
            {
                trailingBlocks.AddRange(
                    RenderBlockChildrenToLines([content])
                        .Where(x => !string.IsNullOrEmpty(CleanText(x)))
                );
            }
        }

        firstText ??= "";

        var lines = new List<string> { $"{indent}{bullet} {firstText}" };

        foreach (var block in trailingBlocks)
        {
            var cleaned = CleanText(block);
            if (string.IsNullOrEmpty(cleaned))
                continue;
            lines.Add($"{indent}{cleaned}");
        }

        return lines;
    }

    private string RenderInlines(RichTextContentField field)
    {
        var stringBuilder = new StringBuilder();

        foreach (var content in field.Content)
        {
            if (!_knownInlineNodeTypes.Contains(content.NodeType))
            {
                // Unknown / unsupported block field: best-effort render children
                stringBuilder.Append(CleanText(RenderInlines(content)));
                continue;
            }

            switch (content.NodeType)
            {
                case "text":
                    stringBuilder.Append(CleanText(content.Value));
                    break;
                case "hyperlink":
                    RenderHyperlink(stringBuilder, content);
                    break;
                case "asset-hyperlink":
                case "entry-hyperlink":
                    stringBuilder.Append(CleanText(RenderInlines(content)));
                    break;
                default:
                    stringBuilder.Append(Environment.NewLine);
                    break;
            }
        }

        var joined = stringBuilder.ToString();

        // Trim spaces from ends of lines
        joined = Regex.Replace(
            joined,
            "[ \t]+\\r?\n$",
            Environment.NewLine,
            RegexOptions.Compiled,
            TimeSpan.FromMilliseconds(200)
        );
        joined = Regex.Replace(
            joined,
            "\\r?\n[ \t]+",
            Environment.NewLine,
            RegexOptions.Compiled,
            TimeSpan.FromMilliseconds(200)
        );
        joined = Whitespace.Replace(joined, " ");
        return joined;
    }

    private void RenderHyperlink(StringBuilder sb, RichTextContentField field)
    {
        var text = CleanText(RenderInlines(field));
        var uri = CleanText(field.Data?.Uri ?? "");

        if (uri.StartsWith('/'))
        {
            uri = $"https://plan-technology-for-your-school.education.gov.uk{uri}";
        }

        if (!string.IsNullOrEmpty(text) && !string.IsNullOrEmpty(uri))
            sb.Append($"[{text}]({uri})");
        else if (!string.IsNullOrEmpty(uri))
            sb.Append(uri);
        else
            sb.Append(text);
    }

    private List<string> RenderListAsLines(RichTextContentField field)
    {
        if (field.NodeType is not ("unordered-list" or "ordered-list"))
            return [];

        var ordered = field.NodeType == "ordered-list";

        _renderContext.ListStack.Add(ordered ? "ol" : "ul");
        _renderContext.ListNestingLevels.Add(0);

        var outLines = new List<string>();
        foreach (var item in field.Content)
        {
            outLines.AddRange(RenderListItem(item, ordered));
        }

        _renderContext.ListStack.RemoveAt(_renderContext.ListStack.Count - 1);
        _renderContext.ListNestingLevels.RemoveAt(_renderContext.ListNestingLevels.Count - 1);

        return outLines;
    }
}
