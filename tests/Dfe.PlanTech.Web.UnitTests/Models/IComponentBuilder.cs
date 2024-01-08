using Dfe.PlanTech.Domain.Content.Models;
using Dfe.PlanTech.Domain.Content.Models.Buttons;
using Dfe.PlanTech.Domain.Questionnaire.Enums;
using Dfe.PlanTech.Domain.Questionnaire.Models;

namespace Dfe.PlanTech.Web.UnitTests.Models
{
    public interface IComponentBuilder
    {
        ButtonWithLink BuildButtonWithLink();
        ButtonWithEntryReference BuildButtonWithEntryReference();
        Category BuildCategory();
        ComponentDropDown BuildDropDownComponent();
        InsetText BuildInsetText();
        TextBody BuildTextBody();
        RecommendationPage BuildRecommendationsPage(Maturity maturity = Maturity.Unknown);
        List<Section> BuildSections();
    }
}