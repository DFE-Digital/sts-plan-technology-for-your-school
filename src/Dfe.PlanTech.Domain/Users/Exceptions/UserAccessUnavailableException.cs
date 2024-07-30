namespace Dfe.PlanTech.Domain.Users.Exceptions;

public class UserAccessUnavailableException : Exception
{
    public UserAccessUnavailableException(string message)
        : base(message)
    {
    }

}
