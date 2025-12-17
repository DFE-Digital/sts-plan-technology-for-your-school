using Dfe.PlanTech.Core.Enums;

namespace Dfe.PlanTech.Core.DataTransferObjects.Sql;

public class SqlUserSettingsDto : ISqlDto
{
    public int UserId { get; set; }
    public RecommendationSortOrder? SortOrder { get; set; }
}
