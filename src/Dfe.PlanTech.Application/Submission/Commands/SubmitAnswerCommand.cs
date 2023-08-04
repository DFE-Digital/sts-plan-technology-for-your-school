using System.Collections.Generic;
using Dfe.PlanTech.Application.Response.Interface;
using Dfe.PlanTech.Application.Submission.Queries;
using Dfe.PlanTech.Domain.Answers.Models;
using Dfe.PlanTech.Domain.Questionnaire.Models;
using Dfe.PlanTech.Domain.Questions.Models;
using Dfe.PlanTech.Domain.Responses.Models;

namespace Dfe.PlanTech.Application.Submission.Commands
{
    public class SubmitAnswerCommand
    {
        private readonly GetSubmitAnswerQueries _getSubmitAnswerQueries;
        private readonly RecordSubmitAnswerCommands _recordSubmitAnswerCommands;
        private readonly IGetLatestResponseListForSubmissionQuery _getLatestResponseListForSubmissionQuery;

        public SubmitAnswerCommand(GetSubmitAnswerQueries getSubmitAnswerQueries, RecordSubmitAnswerCommands recordSubmitAnswerCommands, IGetLatestResponseListForSubmissionQuery getLatestResponseListForSubmissionQuery)
        {
            _getSubmitAnswerQueries = getSubmitAnswerQueries;
            _recordSubmitAnswerCommands = recordSubmitAnswerCommands;
            _getLatestResponseListForSubmissionQuery = getLatestResponseListForSubmissionQuery;
        }

        public async Task<int> SubmitAnswer(SubmitAnswerDto submitAnswerDto, string sectionId, string sectionName)
        {
            int userId = await _getSubmitAnswerQueries.GetUserId();

            int submissionId = await _GetSubmissionId(submitAnswerDto.SubmissionId, sectionId, sectionName);

            int? questionId = await _recordSubmitAnswerCommands.RecordQuestion(new RecordQuestionDto()
            {
                QuestionText = await _GetQuestionTextById(submitAnswerDto.QuestionId),
                ContentfulRef = submitAnswerDto.QuestionId
            });

            int? answerId = await _recordSubmitAnswerCommands.RecordAnswer(new RecordAnswerDto()
            {
                AnswerText = await _GetAnswerTextById(submitAnswerDto.QuestionId, submitAnswerDto.ChosenAnswerId),
                ContentfulRef = submitAnswerDto.ChosenAnswerId
            });

            string maturity = await _GetMaturityForAnswer(submitAnswerDto.QuestionId, submitAnswerDto.ChosenAnswerId);

            await _recordSubmitAnswerCommands.RecordResponse(new RecordResponseDto()
            {
                UserId = userId,
                SubmissionId = submissionId,
                QuestionId = questionId ?? throw new NullReferenceException(nameof(questionId)),
                AnswerId = answerId ?? throw new NullReferenceException(nameof(answerId)),
                Maturity = maturity
            });

            return submissionId;
        }

        public async Task<string?> GetNextQuestionId(string questionId, string chosenAnswerId)
        {
            var question = await _getSubmitAnswerQueries.GetQuestionnaireQuestion(questionId, null, CancellationToken.None);

            return question.Answers.First(answer => answer.Sys.Id.Equals(chosenAnswerId)).NextQuestion?.Sys.Id ?? null;
        }

        public async Task<bool> NextQuestionIsAnswered(int submissionId, string nextQuestionId)
        {
            var responseList = await _getSubmitAnswerQueries.GetResponseList(submissionId);

            foreach (Domain.Responses.Models.Response response in responseList ?? throw new NullReferenceException(nameof(responseList)))
            {
                Domain.Questions.Models.Question? question = await _getSubmitAnswerQueries.GetResponseQuestion(response.QuestionId) ?? throw new NullReferenceException(nameof(question));
                if (question.ContentfulRef.Equals(nextQuestionId)) return true;
            }

            return false;
        }

        public async Task<Domain.Questionnaire.Models.Question?> GetNextUnansweredQuestion(Domain.Submissions.Models.Submission submission)
        {
            List<QuestionWithAnswer> questionWithAnswerList = await _getLatestResponseListForSubmissionQuery.GetResponseListByDateCreated(submission.Id);

            QuestionWithAnswer latestQuestionWithAnswer = questionWithAnswerList.First();

            var question = await _getSubmitAnswerQueries.GetQuestionnaireQuestion(latestQuestionWithAnswer.QuestionRef, null, CancellationToken.None);

            var nextQuestion = question.Answers.FirstOrDefault(answer => answer.Sys.Id.Equals(latestQuestionWithAnswer.AnswerRef))?.NextQuestion;

            if (nextQuestion == null) return null;

            return questionWithAnswerList.Find(questionWithAnswer => questionWithAnswer.QuestionRef.Equals(nextQuestion.Sys.Id)) == null ? nextQuestion : null;
        }

        public async Task<Domain.Questionnaire.Models.Question> GetQuestionnaireQuestion(string questionId, string? section, CancellationToken cancellationToken)
        {
            return await _getSubmitAnswerQueries.GetQuestionnaireQuestion(questionId, section, cancellationToken);
        }

        public async Task<Domain.Submissions.Models.Submission?> GetOngoingSubmission(string sectionId)
        {
            int establishmentId = await _getSubmitAnswerQueries.GetEstablishmentId();
            return await _getSubmitAnswerQueries.GetSubmission(establishmentId, sectionId);
        }

        private async Task<int> _GetSubmissionId(int? submissionId, string sectionId, string sectionName)
        {
            if (submissionId == null || submissionId == 0)
            {
                return await _recordSubmitAnswerCommands.RecordSubmission(new Domain.Submissions.Models.Submission()
                {
                    EstablishmentId = await _getSubmitAnswerQueries.GetEstablishmentId(),
                    SectionId = sectionId,
                    SectionName = sectionName
                });
            }
            else return Convert.ToUInt16(submissionId);
        }

        private async Task<string?> _GetQuestionTextById(string questionId)
        {
            var question = await _getSubmitAnswerQueries.GetQuestionnaireQuestion(questionId, null, CancellationToken.None);
            return question.Text;
        }

        private async Task<string?> _GetAnswerTextById(string questionId, string chosenAnswerId)
        {
            var question = await _getSubmitAnswerQueries.GetQuestionnaireQuestion(questionId, null, CancellationToken.None);
            foreach (var answer in question.Answers)
            {
                if (answer.Sys?.Id == chosenAnswerId) return answer.Text;
            }
            return null;
        }

        private async Task<string> _GetMaturityForAnswer(string questionId, string chosenAnswerId)
        {
            if (string.IsNullOrEmpty(questionId) || string.IsNullOrEmpty(chosenAnswerId))
                return string.Empty;

            var question = await _getSubmitAnswerQueries.GetQuestionnaireQuestion(questionId, null, CancellationToken.None);

            var answer = Array.Find(question.Answers, x => x.Sys?.Id == chosenAnswerId);

            return answer != null ? answer.Maturity : string.Empty;
        }
    }
}
