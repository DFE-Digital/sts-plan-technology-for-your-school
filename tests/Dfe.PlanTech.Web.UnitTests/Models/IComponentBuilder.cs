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