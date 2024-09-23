using Dfe.PlanTech.Domain.Content.Interfaces;
using Dfe.PlanTech.Domain.Content.Models;

namespace Dfe.PlanTech.Web.DbWriter.Mappings;

public class RichTextContentMapper
{
    public RichTextContentDbEntity MapToDbEntity(RichTextContent content)
      => MapContent<RichTextMark, RichTextContent, RichTextData, RichTextMarkDbEntity, RichTextContentDbEntity, RichTextDataDbEntity>(content);

    public RichTextContent MapToRichTextContent(RichTextContentDbEntity content)
      => MapContent<RichTextMarkDbEntity, RichTextContentDbEntity, RichTextDataDbEntity, RichTextMark, RichTextContent, RichTextData>(content);

    public TContentTypeOut MapContent<TMarkIn, TContentTypeIn, TDataIn, TMarkOut, TContentTypeOut, TDataOut>(IRichTextContent<TMarkIn, TContentTypeIn, TDataIn> content)
    where TMarkIn : class, IRichTextMark, new()
    where TContentTypeIn : class, IRichTextContent<TMarkIn, TContentTypeIn, TDataIn>, new()
    where TDataIn : class, IRichTextData, new()
    where TMarkOut : class, IRichTextMark, new()
    where TContentTypeOut : class, IRichTextContent<TMarkOut, TContentTypeOut, TDataOut>, new()
    where TDataOut : class, IRichTextData, new()
    {
        var entity = new TContentTypeOut()
        {
            Content = content.Content.Select(MapContent<TMarkIn, TContentTypeIn, TDataIn, TMarkOut, TContentTypeOut, TDataOut>).ToList(),
            Data = MapData<TDataIn, TDataOut>(content.Data),
            Marks = content.Marks.Select(MapMark<TMarkIn, TMarkOut>).ToList(),
            Value = content.Value,
            NodeType = content.NodeType
        };

        return entity;
    }

    protected virtual TOutData? MapData<TInData, TOutData>(TInData? data)
    where TInData : class, IRichTextData
    where TOutData : class, IRichTextData, new()
    {
        if (data == null)
            return null;

        var outData = new TOutData()
        {
            Uri = data.Uri
        };

        return outData;
    }

    protected virtual TOutMark MapMark<TInMark, TOutMark>(TInMark mark)
      where TInMark : class, IRichTextMark
    where TOutMark : class, IRichTextMark, new()
    {
        var outMark = new TOutMark()
        {
            Type = mark.Type
        };

        return outMark;
    }
}
