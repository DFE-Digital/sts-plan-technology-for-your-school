namespace Dfe.PlanTech.Domain.Users.Exceptions;

public class UserAccessRoleNotFoundException : Exception
{
    public UserAccessRoleNotFoundException(string message)
        : base(message)
    {
    }

}
