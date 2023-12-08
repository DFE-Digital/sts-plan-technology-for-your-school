using Dfe.PlanTech.Domain.Content.Interfaces;
using Dfe.PlanTech.Domain.Content.Models;

namespace Dfe.PlanTech.AzureFunctions.Mappings;

public class RichTextContentMapper
{
  public RichTextContentDbEntity MapContent(RichTextContent content)
  {
    var entity = new RichTextContentDbEntity()
    {
      Content = content.Content.Select(MapContent).ToList(),
      Data = MapData<RichTextData, RichTextDataDbEntity>(content.Data),
      Marks = content.Marks.Select(MapMark<RichTextMark, RichTextMarkDbEntity>).ToList(),
      Value = content.Value,
      NodeType = content.NodeType
    };

    return entity;
  }

  protected virtual TOutData? MapData<TInData, TOutData>(TInData? data)
  where TInData : class, IRichTextData
  where TOutData : class, IRichTextData, new()
  {
    if (data == null) return null;

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