using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Sts.PlanTech.Domain.Entities;
using Sts.PlanTech.Infrastructure.Persistence;
using Sts.PlanTech.Infrastructure.Persistence.Querying;

namespace Sts.PlanTech.Application.Questionnaire.Queries;

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
