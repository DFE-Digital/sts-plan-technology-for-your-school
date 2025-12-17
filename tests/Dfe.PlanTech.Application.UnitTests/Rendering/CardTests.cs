using System.Text;
using Dfe.PlanTech.Core.Contentful.Models;

namespace Dfe.PlanTech.Application.UnitTests.Rendering;

public class CardTests
{
    protected ComponentCardEntry GetFirstCard()
    {
        return new ComponentCardEntry()
        {
            InternalName = "FirstCard",
            Uri = "http://www.linktofirstcard.com",
            Description = "Description of first card"
        };
    }

    protected ComponentCardEntry GetSecondCard()
    {
        return new ComponentCardEntry()
        {
            InternalName = "SecondCard",
            Uri = "http://www.linktosecondcard.com",
            Description = "Description of second card"
        };
    }

    protected ComponentCardEntry GetThirdCard()
    {
        return new ComponentCardEntry()
        {
            InternalName = "ThirdCard",
            Uri = "http://www.linktothirdcard.com",
            Description = "Description of third card"
        };
    }

    protected ComponentCardEntry GetFourthCard()
    {
        return new ComponentCardEntry()
        {
            InternalName = "FourthCard",
            Uri = "http://www.linktoffourthcard.com",
            Description = "Description of fourth card"
        };
    }

    protected StringBuilder GetFirstCardView()
    {
        var stringBuilder = new StringBuilder();
        stringBuilder.Append("<div class=\"dfe-card\">");
        stringBuilder.Append("<div class=\"dfe-card-container\">");
        stringBuilder.Append("<h3 class=\"govuk-heading-m\">");
        stringBuilder.Append($"<a href=\"http://www.linktofirstcard.com\" class=\"govuk-link govuk-link--no-visited-state dfe-card-link--header\">FirstCard</a>");
        stringBuilder.Append("</h3>");
        stringBuilder.Append($"<p class=\"govuk-body-s\">Description of first card</p>");
        stringBuilder.Append("</div></div>");

        return stringBuilder;
    }

    protected StringBuilder GetSecondCardView()
    {
        var stringBuilder = new StringBuilder();
        stringBuilder.Append("<div class=\"dfe-card\">");
        stringBuilder.Append("<div class=\"dfe-card-container\">");
        stringBuilder.Append("<h3 class=\"govuk-heading-m\">");
        stringBuilder.Append($"<a href=\"http://www.linktosecondcard.com\" class=\"govuk-link govuk-link--no-visited-state dfe-card-link--header\">SecondCard</a>");
        stringBuilder.Append("</h3>");
        stringBuilder.Append($"<p class=\"govuk-body-s\">Description of second card</p>");
        stringBuilder.Append("</div></div>");

        return stringBuilder;
    }

    protected StringBuilder GetThirdCardView()
    {
        var stringBuilder = new StringBuilder();
        stringBuilder.Append("<div class=\"dfe-card\">");
        stringBuilder.Append("<div class=\"dfe-card-container\">");
        stringBuilder.Append("<h3 class=\"govuk-heading-m\">");
        stringBuilder.Append($"<a href=\"http://www.linktothirdcard.com\" class=\"govuk-link govuk-link--no-visited-state dfe-card-link--header\">ThirdCard</a>");
        stringBuilder.Append("</h3>");
        stringBuilder.Append($"<p class=\"govuk-body-s\">Description of third card</p>");
        stringBuilder.Append("</div></div>");

        return stringBuilder;
    }

    protected StringBuilder GetFourthCardView()
    {
        var stringBuilder = new StringBuilder();
        stringBuilder.Append("<div class=\"dfe-card\">");
        stringBuilder.Append("<div class=\"dfe-card-container\">");
        stringBuilder.Append("<h3 class=\"govuk-heading-m\">");
        stringBuilder.Append($"<a href=\"http://www.linktoffourthcard.com\" class=\"govuk-link govuk-link--no-visited-state dfe-card-link--header\">FourthCard</a>");
        stringBuilder.Append("</h3>");
        stringBuilder.Append($"<p class=\"govuk-body-s\">Description of fourth card</p>");
        stringBuilder.Append("</div></div>");

        return stringBuilder;
    }

    protected List<ComponentCardEntry> GetCards()
    {
        return
        [
            GetFirstCard(),
            GetSecondCard(),
            GetThirdCard(),
            GetFourthCard(),
        ];
    }

    protected StringBuilder GetCardViews(StringBuilder stringBuilder)
    {
        stringBuilder.Append(GetFirstCardView());
        stringBuilder.Append(GetSecondCardView());
        stringBuilder.Append(GetThirdCardView());
        stringBuilder.Append(GetFourthCardView());

        return stringBuilder;
    }
}
