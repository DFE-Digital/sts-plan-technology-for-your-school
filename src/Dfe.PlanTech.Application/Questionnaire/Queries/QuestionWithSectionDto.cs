using Dfe.PlanTech.Domain.Questionnaire.Models;

namespace Dfe.PlanTech.Application.Questionnaire.Queries;

public readonly record struct QuestionWithSectionDto(Question Question, string SectionId, string SectionName, string SectionSlug);