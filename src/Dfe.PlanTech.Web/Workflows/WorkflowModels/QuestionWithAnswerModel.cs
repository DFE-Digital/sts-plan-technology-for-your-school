using Dfe.PlanTech.Infrastructure.Data.Sql.Entities;

namespace Dfe.PlanTech.Domain.Submissions.Models;

public class QuestionWithAnswerModel
{
    public string QuestionRef { get; init; }

    public string QuestionText { get; init; }

    public string AnswerRef { get; init; }

    public string AnswerText { get; init; }

    public DateTime? DateCreated { get; init; }

    public string? QuestionSlug { get; set; }

    public QuestionWithAnswerModel(ResponseEntity response)
    {
        QuestionRef = response.Question.ContentfulRef;
        QuestionText = response.Question.QuestionText ?? string.Empty; //Should this come from Contentful?
        AnswerRef = response.Answer.ContentfulRef;
        AnswerText = response.Answer.AnswerText ?? string.Empty; //Should this come from Contentful?
        DateCreated = response.DateCreated;
    }
}
