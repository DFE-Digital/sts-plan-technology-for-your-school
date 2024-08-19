using Dfe.PlanTech.AzureFunctions.Models;
using Dfe.PlanTech.Domain.Caching.Enums;
using Dfe.PlanTech.Domain.Content.Models;
using Dfe.PlanTech.Domain.Questionnaire.Models;
using Dfe.PlanTech.Infrastructure.Data;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace Dfe.PlanTech.AzureFunctions.UnitTests.Models;

public class MockDbEntity: ContentComponentDbEntity
{
    public string String { get; init; } = null!;

    public int Int { get; init; }

    public bool Bool { get; init; }
}

/// <summary>
/// Content references are nullable or have DontCopyValue so this scenario *should* never happen.
/// But in the event an entity has a null property that cannot be defaulted, it should be deemed invalid
/// </summary>
public class InvalidMockDbEntity: ContentComponentDbEntity
{
    public Answer Answer { get; init; } = null!;
}

public class MappedEntityTests
{
    private readonly CmsDbContext _cmsDbContextMock;
    private readonly ILogger _logger;

    public MappedEntityTests()
    {
        _cmsDbContextMock = Substitute.For<CmsDbContext>();
        _logger = Substitute.For<ILogger>();

        var mockType = Substitute.For<IEntityType>();
        var invalidMockType = Substitute.For<IEntityType>();
        mockType.ClrType.Returns(typeof(MockDbEntity));
        invalidMockType.ClrType.Returns(typeof(InvalidMockDbEntity));

        _cmsDbContextMock.Model.FindEntityType(typeof(MockDbEntity)).Returns(mockType);
        _cmsDbContextMock.Model.FindEntityType(typeof(InvalidMockDbEntity)).Returns(invalidMockType);

        HashSet<string> nonNullablePropertyNames = ["String", "Int", "Bool", "Answer"];

        mockType.GetProperties().Returns(MockProperties(nonNullablePropertyNames, typeof(MockDbEntity)));
        invalidMockType.GetProperties().Returns(MockProperties(nonNullablePropertyNames, typeof(InvalidMockDbEntity)));
    }

    private static IEnumerable<IProperty> MockProperties(HashSet<string> nonNullableProperties, Type entityType)
    {
        return entityType.GetProperties().Select(property =>
        {
            var propertySub = Substitute.For<IProperty>();
            propertySub.PropertyInfo.Returns(property);
            propertySub.IsNullable.Returns(!nonNullableProperties.Contains(property.Name));

            return propertySub;
        });
    }

    [Fact]
    public void Should_Be_Invalid_If_Cannot_Apply_Default_To_Missing_Required_Field()
    {
        var mappedEntity = new MappedEntity()
        {
            IncomingEntity = new InvalidMockDbEntity(),
            CmsEvent = CmsEvent.CREATE,
        };
        Assert.False(mappedEntity.IsValidComponent(_cmsDbContextMock, _logger));
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
        Assert.True(mappedEntity.IsValidComponent(_cmsDbContextMock, _logger));
        Assert.Equal("", incoming.String);
        Assert.Equal(0, incoming.Int);
        Assert.False(incoming.Bool);
    }
}
