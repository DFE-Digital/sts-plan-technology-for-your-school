using Dfe.PlanTech.Core.Models;

namespace Dfe.PlanTech.Core.DataTransferObjects
{
    public class CmsSectionDto : CmsEntryDto
    {
        public string Name { get; set; } = null!;

        public CmsPageDto? InterstitialPage { get; set; }

        public List<CmsQuestionDto> Questions { get; set; } = new();

        public required string FirstQuestionSysId { get; set; }


        public IEnumerable<QuestionWithAnswerModel> GetOrderedResponsesForJourney(IEnumerable<QuestionWithAnswerModel> responses)
        {
            var QuestionWithAnswerModelMap = responses
                    .Where(r => !string.IsNullOrWhiteSpace(r.QuestionSysId))
                    .GroupBy(r => r.QuestionSysId)
                    .ToDictionary(g => g.Key, g => g.First());

            var currentQuestion = Questions.FirstOrDefault();

            while (currentQuestion is not null)
            {
                var questionSysId = currentQuestion.Sys.Id ?? string.Empty;

                if (!QuestionWithAnswerModelMap.TryGetValue(questionSysId, out QuestionWithAnswerModel? QuestionWithAnswerModel))
                {
                    break;
                }

                var answer = GetAnswerForRef(QuestionWithAnswerModel);

                // Show the latest Text and Slug, but preserve user answer if there has been a change
                QuestionWithAnswerModel = QuestionWithAnswerModel with
                {
                    AnswerText = answer?.Text ?? QuestionWithAnswerModel.AnswerText,
                    QuestionText = currentQuestion.Text,
                    QuestionSlug = currentQuestion.Slug
                };

                yield return QuestionWithAnswerModel;

                currentQuestion = answer?.NextQuestion;
            }
        }

        private CmsAnswerDto? GetAnswerForRef(QuestionWithAnswerModel QuestionWithAnswerModel)
        {
            return Questions
                .Find(question => string.Equals(question.Sys.Id, QuestionWithAnswerModel.QuestionSysId))?
                .Answers
                .Find(answer => string.Equals(answer.Sys.Id, QuestionWithAnswerModel.AnswerSysId));
        }
    }
}
