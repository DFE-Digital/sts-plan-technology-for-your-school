using Dfe.PlanTech.Core.DataTransferObjects.Contentful;
using Dfe.PlanTech.Core.Models;
using Dfe.PlanTech.Core.RoutingDataModel;

namespace Dfe.PlanTech.Web.ViewModels;

public class AnswerViewModel
{
    public AnswerViewModel(CmsQuestionnaireAnswerDto answer)
    {
        Maturity = answer.Maturity;
        Answer = new IdWithTextModel
        {
            Id = answer.Id!,
            Text = answer.Text
        };
    }

    public AnswerViewModel()
    {
        Answer = new IdWithTextModel();
    }

    public string Id
    {
        get => Answer.Id;
        set => Answer.Id = value!;
    }

    public string Text
    {
        get => Answer.Text;
        set => Answer.Text = value!;
    }

    public IdWithTextModel Answer { get; set; } = null!;

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
