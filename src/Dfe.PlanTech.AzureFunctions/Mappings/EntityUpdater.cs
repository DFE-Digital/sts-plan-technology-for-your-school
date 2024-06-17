using Dfe.PlanTech.AzureFunctions.Models;
using Dfe.PlanTech.Domain;
using Dfe.PlanTech.Domain.Caching.Enums;
using Dfe.PlanTech.Domain.Content.Models;
using Dfe.PlanTech.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Dfe.PlanTech.AzureFunctions.Mappings;

public class EntityUpdater(ILogger<EntityUpdater> logger, CmsDbContext db)
{
    private readonly ILogger<EntityUpdater> _logger = logger;
    protected readonly CmsDbContext Db = db;

    /// <summary>
    /// Updates properties on the existing component (if any) based on the incoming component and the CmsEvent
    /// </summary>
    /// <param name="incoming"></param>
    /// <param name="existing"></param>
    /// <param name="cmsEvent"></param>
    /// <returns></returns>
    public MappedEntity UpdateEntity(ContentComponentDbEntity incoming, ContentComponentDbEntity? existing, CmsEvent cmsEvent)
    {
        var mappedEntity = new MappedEntity()
        {
            IncomingEntity = incoming,
            ExistingEntity = existing,
            CmsEvent = cmsEvent
        };

        mappedEntity.UpdateEntity();

        if (!mappedEntity.IsValidComponent(Db, _logger) || mappedEntity.IsMinimalPayloadEvent)
        {
            return mappedEntity;
        }

        mappedEntity = UpdateEntityConcrete(mappedEntity);

        return mappedEntity;
    }

    /// <summary>
    /// Overridable method to allow bespoke updating by entity type if needed
    /// </summary>
    /// <param name="entity"></param>
    /// <returns></returns>
    public virtual MappedEntity UpdateEntityConcrete(MappedEntity entity)
    {
        return entity;
    }
}