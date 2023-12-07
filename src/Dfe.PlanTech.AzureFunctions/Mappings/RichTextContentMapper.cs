using System.Text.Json;
using Dfe.PlanTech.Domain.Caching.Models;
using Dfe.PlanTech.Domain.Content.Interfaces;
using Dfe.PlanTech.Domain.Content.Models;
using Dfe.PlanTech.Domain.Questionnaire.Models;
using Microsoft.Extensions.Logging;

namespace Dfe.PlanTech.AzureFunctions.Mappings;

public class RichTextContentMapper
{
  public RichTextContentDbEntity MapContent(RichTextContent content)
  {
    var entity = new RichTextContentDbEntity()
    {
      Content = content.Content.Select(MapContent).ToList(),
      Data = MapData<RichTextDataDbEntity, RichTextData>(content?.Data),

    }
  }

  protected virtual TOutData? MapData<TOutData, TInData>(TInData? data)
  where TOutData : class, IRichTextData, new()
  where TInData : class, IRichTextData
  {
    if (data == null) return null;

    var outData = new TOutData()
    {
      Uri = data.Uri
    };

    return outData;
  }
}