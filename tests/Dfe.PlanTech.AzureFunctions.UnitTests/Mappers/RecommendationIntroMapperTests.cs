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
    
    private static readonly CmsDbContext _db = Substitute.For<CmsDbContext>();
    private static readonly ILogger<RecommendationIntroUpdater> _logger = Substitute.For<ILogger<RecommendationIntroUpdater>>();
    private static RecommendationIntroUpdater CreateMockRecommendationIntroUpdater() => new(_logger, _db);
    
    [Fact]
    public void RecommendationIntrosAreMappedCorrectly()
    {
        var values = new Dictionary<string, object?>
        {
            ["id"] = "ChunkId",
            ["header"] =  "HeaderId",
            ["content"] = new string[] { "content1", "content2", "content3" },
        };

        var loggerSubstitute = Substitute.For<ILogger<RecommendationIntroMapper>>();
        var jsonSerializerOptions = new JsonSerializerOptions();

        _db.RecommendationIntroContents = _introContentDbSet;

        var mapper = new RecommendationIntroMapper(MapperHelpers.CreateMockEntityRetriever(),
            CreateMockRecommendationIntroUpdater(), _db, loggerSubstitute, jsonSerializerOptions);

        var recommendationIntro = mapper.PerformAdditionalMapping(values);

        Assert.NotNull(recommendationIntro);
        
        _db.RecommendationIntroContents.Received(3).Attach(Arg.Any<RecommendationIntroContentDbEntity>());
    }
}