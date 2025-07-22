using Dfe.PlanTech.Core.DataTransferObjects.Contentful;
using Dfe.PlanTech.Core.Enums;
using Dfe.PlanTech.Core.Exceptions;
using Dfe.PlanTech.Core.RoutingDataModel;

namespace Dfe.PlanTech.Core.RoutingDataModels
{
    public class SubmissionRoutingDataModel
    {
        public string? Maturity { get; set; }
        public CmsQuestionnaireQuestionDto? NextQuestion { get; set; }
        public CmsQuestionnaireSectionDto QuestionnaireSection { get; set; } = null!;
        public SubmissionResponsesModel? Submission { get; set; }
        public SubmissionStatus Status { get; init; }

        public bool IsQuestionInResponses(string questionSysId) =>
            Submission?.Responses.Any(response => response.QuestionSysId.Equals(questionSysId)) ?? false;

        public QuestionWithAnswerModel GetLatestResponseForQuestion(string questionSysId) =>
            Submission?.Responses.FirstOrDefault(response => response.QuestionSysId.Equals(questionSysId))
                ?? throw new DatabaseException($"Could not find response for question entry with sys ID '{questionSysId}'");

        public CmsQuestionnaireQuestionDto GetQuestionForSlug(string questionSlug) =>
            QuestionnaireSection.Questions.FirstOrDefault(q => q.Slug == questionSlug)
                ?? throw new ContentfulDataUnavailableException($"Could not find question with slug '{questionSlug}'");
    }
}
