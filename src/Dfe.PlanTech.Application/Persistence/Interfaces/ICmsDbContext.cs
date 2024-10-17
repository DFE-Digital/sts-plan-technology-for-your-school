using Dfe.PlanTech.Domain.Content.Models;
using Dfe.PlanTech.Domain.Content.Models.Buttons;
using Dfe.PlanTech.Domain.Questionnaire.Models;

namespace Dfe.PlanTech.Application.Persistence.Interfaces;

/// <summary>
/// Interface for the DbContext responsible for storing CMS data
/// </summary>
public interface ICmsDbContext : IDbContext
{
    public IQueryable<AnswerDbEntity> Answers { get; }

    public IQueryable<ButtonDbEntity> Buttons { get; }

    public IQueryable<ButtonWithEntryReferenceDbEntity> ButtonWithEntryReferences { get; }

    public IQueryable<ButtonWithLinkDbEntity> ButtonWithLinks { get; }

    public IQueryable<CategoryDbEntity> Categories { get; }

    public IQueryable<ComponentDropDownDbEntity> ComponentDropDowns { get; }

    public IQueryable<ContentComponentDbEntity> ContentComponents { get; }

    public IQueryable<CSLinkDbEntity> CSLinks { get; }

    public IQueryable<HeaderDbEntity> Headers { get; }

    public IQueryable<InsetTextDbEntity> InsetTexts { get; }

    public IQueryable<NavigationLinkDbEntity> NavigationLink { get; }

    public IQueryable<PageDbEntity> Pages { get; }

    public IQueryable<PageContentDbEntity> PageContents { get; }

    public IQueryable<QuestionDbEntity> Questions { get; }

    public IQueryable<RecommendationChunkDbEntity> RecommendationChunks { get; }

    public IQueryable<RecommendationIntroDbEntity> RecommendationIntros { get; }

    public IQueryable<RecommendationSectionDbEntity> RecommendationSections { get; }

    public IQueryable<RecommendationChunkContentDbEntity> RecommendationChunkContents { get; }

    public IQueryable<RecommendationChunkAnswerDbEntity> RecommendationChunkAnswers { get; }

    public IQueryable<RecommendationSectionChunkDbEntity> RecommendationSectionChunks { get; }

    public IQueryable<RecommendationSectionAnswerDbEntity> RecommendationSectionAnswers { get; }

    public IQueryable<RecommendationIntroContentDbEntity> RecommendationIntroContents { get; }

    public IQueryable<SubtopicRecommendationDbEntity> SubtopicRecommendations { get; }

    public IQueryable<SubtopicRecommendationIntroDbEntity> SubtopicRecommendationIntros { get; }

    public IQueryable<RichTextContentDbEntity> RichTextContents { get; }

    public IQueryable<RichTextContentWithSlugDbEntity> RichTextContentWithSlugs { get; }

    public IQueryable<RichTextContentWithSubtopicRecommendationId> RichTextContentWithSubtopicRecommendationIds { get; }

    public IQueryable<RichTextDataDbEntity> RichTextDataDbEntity { get; }

    public IQueryable<RichTextMarkDbEntity> RichTextMarkDbEntity { get; }

    public IQueryable<SectionDbEntity> Sections { get; }

    public IQueryable<TextBodyDbEntity> TextBodies { get; }

    public IQueryable<TitleDbEntity> Titles { get; }

    public IQueryable<WarningComponentDbEntity> Warnings { get; }


    public Task<PageDbEntity?> GetPageBySlug(string slug, CancellationToken cancellationToken = default);
    public Task<int> SetComponentPublishedAndDeletedStatuses(ContentComponentDbEntity contentComponent, bool published, bool deleted, CancellationToken cancellationToken);

    public Task<TEntity?> GetEntityById<TEntity>(string contentId, CancellationToken cancellationToken = default) where TEntity : ContentComponentDbEntity;
}
