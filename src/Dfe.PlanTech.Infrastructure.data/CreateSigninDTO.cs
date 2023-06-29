using System;
namespace Dfe.PlanTech.Infrastructure.data
{
	public class CreateSigninDTO
	{
        public int UserId { get; set; }
        public int EstablishmentId { get; set; }
        public string URN { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime DateLastUpdated { get; set; }
    }
}

