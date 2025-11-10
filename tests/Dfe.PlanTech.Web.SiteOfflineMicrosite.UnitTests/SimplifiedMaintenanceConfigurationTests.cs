using System.Net;
using Dfe.PlanTech.Web.SiteOfflineMicrosite.Configuration;
using Microsoft.AspNetCore.Mvc.Testing;

namespace Dfe.PlanTech.Web.SiteOfflineMicrosite.UnitTests;

public class SimplifiedMaintenanceConfigurationTests
{
    [Fact]
    public void MaintenanceConfiguration_WhenDefaultConstructed_HasEmptyMessageParagraphs()
    {
        var config = new MaintenanceConfiguration();

        Assert.NotNull(config.MessageParagraphs);
        Assert.Empty(config.MessageParagraphs);
    }

    [Fact]
    public void ConfigurationBinding_WhenSparseArrayProvided_OnlyIncludesProvidedValues()
    {
        var configData = new Dictionary<string, string?>
        {
            ["Maintenance:MessageParagraphs:2"] = "At index 2",
            ["Maintenance:MessageParagraphs:5"] = "At index 5"
        };

        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(configData)
            .Build();

        var config = new MaintenanceConfiguration();
        configuration.GetSection("Maintenance").Bind(config);

        // .NET configuration binding ignores sparse indices - only creates entries for provided values
        Assert.Equal(2, config.MessageParagraphs.Count);
        Assert.Equal("At index 2", config.MessageParagraphs[0]);
        Assert.Equal("At index 5", config.MessageParagraphs[1]);
    }

    [Fact]
    public void ConfigurationBinding_WhenExplicitEmptyStringProvided_IncludesEmptyString()
    {
        var configData = new Dictionary<string, string?>
        {
            ["Maintenance:MessageParagraphs:0"] = "First",
            ["Maintenance:MessageParagraphs:1"] = "",  // Explicit empty
            ["Maintenance:MessageParagraphs:2"] = "Third"
        };

        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(configData)
            .Build();

        var config = new MaintenanceConfiguration();
        configuration.GetSection("Maintenance").Bind(config);

        Assert.Equal(3, config.MessageParagraphs.Count);
        Assert.Equal("First", config.MessageParagraphs[0]);
        Assert.Equal("", config.MessageParagraphs[1]);  // Empty string preserved
        Assert.Equal("Third", config.MessageParagraphs[2]);
    }

