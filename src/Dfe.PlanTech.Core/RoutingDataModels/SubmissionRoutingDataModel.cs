using Dfe.PlanTech.Core.Contentful.Models;
using Dfe.PlanTech.Core.Enums;
using Dfe.PlanTech.Core.Exceptions;
using Dfe.PlanTech.Core.Models;

namespace Dfe.PlanTech.Core.RoutingDataModels
{
    public class SubmissionRoutingDataModel
    {
        public string? Maturity { get; set; }
        public QuestionnaireQuestionEntry? NextQuestion { get; set; }
        public QuestionnaireSectionEntry QuestionnaireSection { get; set; } = null!;
        public SubmissionResponsesModel? Submission { get; set; }
        public SubmissionStatus Status { get; init; }

        public SubmissionRoutingDataModel(
            string? maturity,
            QuestionnaireQuestionEntry? nextQuestion,
            QuestionnaireSectionEntry questionnaireSection,
            SubmissionResponsesModel? submission,
            SubmissionStatus status
        )
        {
            Maturity = maturity;
            NextQuestion = nextQuestion;
            QuestionnaireSection = questionnaireSection;
            Submission = submission;
            Status = status;
        }

        public bool IsQuestionInResponses(string questionSysId) =>
            Submission?.Responses.Any(response => response.QuestionSysId.Equals(questionSysId)) ?? false;

        public QuestionWithAnswerModel GetLatestResponseForQuestion(string questionSysId) =>
            Submission?.Responses.FirstOrDefault(response => response.QuestionSysId.Equals(questionSysId))
                ?? throw new DatabaseException($"Could not find response for question entry with sys ID '{questionSysId}'");

        public QuestionnaireQuestionEntry GetQuestionForSlug(string questionSlug) =>
            QuestionnaireSection.Questions.FirstOrDefault(q => q.Slug == questionSlug)
                ?? throw new ContentfulDataUnavailableException($"Could not find question with slug '{questionSlug}'");
    }
}
