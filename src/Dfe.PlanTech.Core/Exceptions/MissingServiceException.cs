using System.Diagnostics.CodeAnalysis;

namespace Dfe.PlanTech.Core.Exceptions;

[ExcludeFromCodeCoverage]
public class MissingServiceException(string message) : Exception(message)
{
    public MissingServiceException(Type missingService)
        : this($"Missing service {missingService.Name}") { }
}
