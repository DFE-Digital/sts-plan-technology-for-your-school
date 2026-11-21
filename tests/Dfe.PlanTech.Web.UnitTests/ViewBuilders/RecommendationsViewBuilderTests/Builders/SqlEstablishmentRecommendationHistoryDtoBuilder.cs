using Dfe.PlanTech.Core.DataTransferObjects.Sql;
using Dfe.PlanTech.Core.Enums;

namespace Dfe.PlanTech.Web.UnitTests.ViewBuilders.RecommendationsViewBuilderTests.Builders;

public sealed class SqlEstablishmentRecommendationHistoryDtoBuilder
{
    private RecommendationStatus? _newStatus = RecommendationStatus.InProgress;
    private DateTime _dateCreated = new(2025, 11, 14, 9, 0, 0, DateTimeKind.Utc);

    public SqlEstablishmentRecommendationHistoryDtoBuilder WithNewStatus(
        RecommendationStatus? newStatus
    )
    {
        _newStatus = newStatus;
        return this;
    }

    public SqlEstablishmentRecommendationHistoryDtoBuilder WithDateCreated(DateTime dateCreated)
    {
        _dateCreated = dateCreated;
        return this;
    }

    public SqlEstablishmentRecommendationHistoryDto Build() =>
        new() { NewStatus = _newStatus, DateCreated = _dateCreated };
}
