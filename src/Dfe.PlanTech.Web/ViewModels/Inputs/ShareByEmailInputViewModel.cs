using System.ComponentModel.DataAnnotations;
using Dfe.PlanTech.Core.Models;

namespace Dfe.PlanTech.Web.ViewModels.Inputs;

public class ShareByEmailInputViewModel : IValidatableObject
{
    public string? NameOfUser { get; set; } = string.Empty;

    public List<string> EmailAddresses { get; set; } = [];

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
        }
        else
        {
            CleanEmails();

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
                        "Enter a valid email address",
                        [$"EmailAddresses[{i}]"]
                    );
                }
            }
        }

        if (string.IsNullOrWhiteSpace(NameOfUser))
        {
            yield return new ValidationResult("Enter your name", [nameof(NameOfUser)]);
        }
    }

    public ShareByEmailModel ToModel()
    {
        return new ShareByEmailModel
        {
            NameOfUser = NameOfUser ?? string.Empty,
            EmailAddresses = EmailAddresses,
            UserMessage = UserMessage,
        };
    }

    private void CleanEmails()
    {
        EmailAddresses = EmailAddresses
            .Where(address => !string.IsNullOrWhiteSpace(address))
            .Select(address => address.Trim())
            .Distinct()
            .ToList();
    }
}
