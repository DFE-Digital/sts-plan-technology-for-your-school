using Dfe.PlanTech.AzureFunctions.Mappings;
using Dfe.PlanTech.Infrastructure.Data;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace Dfe.PlanTech.AzureFunctions.UnitTests;

public static class MapperHelpers
{
    public static readonly CmsDbContext Db = Substitute.For<CmsDbContext>();
    public static readonly ILogger<EntityUpdater> Logger = Substitute.For<ILogger<EntityUpdater>>();

    public static EntityUpdater CreateMockEntityUpdater(CmsDbContext? db = null, ILogger<EntityUpdater>? logger = null) => new(logger ?? Logger, db ?? Db);

    public static EntityRetriever CreateMockEntityRetriever(CmsDbContext? db = null) => new(db ?? Db);
}
