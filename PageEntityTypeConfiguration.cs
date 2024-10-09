using Microsoft.EntityFrameworkCore;
using YourNamespace.Models;

namespace YourNamespace.Configurations
{
    public class PageEntityTypeConfiguration : IEntityTypeConfiguration<PageDbEntity>
    {
        public void Configure(EntityTypeBuilder<PageDbEntity> builder)
        {
            builder.ToTable("Pages", Schema);
            builder.HasOne(page => page.BeforeTitleContent).WithMany().HasForeignKey(page => page.BeforeTitleContentId);
            builder.HasOne(page => page.Content).WithMany().HasForeignKey(page => page.ContentId);

            // Other configurations for PageDbEntity
        }
    }
}
