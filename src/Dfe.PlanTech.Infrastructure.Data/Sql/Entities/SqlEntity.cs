using Dfe.PlanTech.Core.DataTransferObjects.Sql;

namespace Dfe.PlanTech.Infrastructure.Data.Sql.Entities
{
    public abstract class SqlEntity<TDto>
        where TDto : SqlDto, new()
    {
        public TDto ToDto()
        {
            return CreateDto();
        }

        protected abstract TDto CreateDto();
    }
}
