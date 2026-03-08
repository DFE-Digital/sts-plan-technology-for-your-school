using System.Text.Json;
using System.Text.RegularExpressions;
using Dfe.PlanTech.Application.Workflows.Interfaces;
using Dfe.PlanTech.Core.Constants;
using Dfe.PlanTech.Core.Models;
using Notify.Exceptions;
using Notify.Interfaces;

namespace Dfe.PlanTech.Application.Workflows;

public class NotifyWorkflow(INotificationClient notifyClient) : INotifyWorkflow
{
    private readonly INotificationClient _notifyClient =
        notifyClient ?? throw new ArgumentNullException(nameof(notifyClient));

    private const string CouldNotSendEmail = "Could not send email";

    public List<NotifySendResult> SendEmails(
        ShareByEmailModel model,
        Dictionary<string, dynamic> personalisation,
        string correlationId,
        string templateId
    )
    {
        var results = new List<NotifySendResult>();

        foreach (var recipient in model.EmailAddresses)
        {
            var errorPrefix = $"{CouldNotSendEmail} to {recipient}";

            try
            {
                var response = _notifyClient.SendEmail(
                    recipient,
                    templateId,
                    personalisation,
                    correlationId
                );

                var result = new NotifySendResult { Recipient = recipient, Response = response };

                results.Add(result);
            }
            catch (NotifyClientException clientException)
            {
                var errors = ParseNotifyClientException(clientException.Message);
                var result = new NotifySendResult { Recipient = recipient, Errors = errors };

                results.Add(result);
            }
            catch (Exception)
            {
                var result = new NotifySendResult
                {
                    Recipient = recipient,
                    Errors = ["GOK.UK Notify failed"],
                };

                results.Add(result);
            }
        }

        return results;
    }

    private List<string> ParseNotifyClientException(string message)
    {
        // GOV.UK Notify's provided exception class is weird.
        // It returns a string that combines regular text with JSON, and
        // puts a very similar message twice. This attempts to untangle it.

        var firstPart = message.Split("\n").Select(x => x.Trim()).FirstOrDefault();
        if (firstPart is null)
        {
            return [];
        }

        var matches = Regex.Match(
            firstPart,
            "Status code \\d+. Error: ({.*}).*",
            RegexOptions.Compiled,
            TimeSpan.FromMilliseconds(200)
        );
        var messageJson = matches.Groups.Values.LastOrDefault()?.ToString();
        if (string.IsNullOrWhiteSpace(messageJson))
        {
            return [];
        }

        var result = JsonSerializer.Deserialize<NotifyClientExceptionMessage>(messageJson);
        return result?.Errors.Select(e => e.Message).ToList() ?? [];
    }
}
