using Dfe.PlanTech.AzureFunctions.Mappings;
using Dfe.PlanTech.Infrastructure.Data;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace Dfe.PlanTech.AzureFunctions.UnitTests;

public static class MapperHelpers
{
    private static readonly CmsDbContext _db = Substitute.For<CmsDbContext>();
    private static readonly ILogger<EntityUpdater> _logger = Substitute.For<ILogger<EntityUpdater>>();

    public static EntityUpdater CreateMockEntityUpdater(CmsDbContext? db = null) => new(_logger, db ?? _db);

    public static EntityRetriever CreateMockEntityRetriever(CmsDbContext? db = null) => new(db ?? _db);
}