using Dfe.PlanTech.Application.Persistence.Interfaces;

namespace Dfe.PlanTech.Application.Core;

/// <summary>
/// Abstract class for a command, or query, that uses IContentRepository
/// </summary>
/// <remarks>
/// Simply enforces IContentRepository usage.
/// </remarks>
public abstract class ContentRetriever : IInfrastructureQuery
{
    protected readonly IContentRepository repository;

    protected ContentRetriever(IContentRepository repository)
    {
        this.repository = repository;
    }
}
