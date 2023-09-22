namespace Dfe.PlanTech.Domain.Establishments.Models;

public class EstablishmentDto
{
    public string? Ukprn { get; set; }

    public string? Urn { get; set; }

    public EstablishmentTypeDto Type { get; set; } = new EstablishmentTypeDto();

    public string OrgName { get; set; } = null!;

    public bool IsValid => References().Any(reference => !string.IsNullOrEmpty(reference));

    public string Reference => References().FirstOrDefault(reference => !string.IsNullOrEmpty(reference)) ??
                                throw new Exception($"Both {nameof(Urn)} and {nameof(Ukprn)} are invalid");

    private IEnumerable<string?> References()
    {
        yield return Urn;
        yield return Ukprn;
    }
}