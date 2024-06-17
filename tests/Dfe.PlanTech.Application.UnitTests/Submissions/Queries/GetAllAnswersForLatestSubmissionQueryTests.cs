using Dfe.PlanTech.Application.Submissions.Queries;
using Dfe.PlanTech.Domain.Answers.Models;
using Dfe.PlanTech.Domain.Responses.Models;
using Dfe.PlanTech.Domain.Submissions.Models;
using Dfe.PlanTech.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Dfe.PlanTech.Application.UnitTests.Submissions.Queries;

public class GetAllAnswersForLatestSubmissionQueryTests
{
    [Fact]
    public async Task GetAllAnswersForLatestSubmission_ReturnsAnswers()
    {
        const string section = "broadband-connection";
        const int establishmentId = 123;

        var responses = new List<Response>
        {
            new Response()
            {
                AnswerId = 1,
                QuestionId = 1,
                UserId = 1,
                SubmissionId = 1,
                Maturity = "low",
                DateCreated = DateTime.UtcNow.AddHours(-1),
                DateLastUpdated = null
            },
            new Response()
            {
                AnswerId = 2,
                QuestionId = 2,
                UserId = 2,
                SubmissionId = 1,
                Maturity = "medium",
                DateCreated = DateTime.UtcNow,
                DateLastUpdated = null
            },
            new Response()
            {
                AnswerId = 3,
                QuestionId = 1,
                UserId = 1,
                SubmissionId = 2,
                Maturity = "low",
                DateCreated = DateTime.UtcNow,
                DateLastUpdated = null
            },
            new Response()
            {
                AnswerId = 4,
                QuestionId = 2,
                UserId = 2,
                SubmissionId = 2,
                Maturity = "medium",
                DateCreated = DateTime.UtcNow,
                DateLastUpdated = null
            },
            new Response()
            {
                AnswerId = 5,
                QuestionId = 1,
                UserId = 2,
                SubmissionId = 3,
                Maturity = "medium",
                DateCreated = DateTime.UtcNow,
                DateLastUpdated = null
            }
        };

        var submissions = new List<Submission>
        {
            new Submission()
            {
                SectionId = section,
                SectionName = "Bob",
                EstablishmentId = establishmentId,
                Completed = true,
                DateCreated = DateTime.UtcNow.AddHours(-1),
                Deleted = false
            },
            new Submission()
            {
                SectionId = section,
                SectionName = "Bob",
                EstablishmentId = establishmentId,
                Completed = true,
                DateCreated = DateTime.UtcNow.AddHours(-1),
                Deleted = false
            },
            new Submission()
            {
                SectionId = section,
                SectionName = "Bob",
                EstablishmentId = establishmentId,
                Completed = true,
                DateCreated = DateTime.UtcNow,
                Deleted = true
            }
        };

        var answers = new List<Answer>
        {
            new Answer()
            {
                Id = 1,
                AnswerText = "Answer 1",
                ContentfulRef = "Ref 1"
            },
            new Answer()
            {
                Id = 2,
                AnswerText = "Answer 2",
                ContentfulRef = "Ref 1"
            },
            new Answer()
            {
                Id = 3,
                AnswerText = "Answer 3",
                ContentfulRef = "Ref 1"
            },
            new Answer()
            {
                Id = 4,
                AnswerText = "Answer 4",
                ContentfulRef = "Ref 1"
            },
            new Answer()
            {
                Id = 5,
                AnswerText = "Answer 5",
                ContentfulRef = "Ref 1"
            }
        };

        var options = new DbContextOptionsBuilder<PlanTechDbContext>().UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString()).Options;

        await using var dbContext = new PlanTechDbContext(options);
        dbContext.AddRange(submissions);
        dbContext.AddRange(responses);
        dbContext.AddRange(answers);
        await dbContext.SaveChangesAsync();
        var query = new GetAllAnswersForLatestSubmissionQuery(dbContext);

        var result = await query.GetAllAnswersForLatestSubmission(section, establishmentId);

        Assert.NotNull(result);
        Assert.Equal(2, result.Count);
        Assert.Contains(result, a => a.Id == 3);
        Assert.Contains(result, a => a.Id == 4);
    }
}