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

        public async Task<CheckAnswerDto> ProcessCheckAnswerDto(int submissionId, string sectionId)
        {
            List<QuestionWithAnswer> questionWithAnswerList = await _GetQuestionWithAnswerList(submissionId);
            CheckAnswerDto checkAnswerDto = await _RemoveDetachedQuestions(questionWithAnswerList, sectionId);
            return checkAnswerDto;
        }

        public async Task<CheckAnswerDto> GetLatestSubmissionAnswers(string sectionName, string establishmentRef)
        {
            List<QuestionWithAnswer> questionWithAnswerList = await _getLatestResponseListForSubmissionQuery.GetLatestResponseListForSectioName(sectionName, establishmentRef);
            CheckAnswerDto checkAnswerDto = await RemoveDetachedQuestionsByName(questionWithAnswerList, sectionName);
            return checkAnswerDto;
        }


        public async Task CalculateMaturityAsync(int submissionId)
        {
            await _calculateMaturityCommand.CalculateMaturityAsync(submissionId);
        }

        private async Task<List<QuestionWithAnswer>> _GetQuestionWithAnswerList(int submissionId)
        {
            return await _getLatestResponseListForSubmissionQuery.GetLatestResponseListForSubmissionBy(submissionId);
        }

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
                checkAnswerDto.QuestionAnswerList.Add(questionWithAnswerMap[node.Sys.Id]);
                node = node.Answers.FirstOrDefault(answer => answer.Sys.Id.Equals(questionWithAnswerMap[node.Sys.Id].AnswerRef))?.NextQuestion;
            }

            return checkAnswerDto;
        }

        private async Task<CheckAnswerDto> RemoveDetachedQuestionsByName(List<QuestionWithAnswer> questionWithAnswerList, string sectionName)
        {
            if (questionWithAnswerList == null) throw new ArgumentNullException(nameof(questionWithAnswerList));
            if (string.IsNullOrEmpty(sectionName)) throw new ArgumentNullException(nameof(sectionName));

            CheckAnswerDto checkAnswerDto = new CheckAnswerDto();

            Dictionary<string, QuestionWithAnswer> questionWithAnswerMap = questionWithAnswerList
                            .Select((questionWithAnswer, index) => new KeyValuePair<string, QuestionWithAnswer>(questionWithAnswer.QuestionRef, questionWithAnswerList[index]))
                            .ToDictionary(x => x.Key, x => x.Value);

            Section? section = await _getSectionQuery.GetSectionByName(sectionName, CancellationToken.None);

            if(section == null){
                throw new KeyNotFoundException($"Could not find section with name {sectionName}");
            }

            Question? node = section.Questions.FirstOrDefault(question => question.Sys.Id.Equals(section.FirstQuestionId));

            while (node != null)
            {
                checkAnswerDto.QuestionAnswerList.Add(questionWithAnswerMap[node.Sys.Id]);
                node = node.Answers.FirstOrDefault(answer => answer.Sys.Id.Equals(questionWithAnswerMap[node.Sys.Id].AnswerRef))?.NextQuestion;
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