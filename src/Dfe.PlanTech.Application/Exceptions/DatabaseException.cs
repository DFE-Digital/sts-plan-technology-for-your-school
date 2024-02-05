namespace Dfe.PlanTech.Application.Exceptions;

public class DatabaseException : DbException
{
    public DatabaseException(string message) : base(message) { }
}