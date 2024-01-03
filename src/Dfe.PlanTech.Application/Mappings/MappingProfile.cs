using AutoMapper;
using Dfe.PlanTech.Domain.Content.Models;
using Dfe.PlanTech.Domain.Content.Models.Buttons;

namespace Dfe.PlanTech.Application.Mappings;

public class CmsMappingProfile : Profile
{
  public CmsMappingProfile()
  {
    CreateMap<PageDbEntity, Page>();

    CreateMap<ContentComponentDbEntity, ContentComponent>()
    .Include<ButtonDbEntity, Button>()
    .Include<ButtonWithEntryReferenceDbEntity, ButtonWithEntryReference>()
    .Include<ButtonWithLinkDbEntity, ButtonWithLink>()
    .Include<HeaderDbEntity, Header>()
    .Include<TextBodyDbEntity, TextBody>()
    .ForMember(dest => dest.Sys, opt => opt.MapFrom(src => src));

    CreateMap<ContentComponentDbEntity, SystemDetails>();

    CreateMap<ButtonDbEntity, Button>();
    CreateMap<ButtonWithEntryReferenceDbEntity, ButtonWithEntryReference>();
    CreateMap<ButtonWithLinkDbEntity, ButtonWithLink>()
    .ForMember(dest => dest.Button, opt => opt.MapFrom(src => src.Button));
    CreateMap<HeaderDbEntity, Header>();
    CreateMap<TextBodyDbEntity, TextBody>();
  }
}
