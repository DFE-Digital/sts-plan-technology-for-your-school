using System.Text;
using System.Text.RegularExpressions;
using Dfe.PlanTech.Core.Contentful.Models;

namespace Dfe.PlanTech.Application.Rendering.Markdown;

public class MarkdownRenderer
{
    private const string BlockQuote = "blockquote";
    private const string Document = "document";
    private const string Heading1 = "heading-1";
    private const string Heading2 = "heading-2";
    private const string Heading3 = "heading-3";
    private const string Heading4 = "heading-4";
    private const string Heading5 = "heading-5";
    private const string Heading6 = "heading-6";
    private const string HorizontalRule = "hr";
    private const string ListItem = "list-item";
    private const string OrderedList = "ordered-list";
    private const string Paragraph = "paragraph";
    private const string UnorderedList = "unordered-list";
    private const string AssetHyperlink = "asset-hyperlink";
    private const string EntryHyperlink = "entry-hyperlink";
    private const string Hyperlink = "hyperlink";
    private const string LineBreak = "line-break";
    private const string Text = "text";

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
        BlockQuote,
        Document,
        Heading1,
        Heading2,
        Heading3,
        Heading4,
        Heading5,
        Heading6,
        HorizontalRule,
        ListItem,
        OrderedList,
        Paragraph,
        UnorderedList,
    ];

    private readonly List<string> _knownInlineNodeTypes =
    [
        AssetHyperlink,
        EntryHyperlink,
        Hyperlink,
        LineBreak,
        Text,
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
            case BlockQuote:
                RenderBlockQuote(field);
                break;
            case Document:
                RenderChildren(field);
                break;
            case HorizontalRule:
                EmitBlock(["---"]);
                break;
            case OrderedList:
                RenderOrderedList(field);
                break;
            case Paragraph:
                RenderParagraph(field);
                break;
            case UnorderedList:
                RenderUnorderedList(field);
                break;
        }
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
        if (
            hasHyphenatedLevel
            && int.TryParse(field.NodeType[(hyphenIndex + 1)..], out var parsedLevel)
        )
            level = parsedLevel;

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
        if (field.NodeType != ListItem)
            return [];

        string bullet;
        if (ordered)
        {
            _renderContext.ListNestingLevels[^1] = _renderContext.ListNestingLevels[^1] + 1;
        }

        bullet = ordered ? $"{_renderContext.ListNestingLevels[^1]}." : "*";

        var indentLevel = Math.Max(_renderContext.ListStack.Count - 1, 0);
        var indent = new string(' ', indentLevel * 2);

        string? firstText = null;
        var listTypes = new List<string>() { OrderedList, UnorderedList };
        var trailingBlocks = new List<string>();

        foreach (var content in field.Content)
        {
            if (content.NodeType == Paragraph)
            {
                var text = CleanText(RenderInlines(content));

                if (string.IsNullOrEmpty(text))
                {
                    continue;
                }

                if (firstText == null)
                    firstText = text;
                else
                    trailingBlocks.Add(text);

                continue;
            }

            if (listTypes.Contains(content.NodeType))
            {
                trailingBlocks.AddRange(RenderListAsLines(content));
                continue;
            }

            trailingBlocks.AddRange(
                RenderBlockChildrenToLines([content])
                    .Where(x => !string.IsNullOrEmpty(CleanText(x)))
            );
        }

        firstText ??= "";
        var lines = new List<string> { $"{indent}{bullet} {firstText}" };

        var cleanedBlocks = trailingBlocks
            .Select(CleanText)
            .Where(text => !string.IsNullOrEmpty(text))
            .Select(text => $"{indent}{text}")
            .ToArray();

        lines.AddRange(cleanedBlocks);

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
                case Text:
                    stringBuilder.Append(CleanText(content.Value));
                    break;
                case Hyperlink:
                    RenderHyperlink(stringBuilder, content);
                    break;
                case AssetHyperlink:
                case EntryHyperlink:
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
        if (field.NodeType is not (UnorderedList or OrderedList))
            return [];

        var ordered = field.NodeType == OrderedList;

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
