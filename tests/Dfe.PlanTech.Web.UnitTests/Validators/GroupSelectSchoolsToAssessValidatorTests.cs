using Dfe.PlanTech.Application.Providers.Interfaces;
using Dfe.PlanTech.Core.Constants;
using Dfe.PlanTech.Web.Validators;
using Dfe.PlanTech.Web.ViewModels.Inputs;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using NSubstitute;

namespace Dfe.PlanTech.Web.UnitTests.Validators;

public class GroupSelectSchoolsToAssessValidatorTests
{

    private readonly IMicrocopyProvider _microcopy;
    private readonly GroupSelectSchoolsToAssessValidator _validator;

    public GroupSelectSchoolsToAssessValidatorTests()
    {
        _microcopy = Substitute.For<IMicrocopyProvider>();
        _validator = new GroupSelectSchoolsToAssessValidator(_microcopy);
    }

    [Fact]
    public async Task ValidateSelectionAsync_WhenNoSchoolsSelected_AddsError()
    {
        // Arrange
        var model = new GroupSelectSchoolsToAssessInputViewModel
        {
            SelectedSchoolsRefs = []
        };
        const string errorMessage = "Select at least one school";

        _microcopy.GetTextByKeyAsync(ContentfulMicrocopyConstants.GroupsSelectSchoolsToAssessNoSelectionError)
            .Returns(errorMessage);

        var modelState = new ModelStateDictionary();

        // Act
        await _validator.ValidateSelectionAsync(model, modelState);

        // Assert
        Assert.False(modelState.IsValid);

        Assert.Contains(
            modelState[nameof(model.SelectedSchoolsRefs)]!.Errors,
            e => e.ErrorMessage == errorMessage);
    }

    [Fact]
    public async Task ValidateSelectionAsync_WhenSelectedSchoolsIsNull_AddsError()
    {
        // Arrange
        const string errorMessage = "Select at least one school";

        _microcopy
            .GetTextByKeyAsync(
                ContentfulMicrocopyConstants.GroupsSelectSchoolsToAssessNoSelectionError)
            .Returns(errorMessage);

        var model = new GroupSelectSchoolsToAssessInputViewModel
        {
            SelectedSchoolsRefs = null
        };

        var modelState = new ModelStateDictionary();

        // Act
        await _validator.ValidateSelectionAsync(model, modelState);

        // Assert
        Assert.False(modelState.IsValid);

        Assert.Contains(
            modelState[nameof(model.SelectedSchoolsRefs)]!.Errors,
            e => e.ErrorMessage == errorMessage);
    }


    [Fact]
    public async Task ValidateSelectionAsync_WhenOnlyAllSelected_DoesNotAddError()
    {
        // Arrange
        var model = new GroupSelectSchoolsToAssessInputViewModel
        {
            SelectedSchoolsRefs = ["all"]
        };

        var modelState = new ModelStateDictionary();

        // Act
        await _validator.ValidateSelectionAsync(model, modelState);

        // Assert
        Assert.True(modelState.IsValid);
    }

    [Fact]
    public async Task ValidateSelectionAsync_WhenOneSchoolSelected_DoesNotAddError()
    {
        // Arrange
        var model = new GroupSelectSchoolsToAssessInputViewModel
        {
            SelectedSchoolsRefs = ["000001"]
        };

        var modelState = new ModelStateDictionary();

        // Act
        await _validator.ValidateSelectionAsync(model, modelState);

        // Assert
        Assert.True(modelState.IsValid);
    }


    [Fact]
    public async Task ValidateSelectionAsync_WhenMultipleSchoolsSelected_DoesNotAddError()
    {
        // Arrange
        var model = new GroupSelectSchoolsToAssessInputViewModel
        {
            SelectedSchoolsRefs = ["000001", "000002"]
        };

        var modelState = new ModelStateDictionary();

        // Act
        await _validator.ValidateSelectionAsync(model, modelState);

        // Assert
        Assert.True(modelState.IsValid);
    }


    [Fact]
    public async Task ValidateSelectionAsync_UsesMicrocopyProviderErrorMessage()
    {
        // Arrange
        const string errorMessage =
            "Select either all schools or individual schools";

        _microcopy
            .GetTextByKeyAsync(
                ContentfulMicrocopyConstants.GroupsSelectSchoolsToAssessConflictError)
            .Returns(errorMessage);

        var model = new GroupSelectSchoolsToAssessInputViewModel
        {
            SelectedSchoolsRefs = ["all", "000001"]
        };

        var modelState = new ModelStateDictionary();

        // Act
        await _validator.ValidateSelectionAsync(model, modelState);

        // Assert
        Assert.False(modelState.IsValid);

        Assert.Contains(
            modelState[nameof(model.SelectedSchoolsRefs)]!.Errors,
            e => e.ErrorMessage == errorMessage);
    }
}

