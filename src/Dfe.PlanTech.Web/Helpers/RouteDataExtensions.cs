namespace Dfe.PlanTech.Web.Helpers;

public static class RouteDataExtensions
{
    public const string DefaultPageTitle = "Plan Technology For Your School";
    public const string SectionSlugKey = "sectionSlug";

    // Returns the title for the page based on the given RouteData. It retrieves the sectionSlug
    // from the routeData.Values, processes it to ensure it is not empty, and then formats it
    // as a title by replacing hyphens with spaces, converting to Title Case, and adding
    // white space before every capital letter. If the sectionSlug is empty, it returns the
    // DefaultPageTitle.
    public static string GetTitleForPage(this RouteData routeData)
    {
        var sectionSlug = routeData.Values.Where(routePart =>
                                        {
                                            if (routePart.Key == SectionSlugKey)
                                            {
                                                return true;
                                            }

                                            var routePartValue = routePart.Value as string;


                                            return !string.IsNullOrEmpty(routePartValue) && routePartValue != "/" && !routePartValue.Any(char.IsNumber);
                                        })
                                        .OrderByDescending(routePart => routePart.Key)
                                        .Select(routePart => routePart.Value!.ToString())
                                        .FirstOrDefault();

        if (string.IsNullOrEmpty(sectionSlug))
            return DefaultPageTitle;

        //Replace hyphens with spaces, then convert to Title Case (i.e. capitalise words)
        sectionSlug = sectionSlug.Replace("-", " ");
        sectionSlug = string.Concat(sectionSlug[0].ToString().ToUpper(), sectionSlug.AsSpan(1));

        //Add white space before every capital letter
        sectionSlug = string.Concat(sectionSlug.Select(c => char.IsUpper(c) ? " " + c : c.ToString())).Trim();

        return sectionSlug;
    }
}
