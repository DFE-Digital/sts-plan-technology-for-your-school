using System.ComponentModel.DataAnnotations;
using Dfe.PlanTech.Core.Constants;
using Dfe.PlanTech.Core.Enums;
using Dfe.PlanTech.Core.Extensions;

namespace Dfe.PlanTech.Core.UnitTests.Extensions;

public class EnumExtensionsTests
{
    // Local test enum so we can control Display attributes and names.
    public enum SampleEnum
    {
        [Display(Name = "First value", Description = "The first sample value")]
        First = 1,

        [Display(Name = "Second value", Description = "The second sample value")]
        Second = 2,

        // No Display attribute on this one
        Third = 3
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void GetEnumValue_ReturnsNull_ForNullOrWhitespace(string? input)
    {
        // Act
        var result = input.GetEnumValue<SampleEnum>();

        // Assert
        Assert.Null(result);
    }

    [Theory]
    [InlineData("First value", SampleEnum.First)]     // matches DisplayAttribute.Name
    [InlineData("first value", SampleEnum.First)]     // case-insensitive display name
    [InlineData("Second", SampleEnum.Second)]         // matches enum name
    [InlineData("second", SampleEnum.Second)]         // case-insensitive enum name
    public void GetEnumValue_ReturnsEnum_WhenNameOrDisplayNameMatches(string input, SampleEnum expected)
    {
        // Act
        var result = input.GetEnumValue<SampleEnum>();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(expected, result.Value);
    }

    [Fact]
    public void GetEnumValue_ReturnsNull_WhenNoMatch()
    {
        var result = "does-not-exist".GetEnumValue<SampleEnum>();
        Assert.Null(result);
    }

    [Fact]
    public void GetDisplayName_UsesDisplayName_WhenAttributePresent()
    {
        var value = SampleEnum.First;

        var displayName = value.GetDisplayName();

        Assert.Equal("First value", displayName);
    }

    [Fact]
    public void GetDisplayName_FallsBackToEnumName_WhenNoDisplayAttribute()
    {
        var value = SampleEnum.Third;

        var displayName = value.GetDisplayName();

        Assert.Equal("Third", displayName);
    }

    [Fact]
    public void GetDescription_UsesDescription_WhenAttributePresent()
    {
        var value = SampleEnum.Second;

        var description = value.GetDescription();

        Assert.Equal("The second sample value", description);
    }

    [Fact]
    public void GetDescription_FallsBackToLowercaseName_WhenNoDisplayAttribute()
    {
        var value = SampleEnum.Third;

        var description = value.GetDescription();

        Assert.Equal("third", description);
    }

    [Fact]
    public void GetCssClassOrDefault_ReturnsDefault_WhenValueIsNull()
    {
        RecommendationStatus? status = null;
        var defaultClass = "default-css-class";

        var cssClass = status.GetCssClassOrDefault(defaultClass);

        Assert.Equal(defaultClass, cssClass);
    }

    [Fact]
    public void GetCssClassOrDefault_ReturnsNonEmpty_WhenValueIsNotNull()
    {
        RecommendationStatus? status = RecommendationStatus.NotStarted;
        var defaultClass = "default-css-class";

        var cssClass = status.GetCssClassOrDefault(defaultClass);

        // We don’t assert the exact value (that depends on attributes),
        // just that we get *something* sensible back.
        Assert.False(string.IsNullOrWhiteSpace(cssClass));
    }

    [Fact]
    public void GetCssClass_DelegatesTo_GetCssClassOrDefault_With_DefaultTagClass()
    {
        var status = RecommendationStatus.InProgress;

        var viaDefault = ((RecommendationStatus?)status).GetCssClassOrDefault(RecommendationConstants.DefaultTagClass);
        var viaShortcut = status.GetCssClass();

        Assert.Equal(viaDefault, viaShortcut);
    }
}