    [Fact]
    public async Task MaintenancePage_WhenMessageParagraphsEmpty_DisplaysDefaultMessage()
    {
        var factory = new SimplifiedConfigWebApplicationFactory(new Dictionary<string, string?>());
        var client = factory.CreateClient();

        var response = await client.GetAsync("/");
        var content = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.ServiceUnavailable, response.StatusCode);
        Assert.Contains("You'll be able to use the service later", content);
        Assert.Contains("Your recommendations have been saved", content);
    }

    [Fact]
    public async Task MaintenancePage_WhenSingleParagraphProvided_DisplaysIt()
    {
        var factory = new SimplifiedConfigWebApplicationFactory(new Dictionary<string, string?>
        {
            ["Maintenance:MessageParagraphs:0"] = "We are performing essential maintenance."
        });
        var client = factory.CreateClient();

        var response = await client.GetAsync("/");
        var content = await response.Content.ReadAsStringAsync();

        Assert.Contains("We are performing essential maintenance.", content);
        Assert.DoesNotContain("Your recommendations have been saved", content);
    }

    [Fact]
    public async Task MaintenancePage_WhenMultipleParagraphsProvided_DisplaysAll()
    {
        var factory = new SimplifiedConfigWebApplicationFactory(new Dictionary<string, string?>
        {
            ["Maintenance:MessageParagraphs:0"] = "The service will be unavailable from 5pm on Monday 4th November.",
            ["Maintenance:MessageParagraphs:1"] = "You will be able to use the service from 9am on Tuesday 5th November.",
            ["Maintenance:MessageParagraphs:2"] = "We saved your recommendations."
        });
        var client = factory.CreateClient();

        var response = await client.GetAsync("/");
        var content = await response.Content.ReadAsStringAsync();

        Assert.Contains("The service will be unavailable from 5pm on Monday 4th November.", content);
        Assert.Contains("You will be able to use the service from 9am on Tuesday 5th November.", content);
        Assert.Contains("We saved your recommendations.", content);
    }

    [Fact]
    public async Task MaintenancePage_WhenCustomParagraphs_DoesNotDisplayDefaultMessage()
    {
        var factory = new SimplifiedConfigWebApplicationFactory(new Dictionary<string, string?>
        {
            ["Maintenance:MessageParagraphs:0"] = "Custom message here."
        });
        var client = factory.CreateClient();

        var response = await client.GetAsync("/");
        var content = await response.Content.ReadAsStringAsync();

        Assert.DoesNotContain("You'll be able to use the service later", content);
        Assert.DoesNotContain("Your recommendations have been saved", content);
    }

    [Fact]
    public async Task MaintenancePage_AlwaysDisplaysContactLink()
    {
        var factory = new SimplifiedConfigWebApplicationFactory(new Dictionary<string, string?>
        {
            ["Maintenance:MessageParagraphs:0"] = "Custom message."
        });
        var client = factory.CreateClient();

        var response = await client.GetAsync("/");
        var content = await response.Content.ReadAsStringAsync();

        Assert.Contains("Contact the Plan technology for your school team", content);
        Assert.Contains("if you have a question, feedback, or need help", content);
    }

    [Fact]
    public async Task MaintenancePage_WhenSparseArrayConfigured_DisplaysAllProvidedMessages()
    {
        var factory = new SimplifiedConfigWebApplicationFactory(new Dictionary<string, string?>
        {
            ["Maintenance:MessageParagraphs:2"] = "This is at index 2.",
            ["Maintenance:MessageParagraphs:5"] = "This is at index 5."
        });
        var client = factory.CreateClient();

        var response = await client.GetAsync("/");
        var content = await response.Content.ReadAsStringAsync();

        // .NET config binding compacts sparse arrays, so both messages display normally
        Assert.Contains("This is at index 2.", content);
        Assert.Contains("This is at index 5.", content);

        // Should not display the default message since we have custom paragraphs
        Assert.DoesNotContain("Your recommendations have been saved", content);
    }

    [Fact]
    public async Task MaintenancePage_WhenOnlyMiddleIndexConfigured_DisplaysOnlyThatMessage()
    {
        var factory = new SimplifiedConfigWebApplicationFactory(new Dictionary<string, string?>
        {
            ["Maintenance:MessageParagraphs:3"] = "Only this message."
        });
        var client = factory.CreateClient();

        var response = await client.GetAsync("/");
        var content = await response.Content.ReadAsStringAsync();

        // Should display the message (config binding ignores sparse indices)
        Assert.Contains("Only this message.", content);
        Assert.DoesNotContain("Your recommendations have been saved", content);
    }

    [Fact]
    public async Task MaintenancePage_WhenExplicitlySetToEmptyString_RendersEmptyParagraph()
    {
        var factory = new SimplifiedConfigWebApplicationFactory(new Dictionary<string, string?>
        {
            ["Maintenance:MessageParagraphs:0"] = "First message.",
            ["Maintenance:MessageParagraphs:1"] = "",  // Explicitly empty - user wants spacing
            ["Maintenance:MessageParagraphs:2"] = "Third message."
        });
        var client = factory.CreateClient();

        var response = await client.GetAsync("/");
        var content = await response.Content.ReadAsStringAsync();

        // Should have all three paragraphs including the empty one
        Assert.Contains("First message.", content);
        Assert.Contains("Third message.", content);

        // Verify empty paragraph exists between the two messages
        Assert.Contains("<p class=\"govuk-body\"></p>", content);
    }

    [Fact]
    public async Task MaintenancePage_WhenWhitespaceOnlyString_RendersWhitespaceParagraph()
    {
        var factory = new SimplifiedConfigWebApplicationFactory(new Dictionary<string, string?>
        {
            ["Maintenance:MessageParagraphs:0"] = "Message",
            ["Maintenance:MessageParagraphs:1"] = "   ",  // Whitespace - user's explicit choice
        });
        var client = factory.CreateClient();

        var response = await client.GetAsync("/");
        var content = await response.Content.ReadAsStringAsync();

        // Should render both including the whitespace-only paragraph
        Assert.Contains("Message", content);
        Assert.Contains("<p class=\"govuk-body\">   </p>", content);
    }
}

/// <summary>
/// Simplified custom factory for testing configuration
/// </summary>
public class SimplifiedConfigWebApplicationFactory : WebApplicationFactory<Program>
{
    private readonly Dictionary<string, string?> _configOverrides;

    public SimplifiedConfigWebApplicationFactory(Dictionary<string, string?> configOverrides)
    {
        _configOverrides = configOverrides;
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Test");

        builder.ConfigureAppConfiguration((context, config) =>
        {
            config.AddInMemoryCollection(_configOverrides);
        });
    }
}
