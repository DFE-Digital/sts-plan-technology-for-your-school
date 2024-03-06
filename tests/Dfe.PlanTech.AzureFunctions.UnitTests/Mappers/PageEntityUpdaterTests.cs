using System.Linq.Expressions;
using Dfe.PlanTech.AzureFunctions.Mappings;
using Dfe.PlanTech.AzureFunctions.Models;
using Dfe.PlanTech.Domain.Content.Models;
using Dfe.PlanTech.Domain.Content.Models.Buttons;
using Dfe.PlanTech.Domain.Questionnaire.Models;
using Dfe.PlanTech.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace Dfe.PlanTech.AzureFunctions.UnitTests.Mappers;

public class PageEntityUpdaterTests
{
  private const string ExistingPageId = "existing-page";
  private readonly CmsDbContext _db = Substitute.For<CmsDbContext>();
  private readonly ILogger<PageEntityUpdater> _logger = Substitute.For<ILogger<PageEntityUpdater>>();
  private readonly PageEntityUpdater _updater;

  private readonly List<PageDbEntity> _pages = [];
  private readonly PageDbEntity _existingPage = new() { Id = ExistingPageId, };

  public PageEntityUpdaterTests()
  {
    _updater = new PageEntityUpdater(_logger, _db);
  }

  [Fact]
  public void Should_Error_If_Incorrect_ContentComponent_Types()
  {
    MappedEntity[] shouldErrorEntities = [new MappedEntity()
    {
      IncomingEntity = new ButtonDbEntity(),
      ExistingEntity = new PageDbEntity()
    },
      new MappedEntity()  {
      IncomingEntity = new PageDbEntity(),
      ExistingEntity = new QuestionDbEntity()
    },
      new MappedEntity()  {
      IncomingEntity = new SectionDbEntity(),
      ExistingEntity = new AnswerDbEntity()
    }];

    foreach (var errorableEntity in shouldErrorEntities)
    {
      Assert.ThrowsAny<InvalidCastException>(() => _updater.UpdateEntityConcrete(errorableEntity));
    }
  }

  public void Should_AddOrUpdate_NewAndExistingPageComponents()
  {

  }

  public void Should_Delete_Removed_Entities()
  {

  }
}