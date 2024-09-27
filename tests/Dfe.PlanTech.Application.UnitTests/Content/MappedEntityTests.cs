using Dfe.PlanTech.Application.Content;
using Dfe.PlanTech.Application.Persistence.Interfaces;
using Dfe.PlanTech.Domain.Caching.Enums;
using Dfe.PlanTech.Domain.Content.Models;
using Dfe.PlanTech.Domain.Questionnaire.Models;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace Dfe.PlanTech.Application.UnitTests.Content;

public class MockDbEntity : ContentComponentDbEntity
{
    public string String { get; init; } = null!;

    public int Int { get; init; }

    public bool Bool { get; init; }
}

/// <summary>
/// Content references are nullable or have DontCopyValue so this scenario *should* never happen.
/// But in the event an entity has a null property that cannot be defaulted, it should be deemed invalid
/// </summary>
public class InvalidMockDbEntity : ContentComponentDbEntity
{
    public Answer Answer { get; init; } = null!;
}

public class MappedEntityTests
{
    private readonly ICmsDbContext _cmsDbContextMock = Substitute.For<ICmsDbContext>();
    private readonly IDatabaseHelper<ICmsDbContext> _databaseHelper = Substitute.For<IDatabaseHelper<ICmsDbContext>>();
    private readonly ILogger<MappedEntity> _logger;

    public MappedEntityTests()
    {
        _databaseHelper.Database.Returns(_cmsDbContextMock);
        _logger = Substitute.For<ILogger<MappedEntity>>();

        MockGetRequiredProperties(["String", "Int", "Bool"], typeof(MockDbEntity));
        MockGetRequiredProperties(["Answer"], typeof(InvalidMockDbEntity));
    }

    private void MockGetRequiredProperties(HashSet<string> requiredPropertyNames, Type entityType)
    {
        var requiredProperties = entityType.GetProperties().Where(property => requiredPropertyNames.Contains(property.Name));
        _databaseHelper.GetRequiredPropertiesForType(entityType).Returns(requiredProperties);
    }

    [Fact]
    public void Should_Be_Invalid_If_Cannot_Apply_Default_To_Missing_Required_Field()
    {
        var mappedEntity = new MappedEntity()
        {
            IncomingEntity = new InvalidMockDbEntity(),
            CmsEvent = CmsEvent.CREATE,
        };

        mappedEntity.UpdateEntity(_databaseHelper);
        Assert.False(mappedEntity.IsValidComponent(_logger));
    }

    [Fact]
    public void Should_Set_Correct_Defaults_On_Missing_Required_Field()
    {
        var incoming = new MockDbEntity();
        var mappedEntity = new MappedEntity()
        {
            IncomingEntity = incoming,
            CmsEvent = CmsEvent.CREATE,
        };
        mappedEntity.UpdateEntity(_databaseHelper);
        Assert.True(mappedEntity.IsValidComponent(_logger));
        Assert.Equal("", incoming.String);
        Assert.Equal(0, incoming.Int);
        Assert.False(incoming.Bool);
    }

    [Fact]
    public void GetTypedEntities_Should_Error_If_IncomingEntity_Type_DoesntMatch()
    {
        var mappedEntity = new MappedEntity()
        {
            IncomingEntity = new MockDbEntity(),
            CmsEvent = CmsEvent.CREATE,
        };

        Assert.ThrowsAny<InvalidCastException>(() => mappedEntity.GetTypedEntities<QuestionDbEntity>());
    }

    [Fact]
    public void GetTypedEntities_Should_Error_If_ExistingEntity_Type_DoesntMatch()
    {
        var mappedEntity = new MappedEntity()
        {
            IncomingEntity = new QuestionDbEntity(),
            ExistingEntity = new MockDbEntity(),
            CmsEvent = CmsEvent.CREATE,
        };

        Assert.Throws<InvalidCastException>(() => mappedEntity.GetTypedEntities<QuestionDbEntity>());
    }

}
