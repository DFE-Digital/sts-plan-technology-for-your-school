using System.Diagnostics.CodeAnalysis;

namespace Dfe.PlanTech.Core.Models;

[ExcludeFromCodeCoverage]
public class ShareByEmailModel
{
    public string NameOfUser { get; set; } = null!;
    public List<string> EmailAddresses { get; set; } = [];
    public string? UserMessage { get; set; }
}
