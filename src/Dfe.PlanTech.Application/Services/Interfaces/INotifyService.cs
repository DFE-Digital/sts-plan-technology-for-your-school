namespace Dfe.PlanTech.Application.Services.Interfaces
{
    public interface INotifyService
    {
        Task SendEmailAsync(
            string recommendationRef,
            ICollection<string> recipientEmailAddresses,
            string currentUserFullName,
            string activeSchoolName,
            string sectionName,
            string userMessage,
            string recommendationStatus
        );
    }
}
