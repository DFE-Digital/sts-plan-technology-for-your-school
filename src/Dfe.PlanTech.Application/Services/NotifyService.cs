using System.Text;
using System.Text.RegularExpressions;
using Dfe.PlanTech.Application.Rendering.Markdown;
using Dfe.PlanTech.Application.Services.Interfaces;
using Dfe.PlanTech.Application.Workflows.Interfaces;
using Dfe.PlanTech.Core.Constants;
using Dfe.PlanTech.Core.Contentful.Models;
using Dfe.PlanTech.Core.DataTransferObjects.Sql;
using Dfe.PlanTech.Core.Enums;
using Dfe.PlanTech.Core.Extensions;
using Dfe.PlanTech.Core.Models;

namespace Dfe.PlanTech.Application.Services;

public class NotifyService(INotifyWorkflow notifyWorkflow) : INotifyService
{
    private readonly INotifyWorkflow _notifyWorkflow =
        notifyWorkflow ?? throw new ArgumentNullException(nameof(notifyWorkflow));

    public List<NotifySendResult> SendSingleRecommendationEmail(
        ShareByEmailModel model,
        ComponentTextBodyEntry textBody,
        string establishmentName,
        string recommendationHeader,
        string categoryName,
        RecommendationStatus recommendationStatus
    )
    {
        var userMessage = BuildUserMessage(model);
        var recommendationMarkdown = MarkdownRenderer.Render(textBody.RichText);
        var personalisation = new Dictionary<string, object>
        {
            { "name of user", model.NameOfUser },
            { "school", establishmentName },
            { "standard", categoryName.ToLowerInvariant() },
            { "user message", userMessage },
            { "recommendation name", recommendationHeader },
            { "status", recommendationStatus.GetDisplayName() },
            { "recommendation", recommendationMarkdown },
        };

        // TODO: Use active correlation ID once implemented
        var correlationId = Guid.NewGuid().ToString();

        return _notifyWorkflow.SendEmails(
            model,
            personalisation,
            correlationId,
            NotifyConstants.ShareSingleRecommendationTemplateId
        );
    }

    public List<NotifySendResult> SendStandardEmail(
        ShareByEmailModel model,
        List<QuestionnaireSectionEntry> sections,
        List<SqlSectionStatusDto> sectionStatuses,
        Dictionary<string, SqlEstablishmentRecommendationHistoryDto> recommendationStatuses,
        string categoryName,
        string establishmentName
    )
    {
        string userMessage = BuildUserMessage(model);
        var standardMarkdown = BuildStandardMarkdown(
            sections,
            sectionStatuses,
            recommendationStatuses
        );
        var personalisation = new Dictionary<string, object>
        {
            { "name of user", model.NameOfUser },
            { "school", establishmentName },
            { "standard lowercase", categoryName.ToLowerInvariant() },
            { "user message", userMessage },
            { "standard", categoryName },
            { "recommendations", standardMarkdown },
        };

        // TODO: Use active correlation ID once implemented
        var correlationId = Guid.NewGuid().ToString();

        return _notifyWorkflow.SendEmails(
            model,
            personalisation,
            correlationId,
            NotifyConstants.ShareStandardTemplateId
        );
    }

    public List<NotifySendResult> SendMatStandardEmail(
        ShareByEmailModel model,
        List<QuestionnaireSectionEntry> sections,
        string categoryName,
        string matName
    )
    {
        var userMessage = BuildUserMessage(model);
        var standardMarkdown = BuildMatStandardMarkdown(sections);

        var personalisation = new Dictionary<string, object>
        {
            { "name of user", model.NameOfUser },
            { "school", matName },
            { "standard lowercase", categoryName.ToLowerInvariant() },
            { "user message", userMessage },
            { "standard", categoryName },
            { "recommendations mat", standardMarkdown },
        };

        var correlationId = Guid.NewGuid().ToString();

        return _notifyWorkflow.SendEmails(
            model,
            personalisation,
            correlationId,
            NotifyConstants.ShareMatStandardTemplateId
        );
    }

