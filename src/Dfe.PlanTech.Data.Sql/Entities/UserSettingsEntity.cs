using System.ComponentModel.DataAnnotations.Schema;
using Dfe.PlanTech.Core.DataTransferObjects.Sql;
using Dfe.PlanTech.Core.Helpers;

namespace Dfe.PlanTech.Data.Sql.Entities;

[Table("userSettings")]
public class UserSettingsEntity
{
    public int UserId { get; set; }

    public string? SortOrder { get; set; }

    public SqlUserSettingsDto AsDto()
    {
        return new SqlUserSettingsDto
        {
            UserId = UserId,
            SortOrder = SortOrder.GetRecommendationSortEnumValue(),
        };
    }
}
