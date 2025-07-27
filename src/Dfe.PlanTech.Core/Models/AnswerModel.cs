using Dfe.PlanTech.Core.RoutingDataModel;

namespace Dfe.PlanTech.Core.Models
{
    public class AnswerModel
    {
        public IdWithTextModel Answer { get; set; } = null!;
        public string Maturity { get; init; } = null!;
    }
}
