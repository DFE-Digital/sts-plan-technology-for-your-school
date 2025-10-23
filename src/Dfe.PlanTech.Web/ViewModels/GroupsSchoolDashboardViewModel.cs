using System.Diagnostics.CodeAnalysis;
using Dfe.PlanTech.Core.Contentful.Models;

namespace Dfe.PlanTech.Web.ViewModels;

[ExcludeFromCodeCoverage]
public class GroupsSchoolDashboardViewModel
{
    public List<ContentfulEntry> BeforeTitleContent { get; init; } = [];
    public List<ContentfulEntry> Content { get; init; } = null!;
    public string? ErrorMessage { get; set; }
    public string GroupName { get; set; } = null!;
    public int SchoolId { get; set; }
    public string? SchoolName { get; set; }
    public string Slug { get; set; } = null!;
    public ComponentTitleEntry Title { get; set; } = null!;
}
