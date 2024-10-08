using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using System.Security.Cryptography;
using System.Text;
using Dfe.PlanTech.Application.Caching.Interfaces;
using Dfe.PlanTech.Application.Caching.Models;
using Dfe.PlanTech.Application.Persistence.Interfaces;
using Dfe.PlanTech.Domain.Content.Models;
using Dfe.PlanTech.Domain.Content.Models.Buttons;
using Dfe.PlanTech.Domain.Exceptions;
using Dfe.PlanTech.Domain.Persistence.Models;
using Dfe.PlanTech.Domain.Questionnaire.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Logging;

namespace Dfe.PlanTech.Infrastructure.Data;

[ExcludeFromCodeCoverage]
public class CmsDbContext : DbContext, ICmsDbContext
{
    private const string Schema = "Contentful";

    //HashSet of all the types used for DbSets
    //E.g. DbSet<AnswerDbEntity> -> AnswerDbEntity
    private readonly HashSet<Type> _dbSetTypes;
    private readonly ILogger<CmsDbContext> _logger;

    public DbSet<AnswerDbEntity> Answers { get; set; }

    public DbSet<ButtonDbEntity> Buttons { get; set; }

    public DbSet<ButtonWithEntryReferenceDbEntity> ButtonWithEntryReferences { get; set; }

    public DbSet<ButtonWithLinkDbEntity> ButtonWithLinks { get; set; }

    public DbSet<CategoryDbEntity> Categories { get; set; }

    public DbSet<ComponentDropDownDbEntity> ComponentDropDowns { get; set; }

    public DbSet<ContentComponentDbEntity> ContentComponents { get; set; }

    public DbSet<CSLinkDbEntity> CSLinks { get; set; }

    public DbSet<HeaderDbEntity> Headers { get; set; }

    public DbSet<InsetTextDbEntity> InsetTexts { get; set; }

    public DbSet<NavigationLinkDbEntity> NavigationLink { get; set; }

    public DbSet<PageDbEntity> Pages { get; set; }

    public DbSet<PageContentDbEntity> PageContents { get; set; }

    public DbSet<QuestionDbEntity> Questions { get; set; }


    #region RECOMMENDATIONS
    //New
    public DbSet<RecommendationChunkAnswerDbEntity> RecommendationChunkAnswers { get; set; }
    public DbSet<RecommendationChunkContentDbEntity> RecommendationChunkContents { get; set; }
    public DbSet<RecommendationChunkDbEntity> RecommendationChunks { get; set; }
    public DbSet<RecommendationIntroContentDbEntity> RecommendationIntroContents { get; set; }
    public DbSet<RecommendationIntroDbEntity> RecommendationIntros { get; set; }
    public DbSet<RecommendationSectionAnswerDbEntity> RecommendationSectionAnswers { get; set; }
    public DbSet<RecommendationSectionChunkDbEntity> RecommendationSectionChunks { get; set; }
    public DbSet<RecommendationSectionDbEntity> RecommendationSections { get; set; }
    public DbSet<SubtopicRecommendationDbEntity> SubtopicRecommendations { get; set; }
    public DbSet<SubtopicRecommendationIntroDbEntity> SubtopicRecommendationIntros { get; set; }
    #endregion

    public DbSet<RichTextContentDbEntity> RichTextContents { get; set; }

    public DbSet<RichTextContentWithSubtopicRecommendationId> RichTextContentWithSubtopicRecommendationIds { get; set; }

    public DbSet<RichTextContentWithSlugDbEntity> RichTextContentWithSlugs { get; set; }

    public DbSet<RichTextDataDbEntity> RichTextDataDbEntity { get; set; }

    public DbSet<RichTextMarkDbEntity> RichTextMarkDbEntity { get; set; }

    public DbSet<SectionDbEntity> Sections { get; set; }

    public DbSet<TextBodyDbEntity> TextBodies { get; set; }

    public DbSet<TitleDbEntity> Titles { get; set; }

    public DbSet<WarningComponentDbEntity> Warnings { get; set; }

