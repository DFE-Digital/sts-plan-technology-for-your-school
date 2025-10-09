﻿using Dfe.PlanTech.Core.DataTransferObjects.Sql;

namespace Dfe.PlanTech.Application.Workflows.Interfaces;

public interface IRecommendationWorkflow
{
    Task<SqlEstablishmentRecommendationHistoryDto?> GetCurrentRecommendationStatusAsync(
        string recommendationContentfulReference,
        int establishmentId
    );

    Task<Dictionary<string, SqlEstablishmentRecommendationHistoryDto>> GetLatestRecommendationStatusesByRecommendationIdAsync(
            IEnumerable<string> recommendationContentfulReferences,
            int establishmentId
        );

    Task UpdateRecommendationStatusAsync(
        string recommendationContentfulReference,
        int establishmentId,
        int userId,
        string newStatus,
        string? noteText = null,
        int? matEstablishmentId = null
    );
}
