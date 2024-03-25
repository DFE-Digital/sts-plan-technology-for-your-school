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

    private static readonly CmsDbContext _db = Substitute.For<CmsDbContext>();
    private static readonly ILogger<SubtopicRecommendationUpdater> _logger = Substitute.For<ILogger<SubtopicRecommendationUpdater>>();
    private static SubtopicRecommendationUpdater CreateMockSubTopicRecommendationUpdater() => new(_logger, _db);
    [Fact]
    public void SubtopicRecommendationsAreMappedCorrectly()
    {
        var values = new Dictionary<string, object?>
        {
            ["id"] = "ChunkId",
            ["section"] = "sectionId",
            ["subtopic"] = "subtopicId",
            ["intros"] = new string[] { "intro1", "intro2", "intro3" },
        };

        var _db = Substitute.For<CmsDbContext>();
        var loggerSubstitute = Substitute.For<ILogger<SubtopicRecommendationMapper>>();
        var jsonSerializerOptions = new JsonSerializerOptions();

        _db.SubtopicRecommendationIntros = _subtopicIntroDbSet;

        var mapper = new SubtopicRecommendationMapper(MapperHelpers.CreateMockEntityRetriever(),
            CreateMockSubTopicRecommendationUpdater(), _db, loggerSubstitute, jsonSerializerOptions);

        var subtopicRecommendationContents = mapper.PerformAdditionalMapping(values);

        Assert.NotNull(subtopicRecommendationContents);

        _db.SubtopicRecommendationIntros.Received(3).Attach(Arg.Any<SubtopicRecommendationIntroDbEntity>());
    }

}