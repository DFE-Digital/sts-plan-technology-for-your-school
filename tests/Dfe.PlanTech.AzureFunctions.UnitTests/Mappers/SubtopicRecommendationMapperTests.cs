using System.Text.Json;
using Dfe.PlanTech.AzureFunctions.Mappings;
using Dfe.PlanTech.Domain.Questionnaire.Models;
using Dfe.PlanTech.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace Dfe.PlanTech.AzureFunctions.UnitTests.Mappers;

public class SubtopicRecommendationMapperTests : BaseMapperTests
{
    private readonly DbSet<SubtopicRecommendationIntroDbEntity> _subtopicIntroDbSet = Substitute.For<DbSet<SubtopicRecommendationIntroDbEntity>>();
    
    [Fact]
    public void SubtopicRecommendationsAreMappedCorrectly()
    {
        var values = new Dictionary<string, object?>
        {
            ["id"] = "ChunkId",
            ["section"] =  "sectionId",
            ["subtopic"] =  "subtopicId",
            ["intros"] = new string[] { "intro1", "intro2", "intro3" },
        };
        
        var dbSubstitute = Substitute.For<CmsDbContext>();
        var loggerSubstitute = Substitute.For<ILogger<SubtopicRecommendationMapper>>();
        var jsonSerializerOptions = new JsonSerializerOptions();

        dbSubstitute.SubtopicRecommendationIntros = _subtopicIntroDbSet;

        var mapper = new SubtopicRecommendationMapper(MapperHelpers.CreateMockEntityRetriever(),
            MapperHelpers.CreateMockEntityUpdater(), dbSubstitute, loggerSubstitute, jsonSerializerOptions);

        var subtopicRecommendationContents = mapper.PerformAdditionalMapping(values);

        Assert.NotNull(subtopicRecommendationContents);
        
        dbSubstitute.SubtopicRecommendationIntros.Received(3).Attach(Arg.Any<SubtopicRecommendationIntroDbEntity>());
    }
    
}