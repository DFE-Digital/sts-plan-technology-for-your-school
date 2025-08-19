using Dfe.PlanTech.Application.Workflows;
using Dfe.PlanTech.Core.Contentful.Models;

namespace Dfe.PlanTech.Application.Services
{
    public class RecommendationService(
        ContentfulWorkflow contentfulWorkflow,
        RecommendationWorkflow recommendationWorkflow,
        SubmissionWorkflow submissionWorkflow
    )
    {
        private readonly ContentfulWorkflow _contentfulWorkflow = contentfulWorkflow ?? throw new ArgumentNullException(nameof(contentfulWorkflow));
        private readonly RecommendationWorkflow _recommendationWorkflow = recommendationWorkflow ?? throw new ArgumentNullException(nameof(recommendationWorkflow));
        private readonly SubmissionWorkflow _submissionWorkflow = submissionWorkflow ?? throw new ArgumentNullException(nameof(submissionWorkflow));

        public Task<int> GetRecommendationChunkCount(int page)
        {
            return _recommendationWorkflow.GetRecommendationChunkCount(page);
        }

        public async Task<string> GetRecommendationIntroSlug(int establishmentId, string sectionSlug)
        {
            var cmsQuestionnaireSection = await _contentfulWorkflow.GetSectionBySlugAsync(sectionSlug);

            var latestCompletedSubmission = await _submissionWorkflow.GetLatestSubmissionWithOrderedResponsesAsync(
               establishmentId,
               cmsQuestionnaireSection,
               isCompletedSubmission: true);

            if (latestCompletedSubmission is null)
            {
                throw new InvalidDataException($"No incomplete responses found for section with ID {cmsQuestionnaireSection.Sys.Id}.");
            }

            if (latestCompletedSubmission.Maturity is null)
            {
                throw new InvalidDataException($"No maturity recorded for submission with ID {latestCompletedSubmission.Id}.");
            }

            var maturity = latestCompletedSubmission.Maturity;
            var introSlugForMaturity = await _contentfulWorkflow.GetIntroForMaturityAsync(cmsQuestionnaireSection.Sys.Id, maturity);
            if (introSlugForMaturity is null)
            {
                throw new InvalidDataException($"No recommendation intro found maturity {maturity} for section with ID {cmsQuestionnaireSection.Sys.Id}.");
            }

            return introSlugForMaturity.Slug;
        }

        public Task<IEnumerable<RecommendationChunkEntry>> GetPaginatedRecommendationEntries(int page)
        {
            return _recommendationWorkflow.GetPaginatedRecommendationEntries(page);
        }
    }
}
