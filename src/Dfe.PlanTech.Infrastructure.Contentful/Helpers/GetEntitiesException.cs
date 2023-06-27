using System.Runtime.Serialization;

namespace Dfe.PlanTech.Infrastructure.Contentful.Helpers
{
    public class GetEntitiesException : Exception
    {
        public GetEntitiesException(string? message) : base(message)
        {
        }
    }
}