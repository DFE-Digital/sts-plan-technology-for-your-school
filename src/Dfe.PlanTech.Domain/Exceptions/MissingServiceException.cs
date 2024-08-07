namespace Dfe.PlanTech.Domain.Exceptions;

public class MissingServiceException(string message) : Exception(message)
{
    public MissingServiceException(Type missingService) : this($"Missing service {missingService.Name}") { }
}
