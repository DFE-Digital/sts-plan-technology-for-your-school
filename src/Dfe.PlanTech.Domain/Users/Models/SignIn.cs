using System;
namespace Dfe.PlanTech.Domain.Users.Models
{
	public class SignIn
	{
		public SignIn()
		{
		}

        public int UserId { get; set; }
        public int EstablishmentId { get; set; }
        public string URN { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime DateLastUpdated { get; set; }
    }
}

