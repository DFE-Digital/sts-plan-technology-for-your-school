using System.Text.Json.Nodes;
using AutoMapper;
using Dfe.PlanTech.Domain.Content.Models;
using Dfe.PlanTech.Domain.Questionnaire.Models;

namespace Dfe.PlanTech.Domain.Mappings;

public class CmsDbProfile : Profile
{
  public CmsDbProfile()
  {
    CreateMap<JsonNode, AnswerDbEntity>()
      .ForAllMembers(answer => MapFromDestinationName(answer));

    CreateMap<JsonNode, PageDbEntity>()
          .ForAllMembers(page => MapFromDestinationName(page));

    CreateMap<JsonNode, QuestionDbEntity>()
          .ForAllMembers(question => MapFromDestinationName(question));

    CreateMap<JsonNode, TitleDbEntity>()
      .ForAllMembers(title => MapFromDestinationName(title));
  }

  private static void MapFromDestinationName<TDbEntity>(IMemberConfigurationExpression<JsonNode, TDbEntity, object> mapping)
  where TDbEntity : ContentComponentDbEntity
  {
    mapping.MapFrom((src, dest, destMember, context) =>
    {
      var destinationName = mapping.DestinationMember.Name;

      return src[destinationName];
    });
  }
}

