using System.Runtime.Serialization;

namespace Dfe.PlanTech.Infrastructure.Contentful.Helpers
{
    [Serializable]
    public class GetEntitiesException : Exception
    {
        public GetEntitiesException(string? message) : base(message)
        {
        }
    }
}