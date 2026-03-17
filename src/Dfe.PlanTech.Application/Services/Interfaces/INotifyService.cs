using Dfe.PlanTech.Core.Contentful.Models;
using Dfe.PlanTech.Core.DataTransferObjects.Sql;
using Dfe.PlanTech.Core.Enums;
using Dfe.PlanTech.Core.Models;

namespace Dfe.PlanTech.Application.Services.Interfaces;

public interface INotifyService
{
    List<NotifySendResult> SendSingleRecommendationEmail(
        ShareByEmailModel model,
        ComponentTextBodyEntry textBody,
        string establishmentName,
        string recommendationHeader,
        string sectionName,
        RecommendationStatus recommendationStatus
    );

    List<NotifySendResult> SendStandardEmail(
        ShareByEmailModel model,
        List<QuestionnaireSectionEntry> sections,
        List<SqlSectionStatusDto> sectionStatuses,
        Dictionary<string, SqlEstablishmentRecommendationHistoryDto> recommendationStatuses,
        string categoryName,
        string establishmentName
    );
}
