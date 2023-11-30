using System.Text.Json.Nodes;
using AutoMapper;
using Dfe.PlanTech.Domain.Questionnaire.Models;

namespace Dfe.PlanTech.Domain.Mappings;

public class CmsDbProfile : Profile
{
  public CmsDbProfile()
  {
    CreateMap<JsonNode, AnswerDbEntity>()
      .ForMember(answer => answer.Text, source => source.MapFrom(json => json["Text"]))
      .ForAllMembers(answer => answer.MapFrom((src, dest, destMember, context) =>
      {
        var destinationName = answer.DestinationMember.Name;

        return src[destinationName];
      }));

    CreateMap<JsonNode, QuestionDbEntity>();
  }
}

