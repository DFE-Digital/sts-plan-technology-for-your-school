using Dfe.PlanTech.Core.Contentful.Models;

namespace Dfe.PlanTech.Web.UnitTests.ViewBuilders.RecommendationsViewBuilderTests.Builders;

public sealed class QuestionnaireCategoryEntryBuilder
{
    private ComponentHeaderEntry? _header = new() { Text = "Networking" };
    private PageEntry? _landingPage = new() { Slug = "networking-landing-page" };

    public QuestionnaireCategoryEntryBuilder WithHeaderText(string headerText)
    {
        _header = new ComponentHeaderEntry { Text = headerText };
        return this;
    }

    public QuestionnaireCategoryEntryBuilder WithoutHeader()
    {
        _header = null;
        return this;
    }

    public QuestionnaireCategoryEntryBuilder WithoutLandingPage()
    {
        _landingPage = null;
        return this;
    }

    public QuestionnaireCategoryEntry Build() =>
        new() { Header = _header!, LandingPage = _landingPage };
}
