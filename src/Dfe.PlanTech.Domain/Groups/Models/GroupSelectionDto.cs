namespace Dfe.PlanTech.Domain.Groups.Interfaces
{
    public class GroupSelectionDto
    {
        public int UserId { get; set; }

        public int UserEstablishment { get; set; }

        public int SelectedEstablishmentId { get; set; }

        public string? SelectedEstablishmentName { get; set; }
    }
}
