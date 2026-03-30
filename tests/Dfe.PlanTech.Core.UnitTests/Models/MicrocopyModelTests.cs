using Dfe.PlanTech.Core.Models;

namespace Dfe.PlanTech.Core.UnitTests.Models;

public class MicrocopyModelTests
{
    [Fact]
    public void GetText_NoVariables_Returns_Value_When_Value_Is_Not_Null()
    {
        // Arrange
        var model = new MicrocopyModel("Key 1", "Value 1", "Fallback 1");

        // Act
        var result = model.GetText();

        // Assert
        Assert.Equal("Value 1", result);
    }

    [Fact]
    public void GetText_NoVariables_Returns_Fallback_When_Value_Is_Null()
    {
        // Arrange
        var model = new MicrocopyModel("Key 2", null!, "Fallback 2");

        // Act
        var result = model.GetText();

        // Assert
        Assert.Equal("Fallback 2", result);
    }

    [Fact]
    public void GetText_WithVariables_And_DynamicValues_Null_Returns_Fallback()
    {
        // Arrange
        var model = new MicrocopyModel("Key 3", "Value with {{variable 3}}", "Fallback 3", [ "variable 3" ]);

        // Act
        var result = model.GetText(null);

        // Assert
        Assert.Equal("Fallback 3", result);
    }

    [Fact]
    public void GetText_Replaces_All_Placeholders_And_Returns_Value()
    {
        // Arrange
        string[] VarsFirstSecond = { "first", "second" };
        var model = new MicrocopyModel("Key 4", "Value with {{first}} and {{second}}", "Fallback 4", VarsFirstSecond);
        var dynamicValues = new Dictionary<string, string>
        {
            ["first"] = "value 4a",
            ["second"] = "value 4b"
        };

        // Act
        var result = model.GetText(dynamicValues);

        // Assert
        Assert.Equal("Value with value 4a and value 4b", result);
    }

    [Fact]
    public void GetText_WithVariables_Any_Missing_DynamicValues_Returns_Fallback()
    {
        // Arrange
        var model = new MicrocopyModel("Key 5", "Value with missing {{variable 5}}", "Fallback 5", [ "variable 5" ]);
        var dynamicValues = new Dictionary<string, string>();

        // Act
        var result = model.GetText(dynamicValues);

        // Assert
        Assert.Equal("Fallback 5", result);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void GetText_WithDynamicValues_Empty_Or_Whitespace_Value_Returns_Fallback(string? badValue)
    {
        // Arrange
        var model = new MicrocopyModel("Key 6", "Replace this: {{variable 6}}", "Fallback 6", ["variable 6"]);
        var dynamicValues = new Dictionary<string, string>
        {
            ["variable 6"] = badValue!
        };

        // Act
        var result = model.GetText(dynamicValues);

        // Assert
        Assert.Equal("Fallback 6", result);
    }

    [Fact]
    public void GetText_Extra_DynamicValues_Are_Ignored()
    {
        // Arrange
        var model = new MicrocopyModel("Key 7", "Value with {{variable 7}}", "Fallback 7", ["variable 7"]);
        var dynamicValues = new Dictionary<string, string>
        {
            ["variable 7"] = "some value",
            ["unused"] = "ignored"
        };

        // Act
        var result = model.GetText(dynamicValues);

        // Assert
        Assert.Equal("Value with some value", result);
    }

    [Fact]
    public void GetText_Value_Null_And_No_Variables_Uses_Fallback()
    {
        // Arrange
        var model = new MicrocopyModel("Key 8", null!, "Fallback 8");

        // Act
        var result = model.GetText();

        // Assert
        Assert.Equal("Fallback 8", result);
    }

    [Fact]
    public void GetText_Duplicate_Variables_All_Must_Be_Present_Or_Fallback_Returned()
    {
        // Variables contains duplicates: the method will attempt to validate twice
        // Arrange
        var model = new MicrocopyModel("Key 9", "Duplicate variables {{variable 9}} and {{variable 9}}", "Fallback 9", ["variable 9", "variable 9"]);
        var dynamicValues = new Dictionary<string, string> { ["variable 9"] = "value 9" };

        // Act
        var ok = model.GetText(dynamicValues);
        var missing = model.GetText(new Dictionary<string, string>());

        // Assert
        Assert.Equal("Duplicate variables value 9 and value 9", ok);
        Assert.Equal("Fallback 9", missing);
    }

    [Fact]
    public void GetText_Variable_Listed_But_Not_In_Text_Still_Requires_Value_Or_Fallback_Is_Returned()
    {
        // Arrange
        var model = new MicrocopyModel("Key 10", "Value with no placeholders", "Fallback 10", ["unused"]);
        var dynamicValues = new Dictionary<string, string> { ["unused"] = "not in use" };

        // Act
        var ok = model.GetText(dynamicValues);
        var missing = model.GetText(new Dictionary<string, string>());

        // Assert
        Assert.Equal("Value with no placeholders", ok);
        Assert.Equal("Fallback 10", missing);
    }
}
