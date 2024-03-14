using Dfe.PlanTech.AzureFunctions.Mappings;
using Dfe.PlanTech.AzureFunctions.Models;
using Dfe.PlanTech.Domain.Questionnaire.Models;
using Dfe.PlanTech.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace Dfe.PlanTech.AzureFunctions.UnitTests.Mappers;

public class RecommendationSectionUpdaterTests
{
    private readonly DbSet<RecommendationSectionAnswerDbEntity> _sectionAnswerDbSet = Substitute.For<DbSet<RecommendationSectionAnswerDbEntity>>();
    private readonly DbSet<RecommendationSectionChunkDbEntity> _sectionChunkDbSet = Substitute.For<DbSet<RecommendationSectionChunkDbEntity>>();
   
    [Fact]
    public void UpdateEntityConcrete_EntityExists_RemovesAssociatedChunksAndAnswers()
    {
        var logger = Substitute.For<ILogger<RecommendationSectionUpdater>>();
        var db = Substitute.For<CmsDbContext>();
        db.RecommendationSectionChunks = _sectionChunkDbSet;
        db.RecommendationSectionAnswers = _sectionAnswerDbSet;
        var updater = new RecommendationSectionUpdater(logger, db);
        
        var existingSection = new RecommendationSectionDbEntity { Id = "1" };
        var newSection = new RecommendationSectionDbEntity { Id = "1" };
        
        var entity = new MappedEntity
        {
            ExistingEntity = existingSection,
            IncomingEntity = newSection
        };
        
    }
}