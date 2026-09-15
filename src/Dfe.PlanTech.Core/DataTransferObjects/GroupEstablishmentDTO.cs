namespace Dfe.PlanTech.Core.DataTransferObjects
{
    public class GroupEstablishmentDTO
    {
        public int GroupUID { get; set; }

        public string? GroupID  { get; set; } //dbo.est id? null currently in col

        public string Name { get; set; } = string.Empty;

        public List<EstablishmentBasicDto> BasicEstablishments = new List<EstablishmentBasicDto>();

    }
}
