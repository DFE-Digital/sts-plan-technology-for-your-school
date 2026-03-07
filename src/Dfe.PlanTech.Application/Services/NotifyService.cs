using System.Text.Json;
using System.Text.RegularExpressions;
using Dfe.PlanTech.Application.Rendering.Markdown;
using Dfe.PlanTech.Application.Services.Interfaces;
using Dfe.PlanTech.Application.Workflows.Interfaces;
using Dfe.PlanTech.Core.Constants;
using Dfe.PlanTech.Core.Contentful.Models;
using Dfe.PlanTech.Core.Enums;
using Dfe.PlanTech.Core.Extensions;
using Dfe.PlanTech.Core.Models;
using Notify.Exceptions;
using Notify.Interfaces;

namespace Dfe.PlanTech.Application.Services;

public class NotifyService(IContentfulWorkflow contentfulWorkflow, INotificationClient notifyClient)
    : INotifyService
{
    private readonly IContentfulWorkflow _contentfulWorkflow =
        contentfulWorkflow ?? throw new ArgumentNullException(nameof(contentfulWorkflow));

    private readonly INotificationClient _notifyClient =
        notifyClient ?? throw new ArgumentNullException(nameof(notifyClient));

    private const string CouldNotSendEmail = "Could not send email";

    public List<NotifySendResult> SendSingleRecommendationEmail(
        ShareByEmailModel model,
        ComponentTextBodyEntry textBody,
        string establishmentName,
        string recommendationHeader,
        string sectionName,
        RecommendationStatus recommendationStatus
    )
    {
        var userMessage = model.UserMessage ?? "";
        if (!string.IsNullOrWhiteSpace(userMessage))
        {
            var messagePrefix =
                $"{model.NameOfUser} added a message:" + Environment.NewLine + Environment.NewLine;
            var lines = Regex
                .Split(
                    userMessage,
                    @"\r\n|\n",
                    RegexOptions.Compiled,
                    TimeSpan.FromMilliseconds(200)
                )
                .Select(line => $"^ {line}");
            userMessage = messagePrefix + string.Join(Environment.NewLine, lines);
        }

        var recommendationMarkdown = MarkdownRenderer.Render(textBody.RichText);

        var personalisation = new Dictionary<string, dynamic>
        {
            { "name of user", model.NameOfUser },
            { "school", establishmentName },
            { "standard", sectionName.ToLower() },
            { "user message", userMessage },
            { "recommendation name", recommendationHeader },
            { "status", recommendationStatus.GetDisplayName() },
            { "recommendation", recommendationMarkdown },
        };

        // TODO: Use active correlation ID once implemented
        var correlationId = Guid.NewGuid().ToString();

        var results = new List<NotifySendResult>();

        foreach (var recipient in model.EmailAddresses)
        {
            var errorPrefix = $"{CouldNotSendEmail} to {recipient}";

            try
            {
                var response = _notifyClient.SendEmail(
                    emailAddress: recipient,
                    templateId: NotifyConstants.ShareSingleRecommendationTemplateId,
                    personalisation: personalisation,
                    clientReference: correlationId
                );

                var result = new NotifySendResult { Recipient = recipient, Response = response };

                results.Add(result);
            }
            catch (NotifyClientException clientException)
            {
                var errors = ParseNotifyClientException(clientException.Message);
                var result = new NotifySendResult { Recipient = recipient, Errors = errors };

                results.Add(result);
            }
            catch (Exception)
            {
                var result = new NotifySendResult
                {
                    Recipient = recipient,
                    Errors = ["GOK.UK Notify failed"],
                };

                results.Add(result);
            }
        }

        return results;
    }

    private List<string> ParseNotifyClientException(string message)
    {
        // GOV.UK Notify's provided exception class is weird.
        // It returns a string that combines regular text with JSON, and
        // puts a very similar message twice. This attempts to untangle it.

        var firstPart = message.Split("\n").Select(x => x.Trim()).FirstOrDefault();
        if (firstPart is null)
        {
            return [];
        }

        var matches = Regex.Match(
            firstPart,
            "Status code \\d+. Error: ({.*}).*",
            RegexOptions.Compiled,
            TimeSpan.FromMilliseconds(200)
        );
        var messageJson = matches.Groups.Values.LastOrDefault()?.ToString();
        if (string.IsNullOrWhiteSpace(messageJson))
        {
            return [];
        }

        var result = JsonSerializer.Deserialize<NotifyClientExceptionMessage>(messageJson);
        return result?.Errors.Select(e => e.Message).ToList() ?? [];
    }
}
