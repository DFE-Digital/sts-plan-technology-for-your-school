using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dfe.PlanTech.Domain.Entities;
using Dfe.PlanTech.Infrastructure.Persistence;
using Dfe.PlanTech.Infrastructure.Persistence.Querying;

namespace Dfe.PlanTech.Application.Questionnaire.Queries;

/// <summary>
/// Gets categories from CMS
/// </summary>
public class GetCategories
{
    private readonly IContentRepository _repository;

    public GetCategories(IContentRepository repository)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
    }

    public Task<IEnumerable<Category>> Get(IEnumerable<ContentQuery>? queries = null, CancellationToken cancellationToken = default(CancellationToken))
        => _repository.GetEntities<Category>(queries, cancellationToken);
}
