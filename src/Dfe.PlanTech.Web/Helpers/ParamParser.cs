using Dfe.PlanTech.Domain.Questionnaire.Models;

namespace Dfe.PlanTech.Web.Helpers
{
    public static class ParamParser
    {
        public static Params? _ParseParameters(string? parameters)
        {
            if (string.IsNullOrEmpty(parameters))
            {
                return null;
            }

            string[]? splitParams = parameters.Split('+');

            if (splitParams is null)
            {
                return null;
            }
            else
            {
                return new Params
                {
                    SectionName = splitParams.Length > 0 ? splitParams[0].ToString() : string.Empty,
                    SectionId = splitParams.Length > 1 ? splitParams[1].ToString() : string.Empty,
                    SectionSlug = splitParams.Length > 2 ? splitParams[2].ToString() : string.Empty,
                };
            }
        }
    }
}
