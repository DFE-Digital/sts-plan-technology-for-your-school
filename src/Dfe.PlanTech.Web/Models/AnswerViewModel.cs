using Dfe.PlanTech.Core.DataTransferObjects.Contentful;
using Dfe.PlanTech.Core.Models;
using Dfe.PlanTech.Core.RoutingDataModel;

namespace Dfe.PlanTech.Web.Models;

public class AnswerViewModel
{
    public AnswerViewModel(CmsQuestionnaireAnswerDto answer)
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
