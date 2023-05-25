using Dfe.PlanTech.Application.Persistence.Interfaces;

namespace Dfe.PlanTech.Application.Core;

public abstract class ContentRetriever : IInfrastructureQuery
{
    protected readonly IContentRepository repository;

    protected ContentRetriever(IContentRepository repository)
    {
        this.repository = repository;
    }
}