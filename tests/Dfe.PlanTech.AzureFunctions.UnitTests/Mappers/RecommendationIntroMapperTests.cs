using System.Text.Json;
using Dfe.PlanTech.AzureFunctions.Mappings;
using Dfe.PlanTech.Domain.Questionnaire.Models;
using Dfe.PlanTech.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace Dfe.PlanTech.AzureFunctions.UnitTests.Mappers;

public class RecommendationIntroMapperTests : BaseMapperTests
{
        
    private readonly DbSet<RecommendationIntroContentDbEntity> _introContentDbSet = Substitute.For<DbSet<RecommendationIntroContentDbEntity>>();
    
    [Fact]
    public void RecommendationIntrosAreMappedCorrectly()
    {
        var values = new Dictionary<string, object?>
        {
            ["id"] = "introId",
            ["slug"] = "test-slug",
            ["header"] =  "HeaderId",
            ["content"] = new string[] { "content1", "content2", "content3" },
        };

        var dbSubstitute = Substitute.For<CmsDbContext>();
        var loggerSubstitute = Substitute.For<ILogger<RecommendationIntroMapper>>();
        var jsonSerializerOptions = new JsonSerializerOptions();

        dbSubstitute.RecommendationIntroContents = _introContentDbSet;

        var mapper = new RecommendationIntroMapper(MapperHelpers.CreateMockEntityRetriever(),
            MapperHelpers.CreateMockEntityUpdater(), dbSubstitute, loggerSubstitute, jsonSerializerOptions);

        var recommendationIntro = mapper.PerformAdditionalMapping(values);

        Assert.NotNull(recommendationIntro);
        
        dbSubstitute.RecommendationIntroContents.Received(3).Attach(Arg.Any<RecommendationIntroContentDbEntity>());
    }
}