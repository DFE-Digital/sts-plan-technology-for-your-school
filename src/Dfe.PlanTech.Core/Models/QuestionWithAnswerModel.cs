using System.ComponentModel.DataAnnotations;
using Dfe.PlanTech.Core.Contentful.Models;
using Dfe.PlanTech.Core.DataTransferObjects.Sql;

namespace Dfe.PlanTech.Core.RoutingDataModel;

public record QuestionWithAnswerModel
{
    [Required]
    public string QuestionSysId { get; init; } = null!;

    public string? QuestionSlug { get; init; }

    [Required]
    public string QuestionText { get; init; } = "";

    [Required]
    public string AnswerSysId { get; init; } = null!;

    [Required]
    public string AnswerText { get; init; } = "";

    [Required]
    public DateTime? DateCreated { get; init; } = null!;

    public QuestionWithAnswerModel(SqlResponseDto response, QuestionnaireSectionEntry section)
    {
        QuestionSysId = response.Question.ContentfulSysId;
        QuestionSlug = section.Questions.FirstOrDefault(q => q.Sys.Id.Equals(response.Question.ContentfulSysId))?.Slug;
        QuestionText = response.Question.QuestionText ?? string.Empty; //Should this come from Contentful?
        AnswerSysId = response.Answer.ContentfulSysId;
        AnswerText = response.Answer.AnswerText ?? string.Empty; //Should this come from Contentful?
        DateCreated = response.DateCreated;
    }
}
