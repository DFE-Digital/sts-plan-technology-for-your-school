using System.Data.Common;

namespace Dfe.PlanTech.Core.Exceptions;

public class DatabaseException : DbException
{
    public DatabaseException(string message)
        : base(message) { }
}
