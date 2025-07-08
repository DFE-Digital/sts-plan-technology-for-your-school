using Dfe.PlanTech.Core.Models;
using Dfe.PlanTech.Infrastructure.Data.Contentful.Entries;

namespace Dfe.PlanTech.Web.ViewModels;

public class AnswerViewModel
{
    public AnswerViewModel(AnswerEntry answer)
    {
        Maturity = answer.Maturity;
        Answer = new IdWithTextModel
        {
            Id = answer.Sys.Id!,
            Text = answer.Text
        };
    }

    public IdWithTextModel Answer { get; set; }

    public string Maturity { get; init; } = null!;

    public AnswerModel ToModel()
    {
        return new AnswerModel
        {
            Maturity = Maturity,
            Answer = Answer,
        };
    }
}
