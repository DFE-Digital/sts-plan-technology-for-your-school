using System.Text.Json;
using Dfe.PlanTech.AzureFunctions.Mappings;
using Dfe.PlanTech.Domain.Questionnaire.Models;
using Dfe.PlanTech.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace Dfe.PlanTech.AzureFunctions.UnitTests.Mappers;

public class RecommendationSectionMapperTests : BaseMapperTests
{
    
    private readonly DbSet<RecommendationSectionAnswerDbEntity> _sectionAnswerDbSet = Substitute.For<DbSet<RecommendationSectionAnswerDbEntity>>();
    private readonly DbSet<RecommendationSectionChunkDbEntity> _sectionChunkDbSet = Substitute.For<DbSet<RecommendationSectionChunkDbEntity>>();


    [Fact]
    public void RecommendationSectionsAreMappedCorrectly()  
    {
        var values = new Dictionary<string, object?>
        {
            ["id"] = "sectionId",
            ["chunks"] = new string[] { "chunk1", "chunk2", "chunk3" },
            ["answers"] = new string[] { "answer1", "answer2", "answer3" }
        };

        var dbSubstitute = Substitute.For<CmsDbContext>();
        var loggerSubstitute = Substitute.For<ILogger<RecommendationSectionMapper>>();
        var jsonSerializerOptions = new JsonSerializerOptions();

        dbSubstitute.RecommendationSectionChunks = _sectionChunkDbSet;
        dbSubstitute.RecommendationSectionAnswers = _sectionAnswerDbSet;

        var mapper = new RecommendationSectionMapper(MapperHelpers.CreateMockEntityRetriever(),
            MapperHelpers.CreateMockEntityUpdater(), dbSubstitute, loggerSubstitute, jsonSerializerOptions);

        var recommendationChunk = mapper.PerformAdditionalMapping(values);

        Assert.NotNull(recommendationChunk);
        
        dbSubstitute.RecommendationSectionAnswers.Received(3).Attach(Arg.Any<RecommendationSectionAnswerDbEntity>());
        dbSubstitute.RecommendationSectionChunks.Received(3).Attach(Arg.Any<RecommendationSectionChunkDbEntity>());
    }
}