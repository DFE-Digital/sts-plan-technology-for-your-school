using Dfe.PlanTech.Application.Workflows;
using Dfe.PlanTech.Core.Models;
using Notify.Exceptions;
using Notify.Interfaces;
using Notify.Models.Responses;
using NSubstitute;

namespace Dfe.PlanTech.Application.UnitTests.Workflows;

public class NotifyWorkflowTests
{
    private readonly INotificationClient _notifyClient = Substitute.For<INotificationClient>();

    [Fact]
    public void Constructor_WhenNotifyClientIsNull_ThrowsArgumentNullException()
    {
        var act = () => new NotifyWorkflow(null!);

        var exception = Assert.Throws<ArgumentNullException>(act);

        Assert.Equal("notifyClient", exception.ParamName);
    }

    [Fact]
    public void SendEmails_WhenAllEmailsSendSuccessfully_ReturnsSuccessfulResults()
    {
        var sut = CreateSut();

        var model = new ShareByEmailModel { EmailAddresses = ["one@test.com", "two@test.com"] };

        var personalisation = new Dictionary<string, object> { ["name of user"] = "Drew" };

        const string correlationId = "corr-123";
        const string templateId = "template-123";

        var response1 = Substitute.For<EmailNotificationResponse>();
        var response2 = Substitute.For<EmailNotificationResponse>();

        _notifyClient
            .SendEmail("one@test.com", templateId, personalisation, correlationId)
            .Returns(response1);

        _notifyClient
            .SendEmail("two@test.com", templateId, personalisation, correlationId)
            .Returns(response2);

        var result = sut.SendEmails(model, personalisation, correlationId, templateId);

        Assert.Equal(2, result.Count);

        Assert.Equal("one@test.com", result[0].Recipient);
        Assert.Same(response1, result[0].Response);
        Assert.Empty(result[0].Errors);

        Assert.Equal("two@test.com", result[1].Recipient);
        Assert.Same(response2, result[1].Response);
        Assert.Empty(result[1].Errors);

        _notifyClient
            .Received(1)
            .SendEmail("one@test.com", templateId, personalisation, correlationId);
        _notifyClient
            .Received(1)
            .SendEmail("two@test.com", templateId, personalisation, correlationId);
    }

    [Fact]
    public void SendEmails_WhenNotifyClientThrowsNotifyClientExceptionWithValidJson_ParsesErrorsIntoResult()
    {
        var sut = CreateSut();

        var model = new ShareByEmailModel { EmailAddresses = ["broken@test.com"] };

        var personalisation = new Dictionary<string, object>();
        const string correlationId = "corr-123";
        const string templateId = "template-123";

        var message = """
            Status code 400. Error: {"errors":[{"message":"Missing personalisation: school"},{"message":"Invalid email address"}]} some extra repeated noise
            Status code 400. Error: {"errors":[{"message":"Missing personalisation: school"},{"message":"Invalid email address"}]}
            """;

        _notifyClient
            .SendEmail("broken@test.com", templateId, personalisation, correlationId)
            .Returns(_ => throw new NotifyClientException(message));

        var result = sut.SendEmails(model, personalisation, correlationId, templateId);

        var item = Assert.Single(result);
        Assert.Equal("broken@test.com", item.Recipient);
        Assert.Null(item.Response);
        Assert.Equal(2, item.Errors.Count);
        Assert.Contains("Missing personalisation: school", item.Errors);
        Assert.Contains("Invalid email address", item.Errors);
    }

    [Fact]
    public void SendEmails_WhenNotifyClientThrowsNotifyClientExceptionWithoutMatchingJson_ReturnsEmptyErrors()
    {
        var sut = CreateSut();

        var model = new ShareByEmailModel { EmailAddresses = ["broken@test.com"] };

        var personalisation = new Dictionary<string, object>();
        const string correlationId = "corr-123";
        const string templateId = "template-123";

        _notifyClient
            .SendEmail("broken@test.com", templateId, personalisation, correlationId)
            .Returns(_ => throw new NotifyClientException("Completely unhelpful message blob"));

        var result = sut.SendEmails(model, personalisation, correlationId, templateId);

        var item = Assert.Single(result);
        Assert.Equal("broken@test.com", item.Recipient);
        Assert.Null(item.Response);
        Assert.Empty(item.Errors);
    }