    #region IQueryables from interface
    IQueryable<AnswerDbEntity> ICmsDbContext.Answers => Answers;
    IQueryable<ButtonDbEntity> ICmsDbContext.Buttons => Buttons;
    IQueryable<ButtonWithEntryReferenceDbEntity> ICmsDbContext.ButtonWithEntryReferences => ButtonWithEntryReferences;
    IQueryable<ButtonWithLinkDbEntity> ICmsDbContext.ButtonWithLinks => ButtonWithLinks;
    IQueryable<CategoryDbEntity> ICmsDbContext.Categories => Categories;
    IQueryable<ComponentDropDownDbEntity> ICmsDbContext.ComponentDropDowns => ComponentDropDowns;
    IQueryable<ContentComponentDbEntity> ICmsDbContext.ContentComponents => ContentComponents;
    IQueryable<CSLinkDbEntity> ICmsDbContext.CSLinks => CSLinks;
    IQueryable<HeaderDbEntity> ICmsDbContext.Headers => Headers;
    IQueryable<InsetTextDbEntity> ICmsDbContext.InsetTexts => InsetTexts;
    IQueryable<NavigationLinkDbEntity> ICmsDbContext.NavigationLink => NavigationLink;
    IQueryable<PageContentDbEntity> ICmsDbContext.PageContents => PageContents;
    IQueryable<PageDbEntity> ICmsDbContext.Pages => Pages;
    IQueryable<QuestionDbEntity> ICmsDbContext.Questions => Questions;
    IQueryable<RecommendationChunkAnswerDbEntity> ICmsDbContext.RecommendationChunkAnswers => RecommendationChunkAnswers;
    IQueryable<RecommendationChunkContentDbEntity> ICmsDbContext.RecommendationChunkContents => RecommendationChunkContents;
    IQueryable<RecommendationChunkDbEntity> ICmsDbContext.RecommendationChunks => RecommendationChunks;
    IQueryable<RecommendationIntroContentDbEntity> ICmsDbContext.RecommendationIntroContents => RecommendationIntroContents;
    IQueryable<RecommendationIntroDbEntity> ICmsDbContext.RecommendationIntros => RecommendationIntros;
    IQueryable<RecommendationSectionAnswerDbEntity> ICmsDbContext.RecommendationSectionAnswers => RecommendationSectionAnswers;
    IQueryable<RecommendationSectionChunkDbEntity> ICmsDbContext.RecommendationSectionChunks => RecommendationSectionChunks;
    IQueryable<RecommendationSectionDbEntity> ICmsDbContext.RecommendationSections => RecommendationSections;
    IQueryable<RichTextContentDbEntity> ICmsDbContext.RichTextContents => RichTextContents;
    IQueryable<RichTextContentWithSubtopicRecommendationId> ICmsDbContext.RichTextContentWithSubtopicRecommendationIds => RichTextContentWithSubtopicRecommendationIds.Include(rt => rt.Data).Include(rt => rt.Marks);
    IQueryable<RichTextContentWithSlugDbEntity> ICmsDbContext.RichTextContentWithSlugs => RichTextContentWithSlugs.Include(rt => rt.Data).Include(rt => rt.Marks);
    IQueryable<RichTextDataDbEntity> ICmsDbContext.RichTextDataDbEntity => RichTextDataDbEntity;
    IQueryable<RichTextMarkDbEntity> ICmsDbContext.RichTextMarkDbEntity => RichTextMarkDbEntity;
    IQueryable<SectionDbEntity> ICmsDbContext.Sections => Sections;
    IQueryable<SubtopicRecommendationDbEntity> ICmsDbContext.SubtopicRecommendations => SubtopicRecommendations;
    IQueryable<SubtopicRecommendationIntroDbEntity> ICmsDbContext.SubtopicRecommendationIntros => SubtopicRecommendationIntros;
    IQueryable<TextBodyDbEntity> ICmsDbContext.TextBodies => TextBodies;
    IQueryable<TitleDbEntity> ICmsDbContext.Titles => Titles;
    IQueryable<WarningComponentDbEntity> ICmsDbContext.Warnings => Warnings;
    #endregion

    private readonly ContentfulOptions _contentfulOptions;
    private readonly IQueryCacher _queryCacher;

    public CmsDbContext()
    {
        _contentfulOptions = new ContentfulOptions(false);
        _queryCacher = new QueryCacher();
        _dbSetTypes = GetDbSetTypes();
        _logger = GetLogger();
    }

