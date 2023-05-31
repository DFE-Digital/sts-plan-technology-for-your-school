using Dfe.PlanTech.Application.Core;
using Dfe.PlanTech.Application.Persistence.Interfaces;
using Dfe.PlanTech.Application.Persistence.Models;
using Dfe.PlanTech.Domain.Content.Models;
using Dfe.PlanTech.Infrastructure.Application.Models;
using Dfe.PlanTech.Domain.Questionnaire.Interfaces;

namespace Dfe.PlanTech.Application.Content.Queries;

public class GetCategoriesPageQuery : ContentRetriever
{
    public GetCategoriesPageQuery(IContentRepository repository) : base(repository)
    {
    }

    /// <summary>
    /// Fetches page from <see chref="IContentRepository"/> by slug
    /// </summary>
    /// <param name="slug">Slug for the Page</param>
    /// <returns>Page matching slug</returns>
    public async Task<Page> GetPageBySlug(string slug)
    {
        var pageQuery = new GetPageQuery(repository);
        var page = await pageQuery.GetPageBySlug(slug);
        
        var categoryIndex = 1;
        
        for(var x = 0; x < page.Content.Length; x++){
            var content = page.Content[x];
            
            if(content is not ICategory category){
                continue;
            }
            
            category.Header.WithText($"{categoryIndex}. {category.Header.Text}");
        }
        
        return page;
    }
}
