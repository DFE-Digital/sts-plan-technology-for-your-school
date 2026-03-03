using System.ComponentModel.DataAnnotations;

namespace Dfe.PlanTech.Web.ViewModels.Inputs
{
    public class ShareRecommendationInputViewModel : IValidatableObject
    {
        [Required]
        public List<string> EmailAddresses { get; set; } = [];

        [Required]
        public required string NameOfUser { get; set; }

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

                if (!address.IsValid(email))
                {
                    yield return new ValidationResult(
                        "Enter an email address in the correct format, like name@example.com",
                        [$"{nameof(EmailAddresses)}[{i}]"]
                    );
                }
            }
        }
    }
}
