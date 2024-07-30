namespace Dfe.PlanTech.Domain.Establishments.Exceptions;

public class InvalidEstablishmentException : Exception
{
    public InvalidEstablishmentException(string message)
        : base(message)
    {
    }

}
