using System.Data;
using Dfe.PlanTech.Application.Persistence.Interfaces;
using Dfe.PlanTech.Application.Responses.Interface;
using Dfe.PlanTech.Domain.Responses.Models;
using Microsoft.Data.SqlClient;

namespace Dfe.PlanTech.Application.Responses.Commands
{
    public class CreateResponseCommand : ICreateResponseCommand
    {
        private readonly IPlanTechDbContext _db;

        public CreateResponseCommand(IPlanTechDbContext db)
        {
            _db = db;
        }

        //TODO: REMOVE
        public async Task<int> CreateResponse(RecordResponseDto recordResponseDto)
        {
            var response = new Domain.Responses.Models.Response()
            {
                UserId = recordResponseDto.UserId,
                SubmissionId = recordResponseDto.SubmissionId,
                Question = new Domain.Questions.Models.Question()
                {
                    QuestionText = recordResponseDto.Question.Text,
                    ContentfulRef = recordResponseDto.Question.Id,
                },
                Answer = new Domain.Answers.Models.Answer()
                {
                    AnswerText = recordResponseDto.Answer.Text,
                    ContentfulRef = recordResponseDto.Answer.Id,
                },
                Maturity = recordResponseDto.Maturity,
            };

            _db.AddResponse(response);
            await _db.SaveChangesAsync();
            return response.Id;
        }

        public async Task<int> CreateResponsNew(RecordResponseDto recordResponseDto)
        {
            //TODO: get away from using SQL specific things in application layer!
            var establishmentId = new SqlParameter("@establishmentId", recordResponseDto.EstablishmentId);
            var userId = new SqlParameter("@userId", recordResponseDto.UserId);
            var sectionId = new SqlParameter("@sectionId", recordResponseDto.SectionId);
            var sectionName = new SqlParameter("@sectionName", recordResponseDto.SectionName);
            var questionContentfulId = new SqlParameter("@questionContentfulId", recordResponseDto.Question.Id);
            var questionText = new SqlParameter("@questionText", recordResponseDto.Question.Text);
            var answerContentfulId = new SqlParameter("@answerContentfulId", recordResponseDto.Answer.Id);
            var answerText = new SqlParameter("@answerText", recordResponseDto.Answer.Text);
            var maturity = new SqlParameter("@maturity", recordResponseDto.Maturity);

            var responseId = new SqlParameter("@responseId", SqlDbType.Int)
            {
                Direction = ParameterDirection.Output
            };

            var submissionId = new SqlParameter("@submissionId", SqlDbType.Int)
            {
                Direction = ParameterDirection.Output
            };



            await _db.ExecuteRaw($@"EXEC SubmitAnswer
                                            @establishmentId={establishmentId},
                                            @userId={userId},
                                            @sectionId={sectionId},
                                            @sectionName={sectionName},
                                            @questionContentfulId={questionContentfulId},
                                            @questionText={questionText},
                                            @answerContentfulId={answerContentfulId},
                                            @answerText={answerText},
                                            @maturity={maturity},
                                            @responseId={responseId} OUTPUT,
                                            @submissionId={submissionId} OUTPUT");

            if (responseId.Value is int id){
                return id;
            }

            throw new InvalidCastException($"responseId is not int - is {responseId.Value}");
        }
    }
}