using System.ComponentModel.DataAnnotations;
using Dfe.PlanTech.Web.ViewModels.Inputs;

namespace Dfe.PlanTech.Web.UnitTests.ViewModels;

public class ShareByEmailInputViewModelTests
{
    [Fact]
    public void Validate_WhenNoEmailsProvided_ReturnsValidationError()
    {
        var sut = new ShareByEmailInputViewModel
        {
            NameOfUser = "Drew",
            EmailAddresses = ["", "   "],
        };

        var results = Validate(sut);

        var error = Assert.Single(results);
        Assert.Equal("Please enter at least one email address", error.ErrorMessage);
        Assert.Contains(nameof(ShareByEmailInputViewModel.EmailAddresses), error.MemberNames);
    }

    [Fact]
    public void Validate_WhenNameMissing_ReturnsValidationError()
    {
        var sut = new ShareByEmailInputViewModel
        {
            NameOfUser = "",
            EmailAddresses = ["user@test.com"],
        };

        var results = Validate(sut);

        var error = Assert.Single(results);
        Assert.Equal("Enter your name", error.ErrorMessage);
        Assert.Contains(nameof(ShareByEmailInputViewModel.NameOfUser), error.MemberNames);
    }

    [Fact]
    public void Validate_WhenEmailInvalid_ReturnsIndexedValidationError()
    {
        var sut = new ShareByEmailInputViewModel
        {
            NameOfUser = "Drew",
            EmailAddresses = ["invalid-email"],
        };

        var results = Validate(sut);

        var error = Assert.Single(results);
        Assert.Equal("Enter a valid email address", error.ErrorMessage);
        Assert.Contains("EmailAddresses[0]", error.MemberNames);
    }

    [Fact]
    public void Validate_WhenEmailHasNoDomainDot_ReturnsValidationError()
    {
        var sut = new ShareByEmailInputViewModel
        {
            NameOfUser = "Drew",
            EmailAddresses = ["user@test"],
        };

        var results = Validate(sut);

        var error = Assert.Single(results);
        Assert.Equal("Enter a valid email address", error.ErrorMessage);
        Assert.Contains("EmailAddresses[0]", error.MemberNames);
    }

    [Fact]
    public void Validate_WhenEmailsContainWhitespace_TrimsEmails()
    {
        var sut = new ShareByEmailInputViewModel
        {
            NameOfUser = "Drew",
            EmailAddresses = ["   user@test.com   ", "   "],
        };

        var results = Validate(sut);

        Assert.Empty(results);
        Assert.Single(sut.EmailAddresses);
        Assert.Equal("user@test.com", sut.EmailAddresses[0]);
    }

    [Fact]
    public void Validate_WhenMultipleEmailsAndOneInvalid_ReturnsCorrectIndex()
    {
        var sut = new ShareByEmailInputViewModel
        {
            NameOfUser = "Drew",
            EmailAddresses = ["valid@test.com", "not-an-email", "another@test.com"],
        };

        var results = Validate(sut);

        var error = Assert.Single(results);
        Assert.Contains("EmailAddresses[1]", error.MemberNames);
    }

    [Fact]
    public void Validate_WhenAllInputsValid_ReturnsNoErrors()
    {
        var sut = new ShareByEmailInputViewModel
        {
            NameOfUser = "Drew",
            EmailAddresses = ["user@test.com", "another@test.co.uk"],
            UserMessage = "Hello!",
        };

        var results = Validate(sut);

        Assert.Empty(results);
    }

    [Fact]
    public void ToModel_ReturnsEquivalentModel()
    {
        var sut = new ShareByEmailInputViewModel
        {
            NameOfUser = "Drew",
            EmailAddresses = ["user@test.com", "another@test.com"],
            UserMessage = "Hello!",
        };

        var model = sut.ToModel();

        Assert.Equal("Drew", model.NameOfUser);
        Assert.Equal(sut.EmailAddresses, model.EmailAddresses);
        Assert.Equal("Hello!", model.UserMessage);
    }

    private static List<ValidationResult> Validate(ShareByEmailInputViewModel model)
    {
        var context = new ValidationContext(model);
        return model.Validate(context).ToList();
    }
}
