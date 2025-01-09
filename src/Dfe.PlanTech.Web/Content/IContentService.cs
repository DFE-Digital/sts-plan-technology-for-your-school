using Dfe.PlanTech.Domain.Content.Models.ContentSupport;

namespace Dfe.PlanTech.Web.Content;

public interface IContentService
{
    Task<ContentSupportPage?> GetContent(string slug, CancellationToken cancellationToken = default);
}
