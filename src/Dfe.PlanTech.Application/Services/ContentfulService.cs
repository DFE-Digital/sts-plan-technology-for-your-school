using Dfe.PlanTech.Application.Workflows;
using Dfe.PlanTech.Core.DataTransferObjects.Contentful;

namespace Dfe.PlanTech.Application.Services
{
    public class ContentfulService(
        ContentfulWorkflow contentfulWorkflow
    )
    {
        private readonly ContentfulWorkflow _contentfulWorkflow = contentfulWorkflow ?? throw new ArgumentNullException(nameof(contentfulWorkflow));


        public Task<CmsPageDto> GetPageBySlug(string slug)
        {
            return _contentfulWorkflow.GetPageBySlugAsync(slug);
        }
    }
}
