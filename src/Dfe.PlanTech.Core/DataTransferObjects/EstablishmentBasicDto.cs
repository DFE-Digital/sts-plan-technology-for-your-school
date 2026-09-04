namespace Dfe.PlanTech.Core.DataTransferObjects;

public class EstablishmentBasicDto
{
    public int DboId { get; set; }
    public string Urn { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public int InProgressOrCompletedRecommendationsCount { get; set; } = 0;
    public string GroupUid { get; set; } = string.Empty;
}
