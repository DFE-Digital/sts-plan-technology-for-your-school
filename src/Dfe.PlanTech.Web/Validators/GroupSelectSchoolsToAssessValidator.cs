using Dfe.PlanTech.Application.Providers.Interfaces;
using Dfe.PlanTech.Core.Constants;
using Dfe.PlanTech.Web.Validators.Interfaces;
using Dfe.PlanTech.Web.ViewModels;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Dfe.PlanTech.Web.Validators;

public class GroupSelectSchoolsToAssessValidator : IGroupSelectSchoolsToAssessValidator
{
    private readonly IMicrocopyProvider _microcopy;

    public GroupSelectSchoolsToAssessValidator(
        IMicrocopyProvider microcopy)
    {
        _microcopy = microcopy;
    }

    public async Task ValidateSelectionAsync(
        GroupsSelectSchoolsToAssessViewModel model,
        ModelStateDictionary modelState
    )
    {
        var selectedSchools = model.SelectedSchoolsRefs ?? [];

        if (selectedSchools.Count() == 0)
        {
            var noSelectionError = await _microcopy.GetTextByKeyAsync(ContentfulMicrocopyConstants.GroupsSelectSchoolsToAssessNoSelectionError);

            modelState.AddModelError(
                nameof(model.SelectedSchoolsRefs),
                noSelectionError
                );
        }

        if (selectedSchools.Contains("all") && selectedSchools.Count > 1)
        {
            var conflictError = await _microcopy.GetTextByKeyAsync(ContentfulMicrocopyConstants.GroupsSelectSchoolsToAssessConflictError);

            modelState.AddModelError(
                nameof(model.SelectedSchoolsRefs),
                conflictError
                );
        }
    }
}
