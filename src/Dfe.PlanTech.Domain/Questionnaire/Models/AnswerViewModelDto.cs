namespace Dfe.PlanTech.Domain.Questionnaire.Models;

public class AnswerViewModelDto
{
    public AnswerViewModelDto(Answer answer)
    {
        Maturity = answer.Maturity;
        Answer = new IdWithText(Id: answer.Sys.Id, Text: answer.Text);
        NextQuestion = answer.NextQuestion != null ?
                        new IdAndSlugAndText(Slug: answer.NextQuestion!.Slug, Text: answer.NextQuestion!.Text, Id: answer.NextQuestion.Sys.Id) :
                        null;
    }

    public AnswerViewModelDto()
    {

    }

    public IdWithText Answer { get; set; }

    public IdAndSlugAndText? NextQuestion { get; set; }

    public string Maturity { get; init; } = null!;
}

public readonly record struct IdAndSlugAndText(string Id, string Text, string Slug);
