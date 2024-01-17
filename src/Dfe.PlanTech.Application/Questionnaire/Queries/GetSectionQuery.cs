using AutoMapper;
using Dfe.PlanTech.Application.Core;
using Dfe.PlanTech.Application.Exceptions;
using Dfe.PlanTech.Application.Persistence.Interfaces;
using Dfe.PlanTech.Application.Persistence.Models;
using Dfe.PlanTech.Domain.Questionnaire.Interfaces;
using Dfe.PlanTech.Domain.Questionnaire.Models;
using Dfe.PlanTech.Infrastructure.Application.Models;

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
        var query = _db.Sections.Select(section => new SectionDbEntity()
        {
            Name = section.Name,
            Questions = section.Questions.Select(question => new QuestionDbEntity()
            {
                Slug = question.Slug,

            }).ToList()
        });

        var section = await _db.FirstOrDefaultAsync(query, cancellationToken);

        if (section == null) return null;

        return _mapper.Map<Section>(section);
    }

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