    public CmsDbContext(DbContextOptions<CmsDbContext> options) : base(options)
    {
        _contentfulOptions = this.GetService<ContentfulOptions>() ??
                             throw new MissingServiceException($"Could not find service {nameof(ContentfulOptions)}");
        _queryCacher = this.GetService<IQueryCacher>() ??
                       throw new MissingServiceException($"Could not find service {nameof(IQueryCacher)}");
        _dbSetTypes = GetDbSetTypes();
        _logger = GetLogger();
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(CmsDbContext).Assembly);

        modelBuilder.Entity<RecommendationChunkContentDbEntity>()
            .ToTable("RecommendationChunkContents", Schema);

        modelBuilder.Entity<RecommendationIntroDbEntity>()
            .ToTable("RecommendationIntros", Schema);

        modelBuilder.Entity<RecommendationSectionDbEntity>()
            .ToTable("RecommendationSections", Schema);

        modelBuilder.Entity<SubtopicRecommendationIntroDbEntity>()
            .ToTable("SubtopicRecommendationIntros", Schema);

        modelBuilder.HasDefaultSchema(Schema);

        modelBuilder.Entity<ContentComponentDbEntity>(entity =>
        {
            entity.ToTable("ContentComponents", Schema);
            entity.Property(e => e.Id).HasMaxLength(30);
            entity.HasQueryFilter(ShouldShowEntity());
        });

        modelBuilder.Entity<AnswerDbEntity>(entity =>
        {
            entity.HasOne(a => a.NextQuestion).WithMany(q => q.PreviousAnswers).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(a => a.ParentQuestion).WithMany(q => q.Answers).OnDelete(DeleteBehavior.Restrict);

            entity.ToTable("Answers", Schema);
        });

        modelBuilder.Entity<ButtonWithEntryReferenceDbEntity>(entity =>
        {
            entity.Navigation(button => button.Button).AutoInclude();
        });

        modelBuilder.Entity<ButtonWithLinkDbEntity>()
            .Navigation(button => button.Button).AutoInclude();

        modelBuilder.Entity<ButtonWithEntryReferenceDbEntity>().Navigation(button => button.Button).AutoInclude();

        modelBuilder.Entity<PageContentDbEntity>(entity =>
        {
            entity.HasOne(pc => pc.BeforeContentComponent).WithMany(c => c.BeforeTitleContentPagesJoins);

            entity.HasOne(pc => pc.ContentComponent).WithMany(c => c.ContentPagesJoins);

            entity.HasOne(pc => pc.Page).WithMany(p => p.AllPageContents);
        });


        modelBuilder.Entity<RecommendationChunkContentDbEntity>(entity =>
        {
            entity.HasOne(pc => pc.ContentComponent).WithMany(c => c.RecommendationChunkContentJoins);
        });

        modelBuilder.Entity<RecommendationIntroContentDbEntity>(entity =>
        {
            entity.HasOne(pc => pc.ContentComponent).WithMany(c => c.RecommendationIntroContentJoins);
        });

        modelBuilder.Entity<QuestionDbEntity>().ToTable("Questions", Schema);

        modelBuilder.Entity<RichTextContentWithSlugDbEntity>(entity => { entity.ToView("RichTextContentsBySlug"); });
        modelBuilder.Entity<RichTextContentWithSubtopicRecommendationId>(entity => { entity.ToView("RichTextContentsBySubtopicRecommendationId"); });

        modelBuilder.Entity<TitleDbEntity>(entity => { entity.ToTable("Titles", Schema); });

        modelBuilder.Entity<WarningComponentDbEntity>(entity =>
        {
            entity.HasOne(warning => warning.Text).WithMany(text => text.Warnings).OnDelete(DeleteBehavior.Restrict);

            entity.Navigation(warningComponent => warningComponent.Text).AutoInclude();
        });

