namespace Dfe.PlanTech.Domain.Questionnaire.Models;

public class AnswerViewModelDto
{
  public AnswerViewModelDto(Answer answer)
  {
    Answer = new IdWithText(Id: answer.Sys.Id, Text: answer.Text);
    NextQuestion = answer.NextQuestion != null ? new SlugWithText(Slug: answer.NextQuestion!.Slug, Text: answer.NextQuestion!.Text) : null;
    Maturity = answer.Maturity;
  }

  public AnswerViewModelDto()
  {

  }

  public IdWithText Answer { get; set; }

  public SlugWithText? NextQuestion { get; set; }

  public string Maturity { get; init; } = null!;
}
