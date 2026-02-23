using System.Text.RegularExpressions;
using Dfe.PlanTech.Application.Rendering.Markdown;
using Dfe.PlanTech.Application.Services.Interfaces;
using Dfe.PlanTech.Application.Workflows.Interfaces;
using Dfe.PlanTech.Core.Constants;
using Dfe.PlanTech.Core.Contentful.Models;
using Notify.Interfaces;

namespace Dfe.PlanTech.Application.Services;

public class NotifyService(IContentfulWorkflow contentfulWorkflow, INotificationClient notifyClient)
    : INotifyService
{
    private readonly IContentfulWorkflow _contentfulWorkflow =
        contentfulWorkflow ?? throw new ArgumentNullException(nameof(contentfulWorkflow));

    private readonly INotificationClient _notifyClient =
        notifyClient ?? throw new ArgumentNullException(nameof(notifyClient));

    public async Task SendEmailAsync(
        string recommendationRef,
        ICollection<string> recipientEmailAddresses,
        string userFullName,
        string activeSchoolName,
        string sectionName,
        string userMessage,
        string recommendationStatus
    )
    {
        var recommendationChunk = await _contentfulWorkflow.GetEntryById<RecommendationChunkEntry>(
            recommendationRef
        );
        var textBody = await _contentfulWorkflow.GetEntryById<ComponentTextBodyEntry>(
            recommendationChunk.Content[0].Id
        );

        if (!string.IsNullOrWhiteSpace(userMessage))
        {
            var messagePrefix =
                $"{userFullName} added a message:" + Environment.NewLine + Environment.NewLine;
            var lines = Regex.Split(userMessage, @"\r\n|\n").Select(line => $"^ {line}");
            userMessage = messagePrefix + string.Join(Environment.NewLine, lines);
        }

        var recommendationMarkdown = MarkdownRenderer.Render(textBody.RichText);

        var personalisation = new Dictionary<string, dynamic>
        {
            { "name of user", userFullName },
            { "school", activeSchoolName },
            { "standard", sectionName.ToLower() },
            { "user message", userMessage },
            { "recommendation name", recommendationChunk.HeaderText },
            { "status", recommendationStatus },
            { "recommendation", recommendationMarkdown },
        };

        // TODO: User active correlation ID once implemented
        var correlationId = Guid.NewGuid().ToString();

        foreach (var recipient in recipientEmailAddresses)
        {
            _notifyClient.SendEmail(
                emailAddress: recipient,
                templateId: NotifyConstants.ShareSingleRecommendationTemplateId,
                personalisation: personalisation,
                clientReference: correlationId
            );
        }
    }
}
