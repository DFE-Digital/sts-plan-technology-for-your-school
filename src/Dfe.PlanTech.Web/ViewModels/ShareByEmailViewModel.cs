using Dfe.PlanTech.Core.Models;
using Dfe.PlanTech.Web.ViewModels.Inputs;

namespace Dfe.PlanTech.Web.ViewModels;

public class ShareByEmailViewModel
{
    public required string PostController { get; set; }
    public required string PostAction { get; set; }
    public required string CategorySlug { get; set; }
    public string? SectionSlug { get; set; }
    public string? ChunkSlug { get; set; }
    public required string Caption { get; set; }
    public required string Heading { get; set; }
    public ShareByEmailInputViewModel? InputModel { get; set; }
}
