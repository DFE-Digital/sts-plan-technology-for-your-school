using Dfe.PlanTech.Domain.Questionnaire.Interfaces;
using Dfe.PlanTech.Domain.Questionnaire.Models;
using Dfe.PlanTech.Domain.Submissions.Models;

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

        public IEnumerable<QuestionWithAnswer> SubmissionResponses { get; init; } = null!;
    }
}
