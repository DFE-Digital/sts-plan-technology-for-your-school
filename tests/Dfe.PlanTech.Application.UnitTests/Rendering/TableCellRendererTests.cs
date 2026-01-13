using System.Text;
using Dfe.PlanTech.Application.Rendering;
using Dfe.PlanTech.Core.Contentful.Enums;
using Dfe.PlanTech.Core.Contentful.Interfaces;
using Dfe.PlanTech.Core.Contentful.Models;
using NSubstitute;

namespace Dfe.PlanTech.Application.UnitTests.Rendering;

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

    private RichTextContentField GetTableData()
    {
        return new RichTextContentField()
        {
            NodeType = RichTextNodeType.TableCell.ToString(),
            Content = [
                new RichTextContentField()
                {
                    Content = [
                        new RichTextContentField(){
                            Value = "This is the first line"
                        },
                    ]
                },
                new RichTextContentField()
                {
                    Content = [
                        new RichTextContentField(){
                            Value = "This is the Second line"
                        },
                    ]
                },
                new RichTextContentField()
                {
                    Content = [
                        new RichTextContentField(){
                            Value = "This is the Third line"
                        }
                    ]
                }
            ],
            Data = new RichTextContentSupportDataField()
            {
                Target = new RichTextContentDataEntry()
                {
                    Content =
                    [
                        new RichTextContentDataEntry()
                        {
                            InternalName = "Internal Name 1",
                            Title = "Title 1",
                            SummaryLine = "Summary Line 1",
                            RichText = new RichTextContentField()
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
