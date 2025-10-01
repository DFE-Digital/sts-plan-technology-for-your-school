namespace Dfe.PlanTech.Data.Sql.IntegrationTests.Repositories;

public class RecommendationRepositoryTests : DatabaseIntegrationTestBase
{
    private RecommendationRepository _repository = null!;

    public RecommendationRepositoryTests(DatabaseFixture fixture) : base(fixture)
    {
    }

    public override async Task InitializeAsync()
    {
        await base.InitializeAsync();
        _repository = new RecommendationRepository(DbContext);
    }
}
