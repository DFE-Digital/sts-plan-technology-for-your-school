namespace Dfe.PlanTech.Domain.ContentfulEntries.Questionnaire.Models;

public class AnswerViewModelDto
{
    public AnswerViewModelDto(AnswerEntry answer)
    {
        Maturity = answer.Maturity;
        Answer = new IdWithText(Id: answer.Sys.Id, Text: answer.Text);
    }

    public AnswerViewModelDto()
    {

    }

    public IdWithText Answer { get; set; }

    public string Maturity { get; init; } = null!;
}

public readonly record struct IdAndSlugAndText(string Id, string Text, string Slug);
