using System;
namespace Dfe.PlanTech.Domain.Users.Models
{
	public class User
	{
		public User()
		{
		}

        public int UserId { get; set; }
        public string DfeSigninRef { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime DateLastUpdated { get; set; }
    }
}

