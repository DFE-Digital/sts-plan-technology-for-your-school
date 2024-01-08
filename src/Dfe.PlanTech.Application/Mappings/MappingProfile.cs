using AutoMapper;
using Dfe.PlanTech.Domain.Content.Models;
using Dfe.PlanTech.Domain.Content.Models.Buttons;
using Dfe.PlanTech.Domain.Questionnaire.Models;

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
        .Include<CategoryDbEntity, Category>()
        .Include<HeaderDbEntity, Header>()
        .Include<TextBodyDbEntity, TextBody>()
        .Include<WarningComponentDbEntity, WarningComponent>()
        .ForMember(dest => dest.Sys, opt => opt.MapFrom(src => src));

        CreateMap<ContentComponentDbEntity, SystemDetails>();

        CreateMap<ButtonDbEntity, Button>();
        CreateMap<ButtonWithEntryReferenceDbEntity, ButtonWithEntryReference>();
        CreateMap<ButtonWithLinkDbEntity, ButtonWithLink>();
        CreateMap<CategoryDbEntity, Category>();
        CreateMap<SectionDbEntity, Section>();

        CreateMap<HeaderDbEntity, Header>();
        CreateMap<TextBodyDbEntity, TextBody>();

        CreateMap<RichTextContentDbEntity, RichTextContent>();
        CreateMap<RichTextDataDbEntity, RichTextData>();
        CreateMap<RichTextMarkDbEntity, RichTextMark>();

        CreateMap<TitleDbEntity, Title>();
        CreateMap<WarningComponentDbEntity, WarningComponent>();
    }
}
