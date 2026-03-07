namespace Dfe.PlanTech.Core.Models;

public class ShareByEmailModel
{
    public string NameOfUser { get; set; } = null!;
    public List<string> EmailAddresses { get; set; } = [];
    public string? UserMessage { get; set; }
}
