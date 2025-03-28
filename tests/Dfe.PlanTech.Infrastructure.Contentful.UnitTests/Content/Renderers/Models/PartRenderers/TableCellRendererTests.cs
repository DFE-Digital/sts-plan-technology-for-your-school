using System.Text;
using Dfe.PlanTech.Domain.Content.Enums;
using Dfe.PlanTech.Domain.Content.Interfaces;
using Dfe.PlanTech.Domain.Content.Models;
using Dfe.PlanTech.Infrastructure.Contentful.Content.Renderers.Models.PartRenderers;
using NSubstitute;

namespace Dfe.PlanTech.Infrastructure.Contentful.UnitTests.Content.Renderers.Models.PartRenderers;

public class TableCellRendererTests
{
    private readonly IRichTextContentPartRendererCollection _renderCollecion = Substitute.For<IRichTextContentPartRendererCollection>();

    [Fact]
    public void CheckTableRendersAreBuiltCorrectly()
    {
        var renderer = new TableCellRenderer();

        var stringBuilder = new StringBuilder();

        var rendererCollection = new TableCellRenderer();

        var result = renderer.AddHtml(GetTableData(), _renderCollecion, stringBuilder);

        Assert.Equal(GetStringBuilderValue().ToString(), result.ToString());
    }

    private StringBuilder GetStringBuilderValue()
    {
        var stringBuilder = new StringBuilder();
        stringBuilder.Append("<td class=\"govuk-table__cell\">");
        stringBuilder.Append("This is the first line");
        stringBuilder.Append("<br /><br />");
        stringBuilder.Append("This is the Second line");
        stringBuilder.Append("<br /><br />");
        stringBuilder.Append("This is the Third line");
        stringBuilder.Append("</td>");
        return stringBuilder;
    }

    private RichTextContent GetTableData()
    {
        return new RichTextContent()
        {
            NodeType = RichTextNodeType.TableCell.ToString(),
            Content = [
                new RichTextContent()
                {
                    Content = [
                        new RichTextContent(){
                            Value = "This is the first line"
                        },
                    ]
                },
                new RichTextContent()
                {
                    Content = [
                        new RichTextContent(){
                            Value = "This is the Second line"
                        },
                    ]
                },
                new RichTextContent()
                {
                    Content = [
                        new RichTextContent(){
                            Value = "This is the Third line"
                        }
                    ]
                }
            ],
            Data = new RichTextContentSupportData()
            {
                Target = new RichTextContentData()
                {
                    Content =
                    [
                        new RichTextContentData()
                        {
                            InternalName = "Internal Name 1",
                            Title = "Title 1",
                            SummaryLine = "Summary Line 1",
                            RichText = new RichTextContent()
                            {
                                Description = "This is just a description and has no render content"
                            },
                        },
                    ]
                }
            }
        };
    }
}
