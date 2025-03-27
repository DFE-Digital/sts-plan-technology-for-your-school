using Dfe.PlanTech.Domain.Content.Models;
using Dfe.PlanTech.Domain.Content.Models.Buttons;
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
        List<Section> BuildSections();
        Page BuildPage();
    }
}
