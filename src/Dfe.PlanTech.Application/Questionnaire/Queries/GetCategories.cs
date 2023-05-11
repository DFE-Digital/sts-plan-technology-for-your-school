using Dfe.PlanTech.Application.Persistence.Interfaces;
using Dfe.PlanTech.Domain.Questionnaire.Models;

namespace Dfe.PlanTech.Application.Questionnaire.Queries;

/// <summary>
/// Gets categories from CMS
/// </summary>
public class GetCategoriesQuery 
{
    private readonly IContentRepository _repository;

    public GetCategoriesQuery(IContentRepository repository)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
    }

    public Task<IEnumerable<Category>> Get(IEnumerable<IContentQuery>? queries = null, CancellationToken cancellationToken = default)
        => _repository.GetEntities<Category>(queries, cancellationToken);
}
