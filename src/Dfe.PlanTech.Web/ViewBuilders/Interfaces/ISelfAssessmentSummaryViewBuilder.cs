using Microsoft.AspNetCore.Mvc;

namespace Dfe.PlanTech.Web.ViewBuilders.Interfaces;

public interface ISelfAssessmentSummaryViewBuilder
{
    Task<IActionResult> RouteToSelfAssessmentSummary(
        Controller controller,
        string categorySlug,
        string sectionSlug
    );
}
