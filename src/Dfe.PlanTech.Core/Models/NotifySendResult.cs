using Notify.Models.Responses;

namespace Dfe.PlanTech.Core.Models
{
    public class NotifySendResult
    {
        public required string Recipient { get; set; }
        public EmailNotificationResponse? Response { get; set; }
        public List<string> Errors { get; set; } = [];
        public string ErrorList => string.Join("; ", Errors);
    }
}
