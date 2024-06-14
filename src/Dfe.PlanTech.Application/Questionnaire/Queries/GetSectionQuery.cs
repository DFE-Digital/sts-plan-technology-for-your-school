using AutoMapper;
using Dfe.PlanTech.Application.Core;
using Dfe.PlanTech.Application.Exceptions;
using Dfe.PlanTech.Application.Persistence.Interfaces;
using Dfe.PlanTech.Application.Persistence.Models;
using Dfe.PlanTech.Domain.Content.Models;
using Dfe.PlanTech.Domain.Questionnaire.Interfaces;
using Dfe.PlanTech.Domain.Questionnaire.Models;
using Dfe.PlanTech.Infrastructure.Application.Models;
using System.Linq.Expressions;

namespace Dfe.PlanTech.Application.Questionnaire.Queries;

public class GetSectionQuery : ContentRetriever, IGetSectionQuery
{
    public const string SlugFieldPath = "fields.interstitialPage.fields.slug";
    private readonly ICmsDbContext _db;
    private readonly IMapper _mapper;

    public GetSectionQuery(ICmsDbContext db, IContentRepository repository, IMapper mapper) : base(repository)
    {
        _db = db;
        _mapper = mapper;
    }

    public async Task<Section?> GetSectionBySlug(string sectionSlug, CancellationToken cancellationToken = default)
    {
        var section = await GetSectionFromDb(sectionSlug, cancellationToken);

        return section ?? await GetSectionFromContentful(sectionSlug, cancellationToken);
    }

    private async Task<Section?> GetSectionFromDb(string sectionSlug, CancellationToken cancellationToken)
    {
        var query = _db.Sections.Where(SlugMatchesInterstitialPage(sectionSlug))
                                .Select(ProjectSection);

        var section = await _db.FirstOrDefaultAsync(query, cancellationToken);

        if (section == null) return null;

        return _mapper.Map<Section>(section);
    }

    private static readonly Expression<Func<SectionDbEntity, SectionDbEntity>> ProjectSection = section => new SectionDbEntity()
    {
        Id = section.Id,
        Name = section.Name,
        Questions = section.Questions
                            .OrderBy(question => question.Order)
                            .Select(question => new QuestionDbEntity()
                            {
                                Answers = question.Answers.OrderBy(answer => answer.Order)
                                                        .Select(answer => new AnswerDbEntity()
                                                        {
                                                            Id = answer.Id,
                                                            Maturity = answer.Maturity,
                                                            NextQuestion = answer.NextQuestion == null ? null : new QuestionDbEntity()
                                                            {
                                                                Id = answer.NextQuestion.Id,
                                                                Slug = answer.NextQuestion.Slug,
                                                                Text = answer.NextQuestion.Text
                                                            },
                                                            NextQuestionId = answer.NextQuestionId,
                                                            Text = answer.Text,
                                                        }).ToList(),
                                Id = question.Id,
                                HelpText = question.HelpText,
                                Text = question.Text,
                                Slug = question.Slug,
                            }).ToList(),
    };

    private static Expression<Func<SectionDbEntity, bool>> SlugMatchesInterstitialPage(string sectionSlug)
        => section => section.InterstitialPage != null && section.InterstitialPage.Slug == sectionSlug;

    private async Task<Section?> GetSectionFromContentful(string sectionSlug, CancellationToken cancellationToken)
    {
        var options = new GetEntitiesOptions()
        {
            Queries = new[] {
                    new ContentQueryEquals(){
                        Field = SlugFieldPath,
                        Value = sectionSlug
                    },
                    new ContentQueryEquals(){
                    Field="fields.interstitialPage.sys.contentType.sys.id",
                    Value="page"
                    }
                }
        };

        try
        {
            return (await repository.GetEntities<Section>(options, cancellationToken)).FirstOrDefault();
        }
        catch (Exception ex)
        {
            throw new ContentfulDataUnavailableException($"Error getting section with slug {sectionSlug}", ex);
        }
    }
}
