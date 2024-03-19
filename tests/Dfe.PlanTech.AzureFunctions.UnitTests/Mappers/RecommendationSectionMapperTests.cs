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
    private static readonly CmsDbContext _db = Substitute.For<CmsDbContext>();
    private static readonly ILogger<RecommendationSectionUpdater> _logger = Substitute.For<ILogger<RecommendationSectionUpdater>>();
    private static RecommendationSectionUpdater CreateMockRecommendationSectionUpdater() => new(_logger, _db);


    [Fact]
    public void RecommendationSectionsAreMappedCorrectly()  
    {
        var values = new Dictionary<string, object?>
        {
            ["id"] = "sectionId",
            ["chunks"] = new string[] { "chunk1", "chunk2", "chunk3" },
            ["answers"] = new string[] { "answer1", "answer2", "answer3" }
        };
        
        var loggerSubstitute = Substitute.For<ILogger<RecommendationSectionMapper>>();
        var jsonSerializerOptions = new JsonSerializerOptions();

        _db.RecommendationSectionChunks = _sectionChunkDbSet;
        _db.RecommendationSectionAnswers = _sectionAnswerDbSet;
        
        var mapper = new RecommendationSectionMapper(MapperHelpers.CreateMockEntityRetriever(),
            CreateMockRecommendationSectionUpdater(), _db, loggerSubstitute, jsonSerializerOptions);

        var recommendationChunk = mapper.PerformAdditionalMapping(values);

        Assert.NotNull(recommendationChunk);
        
        _db.RecommendationSectionAnswers.Received(3).Attach(Arg.Any<RecommendationSectionAnswerDbEntity>());
        _db.RecommendationSectionChunks.Received(3).Attach(Arg.Any<RecommendationSectionChunkDbEntity>());
    }
}