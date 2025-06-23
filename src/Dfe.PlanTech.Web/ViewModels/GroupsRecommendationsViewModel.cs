using Dfe.PlanTech.Domain.ContentfulEntries.Questionnaire.Interfaces;
using Dfe.PlanTech.Domain.ContentfulEntries.Questionnaire.Models;
using Dfe.PlanTech.Infrastructure.Data.Contentful.Models;

namespace Dfe.PlanTech.Web.Models
{
    public class GroupsRecommendationsViewModel
    {
        public string SelectedEstablishmentName { get; set; } = null!;

        public int SelectedEstablishmentId { get; set; }

        public string SectionName { get; init; } = null!;

        public RecommendationIntro? Intro { get; init; } = null!;

        public List<RecommendationChunk> Chunks { get; init; } = null!;

        public string Slug { get; init; } = null!;

        public IHeaderWithContent? GroupsCustomRecommendationIntro { get; init; }

        public IEnumerable<IHeaderWithContent> AllContent => GetAllContent();

        private IEnumerable<IHeaderWithContent> GetAllContent()
        {
            if (Intro != null)
            {
                yield return Intro;
            }

            if (GroupsCustomRecommendationIntro != null)
            {
                yield return GroupsCustomRecommendationIntro;
            }

            foreach (var chunk in Chunks)
            {
                yield return chunk;
            }
        }

        public IEnumerable<Workflows.Models.QuestionWithAnswerModel> SubmissionResponses { get; init; } = null!;
    }
}