    [Fact]
    public void SendEmails_WhenNotifyClientThrowsNotifyClientExceptionWithBlankMessage_ReturnsEmptyErrors()
    {
        var sut = CreateSut();

        var model = new ShareByEmailModel { EmailAddresses = ["broken@test.com"] };

        var personalisation = new Dictionary<string, object>();
        const string correlationId = "corr-123";
        const string templateId = "template-123";

        _notifyClient
            .SendEmail("broken@test.com", templateId, personalisation, correlationId)
            .Returns(_ => throw new NotifyClientException("   "));

        var result = sut.SendEmails(model, personalisation, correlationId, templateId);

        var item = Assert.Single(result);
        Assert.Equal("broken@test.com", item.Recipient);
        Assert.Null(item.Response);
        Assert.Empty(item.Errors);
    }

    [Fact]
    public void SendEmails_WhenNotifyClientThrowsGeneralException_ReturnsGenericError()
    {
        var sut = CreateSut();

        var model = new ShareByEmailModel { EmailAddresses = ["broken@test.com"] };

        var personalisation = new Dictionary<string, object>();
        const string correlationId = "corr-123";
        const string templateId = "template-123";

        _notifyClient
            .SendEmail("broken@test.com", templateId, personalisation, correlationId)
            .Returns(_ => throw new InvalidOperationException("Boom"));

        var result = sut.SendEmails(model, personalisation, correlationId, templateId);

        var item = Assert.Single(result);
        Assert.Equal("broken@test.com", item.Recipient);
        Assert.Null(item.Response);
        Assert.Single(item.Errors);
        Assert.Equal("GOV.UK Notify failed", item.Errors[0]);
    }

    [Fact]
    public void SendEmails_WhenSomeRecipientsFail_ContinuesProcessingRemainingRecipients()
    {
        var sut = CreateSut();

        var model = new ShareByEmailModel
        {
            EmailAddresses = ["first@test.com", "second@test.com", "third@test.com"],
        };

        var personalisation = new Dictionary<string, object> { ["standard"] = "Digital Standards" };

        const string correlationId = "corr-123";
        const string templateId = "template-123";

        var successResponse1 = Substitute.For<EmailNotificationResponse>();
        var successResponse3 = Substitute.For<EmailNotificationResponse>();

        _notifyClient
            .SendEmail("first@test.com", templateId, personalisation, correlationId)
            .Returns(successResponse1);

        _notifyClient
            .SendEmail("second@test.com", templateId, personalisation, correlationId)
            .Returns(_ =>
                throw new NotifyClientException(
                    """Status code 400. Error: {"errors":[{"message":"Invalid email address"}]}"""
                )
            );

        _notifyClient
            .SendEmail("third@test.com", templateId, personalisation, correlationId)
            .Returns(successResponse3);

        var result = sut.SendEmails(model, personalisation, correlationId, templateId);

        Assert.Equal(3, result.Count);

        Assert.Equal("first@test.com", result[0].Recipient);
        Assert.Same(successResponse1, result[0].Response);
        Assert.Empty(result[0].Errors);

        Assert.Equal("second@test.com", result[1].Recipient);
        Assert.Null(result[1].Response);
        Assert.Single(result[1].Errors);
        Assert.Equal("Invalid email address", result[1].Errors[0]);

        Assert.Equal("third@test.com", result[2].Recipient);
        Assert.Same(successResponse3, result[2].Response);
        Assert.Empty(result[2].Errors);
    }

    [Fact]
    public void SendEmails_WhenNoRecipients_ReturnsEmptyList()
    {
        var sut = CreateSut();

        var model = new ShareByEmailModel { EmailAddresses = [] };

        var personalisation = new Dictionary<string, object>();
        const string correlationId = "corr-123";
        const string templateId = "template-123";

        var result = sut.SendEmails(model, personalisation, correlationId, templateId);

        Assert.Empty(result);
        _notifyClient.DidNotReceiveWithAnyArgs().SendEmail(default!, default!, default!, default!);
    }

    private NotifyWorkflow CreateSut() => new(_notifyClient);
}
