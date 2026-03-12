using Dfe.PlanTech.Core.Models;

namespace Dfe.PlanTech.Application.Workflows.Interfaces;

public interface INotifyWorkflow
{
    List<NotifySendResult> SendEmails(
        ShareByEmailModel model,
        Dictionary<string, object> personalisation,
        string correlationId,
        string templateId
    );
}
