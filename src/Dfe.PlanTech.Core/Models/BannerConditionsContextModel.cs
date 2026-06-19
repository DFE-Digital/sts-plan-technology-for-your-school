namespace Dfe.PlanTech.Core.Models;

public class BannerConditionsContextModel
{
    public int TimesShown { get; set; }
    public bool IsSchoolUser { get; set; }
    public bool IsGroupUser { get; set; }
    public bool IsUnknownStatus { get; set; }
    public bool IsNotStartedStatus { get; set; }
    public bool IsInProgressStatus { get; set; }
    public bool IsCompletedStatus { get; set; }
}
