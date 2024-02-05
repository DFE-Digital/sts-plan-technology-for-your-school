namespace Dfe.PlanTech.Application.Mappings;

[ExcludeFromCodeCoverage]
public class CmsMappingProfile : Profile
{
    public CmsMappingProfile()
    {
        CreateMap<PageDbEntity, Page>();

        CreateMap<ContentComponentDbEntity, ContentComponent>()
        .Include<AnswerDbEntity, Answer>()
        .Include<ButtonDbEntity, Button>()
        .Include<ButtonWithEntryReferenceDbEntity, ButtonWithEntryReference>()
        .Include<ButtonWithLinkDbEntity, ButtonWithLink>()
        .Include<CategoryDbEntity, Category>()
        .Include<HeaderDbEntity, Header>()
        .Include<InsetTextDbEntity, InsetText>()
        .Include<QuestionDbEntity, Question>()
        .Include<RecommendationPageDbEntity, RecommendationPage>()
        .Include<SectionDbEntity, Section>()
        .Include<TextBodyDbEntity, TextBody>()
        .Include<TitleDbEntity, Title>()
        .Include<WarningComponentDbEntity, WarningComponent>()
        .ForMember(dest => dest.Sys, opt => opt.MapFrom(src => src));

        CreateMap<ContentComponentDbEntity, SystemDetails>();

        CreateMap<AnswerDbEntity, Answer>();
        CreateMap<ButtonDbEntity, Button>();
        CreateMap<ButtonWithEntryReferenceDbEntity, ButtonWithEntryReference>();
        CreateMap<ButtonWithLinkDbEntity, ButtonWithLink>();
        CreateMap<CategoryDbEntity, Category>();
        CreateMap<HeaderDbEntity, Header>();
        CreateMap<InsetTextDbEntity, InsetText>();
        CreateMap<QuestionDbEntity, Question>();
        CreateMap<RecommendationPageDbEntity, RecommendationPage>();
        CreateMap<SectionDbEntity, Section>();
        CreateMap<TextBodyDbEntity, TextBody>();
        CreateMap<TitleDbEntity, Title>();
        CreateMap<WarningComponentDbEntity, WarningComponent>();

        CreateMap<RichTextContentDbEntity, RichTextContent>();
        CreateMap<RichTextDataDbEntity, RichTextData>();
        CreateMap<RichTextMarkDbEntity, RichTextMark>();

    }
}
