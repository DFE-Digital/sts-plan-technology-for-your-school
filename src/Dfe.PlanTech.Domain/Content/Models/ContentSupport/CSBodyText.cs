using System.Diagnostics.CodeAnalysis;
using Dfe.PlanTech.Domain.Content.Interfaces;
using Dfe.PlanTech.Domain.Content.Models;

namespace Dfe.PlanTech.Domain.Content.Models.ContentSupport;

[ExcludeFromCodeCoverage]
public class CSBodyText : Target
{
	public RichTextContent RichText { get; set; } = null!;
}