    public List<NotifySendResult> SendMatSingleRecommendationEmail(
        ShareByEmailModel model,
        ComponentTextBodyEntry textBody,
        string recommendationHeader,
        string categoryName,
        string matName,
        string topic,
        string schoolsList
    )
    {
        var userMessage = BuildUserMessage(model);
        var recommendationMarkdown = MarkdownRenderer.Render(textBody.RichText);

        var personalisation = new Dictionary<string, object>
        {
            { "name of user", model.NameOfUser },
            { "school", matName },
            { "standard", categoryName.ToLowerInvariant() },
            { "user message", userMessage },
            { "recommendation name", recommendationHeader },
            { "recommendation", recommendationMarkdown },
            { "topic", topic.ToLowerInvariant() },
            { "schools list", schoolsList },
        };

        var correlationId = Guid.NewGuid().ToString();

        return _notifyWorkflow.SendEmails(
            model,
            personalisation,
            correlationId,
            NotifyConstants.ShareMatSingleRecommendationTemplateId
        );
    }

    private static string BuildUserMessage(ShareByEmailModel model)
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

        return userMessage.Trim();
    }

    private static string BuildMatStandardMarkdown(
        List<QuestionnaireSectionEntry> sections
    )
    {
        var stringBuilder = new StringBuilder();

        foreach (var section in sections)
        {
            stringBuilder.AppendLine(
                $"## Recommendations for {section.Name.ToLowerInvariant()}"
            );
            stringBuilder.AppendLine();

            var recommendations = section.CoreRecommendations.ToList();

            for (var i = 0; i < recommendations.Count; i++)
            {
                var recommendation = recommendations[i];

                stringBuilder.AppendLine(
                    $"{i + 1}. {recommendation.HeaderText}"
                );
            }

            stringBuilder.AppendLine();
        }

        return stringBuilder.ToString();
    }

    private static string BuildStandardMarkdown(
        List<QuestionnaireSectionEntry> sections,
        List<SqlSectionStatusDto> sectionStatuses,
        Dictionary<string, SqlEstablishmentRecommendationHistoryDto> recommendationStatuses
    )
    {
        var stringBuilder = new StringBuilder();

        foreach (var section in sections)
        {
            var sectionStatus = sectionStatuses.FirstOrDefault(ss =>
                ss.SectionId.Equals(section.Id)
            );
            var coreRecommendationIds = section.CoreRecommendations.Select(cr => cr.Id).ToList();

            var sectionRecommendationStatuses = recommendationStatuses
                .Where(rs => coreRecommendationIds.Contains(rs.Key))
                .ToDictionary();

            var sectionMarkdown = BuildSectionMarkdown(
                section.Name,
                sectionStatus,
                section.CoreRecommendations.ToList(),
                sectionRecommendationStatuses
            );

            stringBuilder.AppendLine(sectionMarkdown);
        }

        return stringBuilder.ToString();
    }

    private static string BuildSectionMarkdown(
        string sectionName,
        SqlSectionStatusDto? sectionStatus,
        List<RecommendationChunkEntry> recommendationChunks,
        Dictionary<string, SqlEstablishmentRecommendationHistoryDto> recommendationStatuses
    )
    {
        var stringBuilder = new StringBuilder();

        stringBuilder.AppendLine($"## Recommendations for {sectionName.ToLowerInvariant()}");
        stringBuilder.AppendLine();

        if (sectionStatus?.LastCompletionDate is null)
        {
            stringBuilder.AppendLine($"The self-assessment hasn't been completed.");
        }
        else
        {
            var completionDate = sectionStatus.LastCompletionDate.Value.ToString("dd MMMM yyyy");
            stringBuilder.AppendLine(
                $"The self-assessment for {sectionName.ToLowerInvariant()} was completed on {completionDate}."
            );
            stringBuilder.AppendLine();

            for (var i = 0; i < recommendationChunks.Count; i++)
            {
                var recommendationEntry = recommendationChunks[i];
                var recommendationStatus =
                    recommendationStatuses
                        .FirstOrDefault(rs => rs.Key.Equals(recommendationEntry.Id))
                        .Value
                    ?? throw new InvalidOperationException(
                        "Cannot prepare markdown for a recommendation that does not exist in the database."
                    );

                var status = recommendationStatus.NewStatus ?? RecommendationStatus.NotStarted;
                stringBuilder.AppendLine(
                    $"{i + 1}. {status.GetDisplayName()}: {recommendationEntry.Header}"
                );
            }
        }

        return stringBuilder.ToString();
    }
}
