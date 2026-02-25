namespace Dfe.PlanTech.Application.Services.Interfaces
{
    public interface INotifyService
    {
        Task SendEmailAsync(
            string recommendationRef,
            ICollection<string> recipients,
            string subject,
            string body
        );
    }
}
