namespace Dfe.PlanTech.Domain.Responses.Models;

    public class Response
    {
        public int Id { get; set; }

        public int UserId { get; set; }

        public int SubmissionId { get; set; }

        public int QuestionId { get; set; }

        public int AnswerId { get; set; }

        public string Maturity { get; set; } = null!;

        public DateTime? DateCreated { get; set; }

        public DateTime? DateLastUpdated { get; set; }
    }