namespace Dfe.PlanTech.Domain.Users.Models
{
	public class User
	{
        public int Id { get; set; }

        public string DfeSignInRef { get; set; } = null!;

        public DateTime DateCreated { get; set; } = DateTime.UtcNow;

        public DateTime? DateLastUpdated { get; set; }
    }
}

