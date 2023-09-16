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

        //TODO: DELETE
        public async Task<CheckAnswerDto> ProcessCheckAnswerDto(int submissionId, string sectionId)
        {
            List<QuestionWithAnswer> questionWithAnswerList = await _GetQuestionWithAnswerList(submissionId);
            CheckAnswerDto checkAnswerDto = await _RemoveDetachedQuestions(questionWithAnswerList, sectionId);
            return checkAnswerDto;
        }

        //TODO: Rename
        public async Task<CheckAnswerDto> GetCheckAnswerDtoForSectionId(int establishmentId, string sectionId)
        {
            var questionWithAnswerList = await _getLatestResponseListForSubmissionQuery.GetResponses(establishmentId, sectionId);
            if (questionWithAnswerList == null || !questionWithAnswerList.Any())
            {
                return new CheckAnswerDto()
                {

                };
            }

            CheckAnswerDto checkAnswerDto = await _RemoveDetachedQuestions(questionWithAnswerList!.ToList(), sectionId);
            return checkAnswerDto;
        }


        public async Task CalculateMaturityAsync(int submissionId)
        {
            await _calculateMaturityCommand.CalculateMaturityAsync(submissionId);
        }

        //TODO: DELETE
        private Task<List<QuestionWithAnswer>> _GetQuestionWithAnswerList(int submissionId)
        => _getLatestResponseListForSubmissionQuery.GetLatestResponseListForSubmissionBy(submissionId);

        private async Task<CheckAnswerDto> _RemoveDetachedQuestions(List<QuestionWithAnswer> questionWithAnswerList, string sectionId)
        {
            if (questionWithAnswerList == null) throw new ArgumentNullException(nameof(questionWithAnswerList));
            if (sectionId == null) throw new ArgumentNullException(nameof(sectionId));

            CheckAnswerDto checkAnswerDto = new CheckAnswerDto();

            Dictionary<string, QuestionWithAnswer> questionWithAnswerMap = questionWithAnswerList
                            .Select((questionWithAnswer, index) => new KeyValuePair<string, QuestionWithAnswer>(questionWithAnswer.QuestionRef, questionWithAnswerList[index]))
                            .ToDictionary(x => x.Key, x => x.Value);

            Section section = await _GetSection(sectionId) ?? throw new NullReferenceException(nameof(section));

            Question? node = section.Questions.FirstOrDefault(question => question.Sys.Id.Equals(section.FirstQuestionId));

            while (node != null)
            {
                if (questionWithAnswerMap.TryGetValue(node.Sys.Id, out QuestionWithAnswer? questionWithAnswer))
                {
                    var answer = Array.Find(node.Answers, answer => answer.Sys.Id == questionWithAnswer.AnswerRef);
                    questionWithAnswer = questionWithAnswer with
                    {
                        AnswerText = answer?.Text ?? questionWithAnswer.AnswerText,
                        QuestionText = node.Text
                    };

                    checkAnswerDto.QuestionAnswerList.Add(questionWithAnswer);
                    node = node.Answers.FirstOrDefault(answer => answer.Sys.Id.Equals(questionWithAnswer.AnswerRef))?.NextQuestion;
                }
                else node = null;
            }

            return checkAnswerDto;
        }

        private async Task<Domain.Questionnaire.Models.Section?> _GetSection(string sectionId)
        {
            if (string.IsNullOrEmpty(sectionId)) throw new ArgumentNullException(nameof(sectionId));
            return await _getSectionQuery.GetSectionById(sectionId, CancellationToken.None);
        }
    }
}