using Dfe.PlanTech.Application.Response.Interface;
using Dfe.PlanTech.Application.Submission.Interfaces;
using Dfe.PlanTech.Domain.Answers.Models;
using Dfe.PlanTech.Domain.Questions.Models;
using Dfe.PlanTech.Domain.Responses.Models;

namespace Dfe.PlanTech.Application.Submission.Commands
{
    public class RecordSubmitAnswerCommands
    {

        private readonly IRecordQuestionCommand _recordQuestionCommand;
        private readonly IRecordAnswerCommand _recordAnswerCommand;
        private readonly ICreateSubmissionCommand _createSubmissionCommand;
        private readonly ICreateResponseCommand _createResponseCommand;

        public RecordSubmitAnswerCommands(
            IRecordQuestionCommand recordQuestionCommand,
            IRecordAnswerCommand recordAnswerCommand,
            ICreateSubmissionCommand createSubmissionCommand,
            ICreateResponseCommand createResponseCommand)
        {
            _recordQuestionCommand = recordQuestionCommand;
            _recordAnswerCommand = recordAnswerCommand;
            _createSubmissionCommand = createSubmissionCommand;
            _createResponseCommand = createResponseCommand;
        }

        public async Task<int> RecordQuestion(RecordQuestionDto recordQuestionDto)
        {
            if (recordQuestionDto.QuestionText == null) throw new NullReferenceException(nameof(recordQuestionDto));
            return await _recordQuestionCommand.RecordQuestion(recordQuestionDto);
        }

        public async Task<int> RecordAnswer(RecordAnswerDto recordAnswerDto)
        {
            if (recordAnswerDto.AnswerText == null) throw new NullReferenceException(nameof(recordAnswerDto));
            return await _recordAnswerCommand.RecordAnswer(recordAnswerDto);
        }

        public async Task<int> RecordSubmission(Domain.Submissions.Models.Submission submission)
        {
            if (submission == null) throw new ArgumentNullException(nameof(submission));
            return await _createSubmissionCommand.CreateSubmission(submission);
        }

        public async Task RecordResponse(RecordResponseDto recordResponseDto)
        {
            if (recordResponseDto == null) throw new ArgumentNullException(nameof(recordResponseDto));
            await _createResponseCommand.CreateResponse(recordResponseDto);
        }

    }
}