using Dfe.PlanTech.Application.Providers.Interfaces;
using Dfe.PlanTech.Core.Constants;
using Dfe.PlanTech.Web.Validators.Interfaces;
using Dfe.PlanTech.Web.ViewModels;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Dfe.PlanTech.Web.Validators;

public class GroupSelectSchoolsToUpdateStatusValidator(IMicrocopyProvider microcopy)
    : IGroupSelectSchoolsToUpdateStatusValidator
{
    public async Task ValidateSelectionAsync(
        GroupsSelectSchoolsToUpdateStatusViewModel model,
        ModelStateDictionary modelState
    )
    {
        var selectedSchools = model.SelectedSchoolsRefs ?? [];

        if (selectedSchools.Count == 0)
        {
            var noSelectionError = await microcopy.GetTextByKeyAsync(ContentfulMicrocopyConstants.GroupsSelectSchoolsToUpdateStatusNoSelectionError);

            modelState.AddModelError(
                nameof(model.SelectedSchoolsRefs),
                noSelectionError
                );
        }
    }
}
