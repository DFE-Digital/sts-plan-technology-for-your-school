﻿using Dfe.PlanTech.Application.Persistence.Interfaces;
using Dfe.PlanTech.Application.Response.Interface;
using Dfe.PlanTech.Domain.Responses.Models;

namespace Dfe.PlanTech.Application.Response.Commands
{
    public class CreateResponseCommand : ICreateResponseCommand
    {
        private readonly IPlanTechDbContext _db;

        public CreateResponseCommand(IPlanTechDbContext db)
        {
            _db = db;
        }

        public async Task<int> CreateResponse(RecordResponseDto recordResponseDto)
        {
            var response = new Domain.Responses.Models.Response()
            {
                UserId = recordResponseDto.UserId,
                SubmissionId = recordResponseDto.SubmissionId,
                QuestionId = recordResponseDto.QuestionId,
                AnswerId = recordResponseDto.AnswerId,
                Maturity = recordResponseDto.Maturity,
                DateCreated = DateTime.UtcNow
            };

            _db.AddResponse(response);
            await _db.SaveChangesAsync();
            return response.Id;
        }
    }
}