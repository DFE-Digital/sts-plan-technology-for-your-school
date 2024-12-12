using Dfe.PlanTech.Domain.Content.Models.ContentSupport.Mapped;

namespace Dfe.PlanTech.Web.Content;

public interface IContentService
{
    Task<CsPage?> GetContent(string slug, CancellationToken cancellationToken = default);
}
