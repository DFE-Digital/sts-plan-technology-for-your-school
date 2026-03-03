using System.ComponentModel.DataAnnotations;

namespace Dfe.PlanTech.Web.ViewModels.Inputs;

public class ShareRecommendationInputViewModel : IValidatableObject
{
    public List<string> EmailAddresses { get; set; } = [];

    public string? NameOfUser { get; set; } = string.Empty;

    public string? UserMessage { get; set; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        var hasAtLeastOneEmail = EmailAddresses.Any(e => !string.IsNullOrWhiteSpace(e));
        if (!hasAtLeastOneEmail)
        {
            yield return new ValidationResult(
                "Please enter at least one email address",
                [nameof(EmailAddresses)]
            );

            yield break;
        }

        var address = new EmailAddressAttribute();

        for (var i = 0; i < EmailAddresses.Count; i++)
        {
            var email = EmailAddresses[i];

            if (string.IsNullOrWhiteSpace(email))
                continue;

            var at = email.LastIndexOf('@');
            var domain = at >= 0 ? email[(at + 1)..] : "";

            if (!address.IsValid(email) || !domain.Contains('.'))
            {
                yield return new ValidationResult(
                    "Enter an email address in the correct format, like name@example.com",
                    [$"EmailAddresses[{i}]"]
                );
            }
        }

        if (string.IsNullOrWhiteSpace(NameOfUser))
        {
            yield return new ValidationResult("Please enter your name", [nameof(NameOfUser)]);

            yield break;
        }
    }
}
