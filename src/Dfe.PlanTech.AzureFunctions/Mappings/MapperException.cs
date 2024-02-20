namespace Dfe.PlanTech.AzureFunctions.Mappings;

public class MapperException : Exception
{
    public MapperException(string message)
        : base(message)
    {
    }

}