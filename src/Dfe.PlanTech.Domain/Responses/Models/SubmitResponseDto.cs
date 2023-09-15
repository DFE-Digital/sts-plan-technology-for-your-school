namespace Dfe.PlanTech.Domain.Responses.Models;

public readonly record struct SubmitResponseDto(
  string SectionId,
  string SectionName,
  int EstablishmentId,
  int UserId,
  string QuestionContentfulId,
  string QuestionText,
  string AnswerContentfulId,
  string AnswerText,
  string Maturity
);