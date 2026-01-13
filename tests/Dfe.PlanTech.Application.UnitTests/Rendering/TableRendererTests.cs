using System.Text;
using Dfe.PlanTech.Application.Rendering;
using Dfe.PlanTech.Core.Contentful.Interfaces;
using Dfe.PlanTech.Core.Contentful.Models;
using Microsoft.Extensions.Logging.Abstractions;

namespace Dfe.PlanTech.Application.UnitTests.Rendering;

public class TableRendererTests
{
    private const string NODE_TYPE = "table";

    private static RichTextContentField CreateTestData()
    {
        var tableCellText = new RichTextContentField()
        {
            NodeType = "text",
            Value = "test"
        };

        var tableCell = new RichTextContentField()
        {
            Content = new() { tableCellText },
            NodeType = "paragraph",
            Value = ""
        };

        var tableCellContent = new RichTextContentField()
        {
            Content = new() { tableCell },
            NodeType = "table-cell",
            Value = ""
        };

        var tableHeaderCellsContent = new RichTextContentField()
        {
            Content = new() { tableCell },
            NodeType = "table-header-cell",
            Value = ""
        };

        var tableCellContentTwo = new RichTextContentField()
        {
            Content = new() { tableCell },
            NodeType = "table-cell",
            Value = ""
        };

        var tableHeaderCellsContentTwo = new RichTextContentField()
        {
            Content = new() { tableCell },
            NodeType = "table-header-cell",
            Value = ""
        };

        var tableHeaderRowContent = new RichTextContentField()
        {
            Content = new() { tableHeaderCellsContent, tableHeaderCellsContentTwo },
            NodeType = "table-row",
            Value = "",
        };

        var tableBodyRowContent = new RichTextContentField()
        {
            Content = new() { tableCellContent, tableCellContentTwo },
            NodeType = "table-row",
            Value = "",
        };

        var tableContent = new RichTextContentField()
        {
            Content = new() { tableHeaderRowContent, tableBodyRowContent },
            NodeType = NODE_TYPE,
            Value = "",
        };

        return tableContent;
    }

    [Fact]
    public void Should_Accept_When_ContentIs_Table()
    {
        const string value = "Text value";

        var content = new RichTextContentField()
        {
            NodeType = NODE_TYPE,
            Value = value,
        };

        var renderer = new TableRenderer();

        var accepted = renderer.Accepts(content);

        Assert.True(accepted);
    }


    [Fact]
    public void Renders_Table_Content_As_Expected()
    {
        var content = CreateTestData();
        var tableRenderer = new TableRenderer();
        var tableHeaderCellRenderer = new TableHeaderCellRenderer();
        var tableRowRenderer = new TableRowRenderer();
        var tableCellRenderer = new TableCellRenderer();

        var renderers = new List<IRichTextContentPartRenderer>
            { tableRenderer, tableRowRenderer, tableHeaderCellRenderer, tableCellRenderer };

        var rendererCollection = new RichTextRenderer(new NullLogger<RichTextRenderer>(),
            renderers);

        StringBuilder output = tableRenderer.AddHtml(content, rendererCollection, new StringBuilder());

        Assert.Equal(
            """<table class="govuk-table"><thead class="govuk-table__head"><tr class="govuk-table__row"><th class="govuk-table__header">test</th><th class="govuk-table__header">test</th></tr></thead><tbody class="govuk-table__body"><tr class="govuk-table__row"><th scope="row" class="govuk-table__header">test</th><td class="govuk-table__cell">test</td></tr></tbody></table>""",
            output.ToString());


    }

}
