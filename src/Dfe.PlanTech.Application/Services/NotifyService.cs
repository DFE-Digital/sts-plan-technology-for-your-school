using Dfe.PlanTech.Application.Rendering.Markdown;
using Dfe.PlanTech.Application.Services.Interfaces;
using Dfe.PlanTech.Application.Workflows.Interfaces;
using Dfe.PlanTech.Core.Contentful.Models;

namespace Dfe.PlanTech.Application.Services;

public class NotifyService(IContentfulWorkflow contentfulWorkflow) : INotifyService
{
    private readonly IContentfulWorkflow _contentfulWorkflow =
        contentfulWorkflow ?? throw new ArgumentNullException(nameof(contentfulWorkflow));

    public async Task SendEmailAsync(
        string recommendationRef,
        ICollection<string> recipients,
        string subject,
        string body
    )
    {
        var recommendationChunk = await _contentfulWorkflow.GetEntryById<RecommendationChunkEntry>(
            recommendationRef
        );
        var textBody = await _contentfulWorkflow.GetEntryById<ComponentTextBodyEntry>(
            recommendationChunk.Content[0].Id
        );

        var markdownRenderer = new MarkdownRenderer();
        var markdown = markdownRenderer.Render(textBody.RichText);

        // Placeholder for email sending logic
    }
}
