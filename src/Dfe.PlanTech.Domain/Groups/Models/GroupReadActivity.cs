namespace Dfe.PlanTech.Domain.Groups.Models
{
    public class GroupReadActivity
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int UserEstablishmentId { get; set; }
        public int SelectedEstablishmentId { get; set; }
        public string SelectedEstablishmentName { get; set; } = string.Empty;
        public DateTime DateSelected { get; set; }
    }
}
