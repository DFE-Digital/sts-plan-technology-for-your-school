using Dfe.PlanTech.Web.Controllers;
using Dfe.PlanTech.Web.ViewBuilders.Interfaces;
using Dfe.PlanTech.Web.ViewModels.Inputs;
using Microsoft.Extensions.Logging.Testing;
using NSubstitute;

namespace Dfe.PlanTech.Web.UnitTests.Controllers.RecommendationsControllerTests;

public abstract class RecommendationsControllerTestBase
{
    protected const string CategorySlug = "category-slug";
    protected const string SectionSlug = "section-slug";
    protected const string ChunkSlug = "chunk-slug";

    public static TheoryData<string?, string?, string?> InvalidSlugs =>
        new()
        {
            { null, SectionSlug, ChunkSlug },
            { "", SectionSlug, ChunkSlug },
            { " ", SectionSlug, ChunkSlug },
            { CategorySlug, null, ChunkSlug },
            { CategorySlug, "", ChunkSlug },
            { CategorySlug, " ", ChunkSlug },
            { CategorySlug, SectionSlug, null },
            { CategorySlug, SectionSlug, "" },
            { CategorySlug, SectionSlug, " " },
        };

    protected static readonly ShareByEmailInputViewModel ShareInputModel = new()
    {
        NameOfUser = "Alex Morgan",
        EmailAddresses = ["recipient@example.com"],
        UserMessage = "Worth a read",
    };

    protected readonly FakeLogger<RecommendationsController> _logger = new();
    protected readonly IRecommendationsViewBuilder _recommendationsViewBuilder =
        Substitute.For<IRecommendationsViewBuilder>();

    protected RecommendationsController CreateSystemUnderTest() =>
        new(_logger, _recommendationsViewBuilder);
}
