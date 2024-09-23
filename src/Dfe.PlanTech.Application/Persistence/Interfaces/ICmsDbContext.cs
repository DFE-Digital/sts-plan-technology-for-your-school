using System.Reflection;
using Dfe.PlanTech.Domain.Content.Models;
using Dfe.PlanTech.Domain.Content.Models.Buttons;
using Dfe.PlanTech.Domain.Questionnaire.Models;

namespace Dfe.PlanTech.Application.Persistence.Interfaces;

public interface ICmsDbContext
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

    public Task<T?> FirstOrDefaultAsync<T>(IQueryable<T> queryable, CancellationToken cancellationToken = default);
    public Task<List<T>> ToListAsync<T>(IQueryable<T> queryable, CancellationToken cancellationToken = default);
    public bool GetRequiredPropertiesForType(Type type, out IEnumerable<PropertyInfo> properties);
    public void ClearChangeTracker();
    public void AddEntity<TEntity>(TEntity entity) where TEntity: ContentComponentDbEntity;
    public void UpdateEntity<TEntity>(TEntity entity) where TEntity: ContentComponentDbEntity;
    public Task<int> SaveChangesAsync(CancellationToken cancellationToken);

    public Task<int> SetComponentPublishedAndDeletedStatuses(ContentComponentDbEntity contentComponent, bool published, bool deleted, CancellationToken cancellationToken);
}
