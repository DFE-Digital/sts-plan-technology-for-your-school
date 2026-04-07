using Dfe.PlanTech.Core.Contentful.Models;
using Dfe.PlanTech.Core.Helpers;
using static Dfe.PlanTech.Core.Constants.ContentfulMicrocopyConstants;

namespace Dfe.PlanTech.Core.UnitTests.Helpers;

public class ContentfulMicrocopyHelperTests
{
    [Fact]
    public void GetMicrocopyTextByKey_Returns_Correct_Text_When_VariablesExist()
    {
        var microcopyEntries = new List<MicrocopyEntry>
        {
            new MicrocopyEntry
            {
                InternalName = "Test",
                Key = "test",
                Value = "Microcopy text containing a {{variable}}."
            }
        };
        var microcopyRecord = new MicrocopyRecord("test", "", "variable");
        var dynamicValues = new Dictionary<string, string> { ["variable"] = "test value" };

        var result = ContentfulMicrocopyHelper.GetMicrocopyTextByKey(microcopyRecord, microcopyEntries, dynamicValues);

        Assert.IsType<string>(result);
        Assert.Equal("Microcopy text containing a test value.", result);
    }

    [Fact]
    public void GetMicrocopyTextByKey_Returns_Correct_Text_When_No_Variables()
    {
        var microcopyEntries = new List<MicrocopyEntry>
        {
            new MicrocopyEntry
            {
                InternalName = "Test",
                Key = "test",
                Value = "Microcopy text containing no variables."
            }
        };
        var microcopyRecord = new MicrocopyRecord("test", "");
        var dynamicValues = new Dictionary<string, string> { ["variable"] = "test value" };

        var result = ContentfulMicrocopyHelper.GetMicrocopyTextByKey(microcopyRecord, microcopyEntries, dynamicValues);

        Assert.IsType<string>(result);
        Assert.Equal("Microcopy text containing no variables.", result);
    }

    [Fact]
    public void GetMicrocopyTextByKey_Returns_FallbackText_When_NoMatchingEntries()
    {
        var microcopyEntries = new List<MicrocopyEntry>
        {
            new MicrocopyEntry
            {
                InternalName = "Test",
                Key = "test",
                Value = "Microcopy text."
            }
        };
        var microcopyRecord = new MicrocopyRecord("not-test", "Fallback text");

        var result = ContentfulMicrocopyHelper.GetMicrocopyTextByKey(microcopyRecord, microcopyEntries);

        Assert.IsType<string>(result);
        Assert.Equal("Fallback text", result);
    }

    [Fact]
    public void GetMicrocopyTextByKey_Returns_FallbackText_When_MicrocopyEntriesNull()
    {
        var microcopyRecord = new MicrocopyRecord("test", "Fallback text");

        var result = ContentfulMicrocopyHelper.GetMicrocopyTextByKey(microcopyRecord, null!);

        Assert.IsType<string>(result);
        Assert.Equal("Fallback text", result);
    }

    [Fact]
    public void GetMicrocopyTextByKey_Returns_FallbackText_When_VariablesMatchMissing()
    {
        var microcopyEntries = new List<MicrocopyEntry>
        {
            new MicrocopyEntry
            {
                InternalName = "Test",
                Key = "test",
                Value = "Microcopy text containing a {{variable}}."
            }
        };
        var microcopyRecord = new MicrocopyRecord("test", "Fallback text", "variable");
        var dynamicValues = new Dictionary<string, string> { ["incorrect-variable"] = "test value" };

        var result = ContentfulMicrocopyHelper.GetMicrocopyTextByKey(microcopyRecord, microcopyEntries, dynamicValues);

        Assert.IsType<string>(result);
        Assert.Equal("Fallback text", result);
    }

    [Fact]
    public void GetMicrocopyTextByKey_Returns_FallbackText_When_VariablesPresentAndDynamicValuesNull()
    {
        var microcopyEntries = new List<MicrocopyEntry>
        {
            new MicrocopyEntry
            {
                InternalName = "Test",
                Key = "test",
                Value = "Microcopy text containing a {{variable}}."
            }
        };
        var microcopyRecord = new MicrocopyRecord("test", "Fallback text", "variable");

        var result = ContentfulMicrocopyHelper.GetMicrocopyTextByKey(microcopyRecord, microcopyEntries, null);

        Assert.IsType<string>(result);
        Assert.Equal("Fallback text", result);
    }
};
