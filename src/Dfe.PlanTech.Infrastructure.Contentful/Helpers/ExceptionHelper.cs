using System.Runtime.Serialization;

namespace Dfe.PlanTech.Infrastructure.Contentful.Helpers
{
    [Serializable]
    internal class ExceptionHelper : Exception
    {
        public ExceptionHelper(string? message) : base(message)
        {
        }
    }
}