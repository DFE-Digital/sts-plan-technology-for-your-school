using Dfe.PlanTech.Application.Questionnaire.Queries;
using Dfe.PlanTech.Application.Response.Interface;
using Dfe.PlanTech.Application.Submission.Interfaces;
using Dfe.PlanTech.Domain.Questionnaire.Models;

namespace Dfe.PlanTech.Application.Response.Commands
{
    public class ProcessCheckAnswerDtoCommand
    {
        private readonly GetSectionQuery _getSectionQuery;
        private readonly IGetLatestResponseListForSubmissionQuery _getLatestResponseListForSubmissionQuery;
        private readonly ICalculateMaturityCommand _calculateMaturityCommand;

        public ProcessCheckAnswerDtoCommand(
            GetSectionQuery getSectionQuery,
            IGetLatestResponseListForSubmissionQuery getLatestResponseListForSubmissionQuery,
            ICalculateMaturityCommand calculateMaturityCommand)
        {
            _getSectionQuery = getSectionQuery;
            _getLatestResponseListForSubmissionQuery = getLatestResponseListForSubmissionQuery;
            _calculateMaturityCommand = calculateMaturityCommand;
        }

        //TODO: Rename
        //TODO: Pass in Section object
        public async Task<CheckAnswerDto?> GetCheckAnswerDtoForSectionId(int establishmentId, string sectionId, CancellationToken cancellationToken = default)
        {
            var questionWithAnswerList = await _getLatestResponseListForSubmissionQuery.GetLatestResponses(establishmentId, sectionId, cancellationToken);
            if (questionWithAnswerList.Responses == null || !questionWithAnswerList.Responses.Any())
            {
                return null;
            }

            return await RemoveDetachedQuestions(questionWithAnswerList.Responses, sectionId, questionWithAnswerList.Id, cancellationToken);
        }


        public Task CalculateMaturityAsync(int submissionId) => _calculateMaturityCommand.CalculateMaturityAsync(submissionId);

        private async Task<CheckAnswerDto> RemoveDetachedQuestions(List<QuestionWithAnswer> questionWithAnswerList, string sectionId, int submissionId, CancellationToken cancellationToken)
        {
            if (questionWithAnswerList == null) throw new ArgumentNullException(nameof(questionWithAnswerList));
            if (sectionId == null) throw new ArgumentNullException(nameof(sectionId));

            CheckAnswerDto checkAnswerDto = new()
            {
                SubmissionId = submissionId,
                QuestionAnswerList = new List<QuestionWithAnswer>(questionWithAnswerList.Count)
            };

            var questionWithAnswerMap = questionWithAnswerList.ToDictionary(questionWithAnswer => questionWithAnswer.QuestionRef, 
                                                                            questionWithAnswer => questionWithAnswer);

            Section section = await _getSectionQuery.GetSectionById(sectionId, cancellationToken) ?? throw new KeyNotFoundException(sectionId);

            Question? node = section.Questions[0];

            while (node != null)
            {
                if (questionWithAnswerMap.TryGetValue(node.Sys.Id, out QuestionWithAnswer? questionWithAnswer))
                {
                    var answer = Array.Find(node.Answers, answer => answer.Sys.Id == questionWithAnswer.AnswerRef);
                    questionWithAnswer = questionWithAnswer with
                    {
                        AnswerText = answer?.Text ?? questionWithAnswer.AnswerText,
                        QuestionText = node.Text,
                        QuestionSlug = node.Slug
                    };

                    checkAnswerDto.QuestionAnswerList.Add(questionWithAnswer);
                    node = node.Answers.FirstOrDefault(answer => answer.Sys.Id.Equals(questionWithAnswer.AnswerRef))?.NextQuestion;
                }
                else node = null;
            }

            return checkAnswerDto;
        }
    }
}