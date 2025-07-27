using System.ComponentModel.DataAnnotations;
using Dfe.PlanTech.Core.DataTransferObjects.Sql;
using Dfe.PlanTech.Core.RoutingDataModel;

namespace Dfe.PlanTech.Web.Models;

public class SubmissionResponseViewModel
{
    [Required]
    public string QuestionRef { get; init; } = null!;

    [Required]
    public string? QuestionText { get; init; } = "";

    [Required]
    public string AnswerRef { get; init; } = null!;

    [Required]
    public string? AnswerText { get; init; } = "";

    [Required]
    public DateTime? DateCreated { get; init; } = null!;

    public string? QuestionSlug { get; set; }

    public SubmissionResponseViewModel(SqlResponseDto response)
    {
        var question = response.Question;
        var answer = response.Answer;

        QuestionRef = question.ContentfulSysId;
        QuestionText = question.QuestionText;
        AnswerRef = answer.ContentfulSysId;
        AnswerText = answer.AnswerText;
        DateCreated = response.DateCreated;
    }

    public SubmissionResponseViewModel(QuestionWithAnswerModel response)
    {
        QuestionRef = response.QuestionSysId;
        QuestionText = response.QuestionText;
        AnswerRef = response.AnswerSysId;
        AnswerText = response.AnswerText;
        DateCreated = response.DateCreated;
    }
}
