namespace Dfe.PlanTech.Domain.Users.Exceptions;

public class PageNotFoundException : Exception
{
    public PageNotFoundException(string message)
        : base(message)
    {
    }
}
