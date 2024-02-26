using Dfe.PlanTech.Application.Persistence.Interfaces;
using Dfe.PlanTech.Application.Submissions.Queries;
using Dfe.PlanTech.Domain.Answers.Models;
using NSubstitute;

namespace Dfe.PlanTech.Application.UnitTests.Submissions.Queries;

public class GetAllAnswersForSubmissionQueryTests
{
    private IPlanTechDbContext _dbSubstitute = Substitute.For<IPlanTechDbContext>();
    
    private GetAllAnswersForSubmissionQuery CreateStrut() => new (_dbSubstitute);
    
    public GetAllAnswersForSubmissionQueryTests()
    {
        var answerResponses = new List<Answer>()
        {
            new Answer(){Id = 1},
            new Answer(){Id = 2}
        };
        
        _dbSubstitute.GetAnswersForLatestSubmissionBySectionId("test", 1).Returns(answerResponses);
    }
    
    
    [Fact]
    public async Task Answers_Are_Returned_By_Query_If_Section_Exists()
    {
        var establishmentId = 1;
        var section = "test";

        var result = await CreateStrut().GetAllAnswersForLatestSubmission(section,establishmentId);

        Assert.NotNull(result);
    }
    
    
    [Fact]
    public async Task Null_Is_Returned_By_Query_If_Section_Does_Not_Exist()
    {
        var establishmentId = 1;
        var section = "";

        var result = await CreateStrut().GetAllAnswersForLatestSubmission(section,establishmentId);

        Assert.Null(result);
    }
    
    
  
}