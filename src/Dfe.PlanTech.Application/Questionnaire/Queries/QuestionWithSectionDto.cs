using Dfe.PlanTech.Domain.Questionnaire.Models;

namespace Dfe.PlanTech.Application.Questionnaire.Queries;

//TODO: Refactor section to own DTO
public readonly record struct QuestionWithSectionDto(Question Question, string SectionId, string SectionName, string SectionSlug);