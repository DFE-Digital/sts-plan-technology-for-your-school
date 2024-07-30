using Dfe.PlanTech.Domain.Users.Models;

namespace Dfe.PlanTech.Domain.SignIns.Models
{
    public class SignIn
    {
        public int Id { get; set; }

        public int UserId { get; set; }

        public int EstablishmentId { get; set; }

        public DateTime SignInDateTime { get; set; }

        public User User { get; set; } = default!;
    }
}
