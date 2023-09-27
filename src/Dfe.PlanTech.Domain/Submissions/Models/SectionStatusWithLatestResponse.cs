using Dfe.PlanTech.Domain.Responses.Models;

namespace Dfe.PlanTech.Domain.Submissions.Models;

public record SectionStatusWithLatestResponse(SectionStatus? SectionStatus, SectionResponseDto? LatestResponse);

public record SectionResponseDto(string QuestionContentfulId, string AnswerContentfulId);