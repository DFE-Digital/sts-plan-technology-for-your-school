namespace Dfe.PlanTech.Domain.Submissions.Models;

public record SectionStatusWithLatestResponse(SectionStatusNew? SectionStatus, SectionResponseDto? LatestResponse);

public record SectionResponseDto(string QuestionContentfulId, string AnswerContentfulId);