using Dfe.PlanTech.Domain.Questionnaire.Models;

namespace Dfe.PlanTech.Domain.Questionnaire.Interfaces;

public interface IGetCategoryQuery
{
    public Task<Category?> GetCategoryBySlug(
        string categorySlug,
        CancellationToken cancellationToken = default
    );
}
