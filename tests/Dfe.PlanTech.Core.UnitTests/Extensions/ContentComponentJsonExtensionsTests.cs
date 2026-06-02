using Dfe.PlanTech.Core.Extensions;

namespace Dfe.PlanTech.Core.UnitTests.Extensions;

public class ContentfulTypeMappingValidatorTests
{
    [Fact]
    public void ValidateTypeMapping_WhenSetsMatch_DoesNotThrow()
    {
        var reflected = new HashSet<Type> { typeof(string), typeof(int) };
        var mapped = new HashSet<Type> { typeof(string), typeof(int) };

        var ex = Record.Exception(() =>
            ContentComponentJsonExtensions.ValidateTypeMapping(reflected, mapped)
        );

        Assert.Null(ex);
    }

    [Fact]
    public void ValidateTypeMapping_WhenBothEmpty_DoesNotThrow()
    {
        var ex = Record.Exception(() =>
            ContentComponentJsonExtensions.ValidateTypeMapping(
                new HashSet<Type>(),
                new HashSet<Type>()
            )
        );

        Assert.Null(ex);
    }

    [Fact]
    public void ValidateTypeMapping_WhenTypeMissingFromMap_ThrowsWithTypeName()
    {
        var reflected = new HashSet<Type> { typeof(string), typeof(int) };
        var mapped = new HashSet<Type> { typeof(string) };

        var ex = Assert.Throws<InvalidOperationException>(() =>
            ContentComponentJsonExtensions.ValidateTypeMapping(reflected, mapped)
        );

        Assert.Contains("corresponding entry", ex.Message);
        Assert.Contains(nameof(Int32), ex.Message);
    }

    [Fact]
    public void ValidateTypeMapping_WhenTypeMissingFromReflection_ThrowsWithTypeName()
    {
        var reflected = new HashSet<Type> { typeof(string) };
        var mapped = new HashSet<Type> { typeof(string), typeof(int) };

        var ex = Assert.Throws<InvalidOperationException>(() =>
            ContentComponentJsonExtensions.ValidateTypeMapping(reflected, mapped)
        );

        Assert.Contains("corresponding class", ex.Message);
        Assert.Contains(nameof(Int32), ex.Message);
    }

    [Fact]
    public void ValidateTypeMapping_WhenBothSidesHaveMismatches_ThrowsWithBothMessages()
    {
        var reflected = new HashSet<Type> { typeof(string), typeof(int) };
        var mapped = new HashSet<Type> { typeof(string), typeof(DateTime) };

        var ex = Assert.Throws<InvalidOperationException>(() =>
            ContentComponentJsonExtensions.ValidateTypeMapping(reflected, mapped)
        );

        Assert.Contains("corresponding entry", ex.Message);
        Assert.Contains("corresponding class", ex.Message);
        Assert.Contains(nameof(Int32), ex.Message);
        Assert.Contains(nameof(DateTime), ex.Message);
    }

    [Fact]
    public void ValidateContentfulTypeMapping_AgainstRealAssembly_DoesNotThrow()
    {
        var ex = Record.Exception(ContentComponentJsonExtensions.ValidateContentfulTypeMapping);
        Assert.Null(ex);
    }
}