        modelBuilder.Entity<ContentComponentDbEntity>().HasQueryFilter(ShouldShowEntity());
    }


    /// <summary>
    /// Sets the published and deleted statuses for a content component.
    /// </summary>
    /// <remarks>
    /// Uses interpolated SQL for effeciency
    /// </remarks>
    /// <param name="contentComponent">The content component to update.</param>
    /// <param name="published">Whether the content component should be published.</param>
    /// <param name="deleted">Whether the content component should be deleted.</param>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the operation.</param>
    /// <returns>Task - result is amount of rows affected by the operation.</returns>
    public virtual Task<int> SetComponentPublishedAndDeletedStatuses(ContentComponentDbEntity contentComponent, bool published, bool deleted, CancellationToken cancellationToken)
        => Database.ExecuteSqlAsync($"UPDATE [Contentful].[ContentComponents] SET Published = {published}, Deleted = {deleted} WHERE [Id] = {contentComponent.Id}", cancellationToken: cancellationToken);

    /// <summary>
    /// Should the given entity be displayed? I.e. is it not archived, not deleted, and either published or use preview mode is enabled
    /// </summary>
    private Expression<Func<ContentComponentDbEntity, bool>> ShouldShowEntity()
        => entity => (_contentfulOptions.UsePreview || entity.Published) && !entity.Archived && !entity.Deleted;

    private static string GetCacheKey(IQueryable query)
    {
        var queryString = query.ToQueryString();
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(queryString));
        return Convert.ToBase64String(hash);
    }

    public Task<PageDbEntity?> GetPageBySlug(string slug, CancellationToken cancellationToken = default)
        => FirstOrDefaultAsync(
            Pages.Where(page => page.Slug == slug)
                .Include(page => page.BeforeTitleContent)
                .Include(page => page.Content)
                .Include(page => page.Title)
                .AsSplitQuery(),
            cancellationToken);

    public async Task<List<T>> ToListAsync<T>(IQueryable<T> queryable, CancellationToken cancellationToken = default)
    {
        var key = GetCacheKey(queryable);
        var result = await _queryCacher.GetOrCreateAsyncWithCache(key, queryable,
            (q, ctoken) => q.ToListAsync(ctoken), cancellationToken);

        foreach (var item in result)
        {
            AttachEntity(item);
        }

        return result;
    }

    public async Task<T?> FirstOrDefaultAsync<T>(IQueryable<T> queryable, CancellationToken cancellationToken = default)
    {
        var key = GetCacheKey(queryable);
        var result = await _queryCacher.GetOrCreateAsyncWithCache(key, queryable,
            (q, ctoken) => q.FirstOrDefaultAsync(ctoken), cancellationToken);

        AttachEntity(result);

        return result;
    }

    ///<summary>
    ///Attaches the entity/marks it as "tracked" in the DbContext change tracker
    ///</summary>
    ///<remarks>
    ///If the result was retrieved from the cache, or it is going to be used by another entity that _was_ retrieved from the cache,
    ///Then we need to ensure that EF Core is aware of it, otherwise it will not fix-up the navigations as it would normally
    ///</remarks>
    private void AttachEntity<T>(T entity)
    {
        //T can be any type, since FirstOrDefaultAsync or ToListAsync etc can return any type
        //Therefore we need to check if there is actually a DbSet for the entity first, before attaching it
        var isExistingDbSet = _dbSetTypes.Contains(typeof(T));

        if (!isExistingDbSet)
        {
            _logger.LogWarning("Tried to attach entity of type {EntityType} but was not found in DbSets", typeof(T));
            return;
        }

        //Sonarcloud will complain about default(T), but it _has_ to be a class at this point
        if (entity is null)
        {
            _logger.LogWarning("Tried to attach null or default entity of type {EntityType}", typeof(T));
            return;
        }

        base.Attach(entity);
    }

    ///<summary>
    ///Creates a HashSet of all types of DbSet in the CmsDbContext.
    ///</summary>
    ///<remarks>
    ///E.g. AnswerDbEntity, QuestionDbEntity, etc.
    ///</remarks>
    private HashSet<Type> GetDbSetTypes() => this.GetType()
                                                .GetProperties()
                                                .Where(prop => prop.PropertyType.IsGenericType &&
                                                            prop.PropertyType.GetGenericTypeDefinition() == typeof(DbSet<>))
                                                .Select(prop => prop.PropertyType.GetGenericArguments()[0])
                                                .ToHashSet();

    private ILogger<CmsDbContext> GetLogger()
        =>this.GetService<ILogger<CmsDbContext>>() ?? throw new MissingServiceException($"Could not find service for {typeof(ILogger<CmsDbContext>)}");
}
