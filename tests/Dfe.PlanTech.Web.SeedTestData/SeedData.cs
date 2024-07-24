using Dfe.PlanTech.Domain.Content.Enums;
using Dfe.PlanTech.Domain.Content.Models;
using Dfe.PlanTech.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Dfe.PlanTech.Web.SeedTestData;

public class SeedData
{
    private static CmsDbContext _db;

    public SeedData(CmsDbContext dbContext)
    {
        _db = dbContext;
    }

    public void CreateData()
    {
        _db.Pages.Add(new PageDbEntity()
        {
            Id = "self-assessment-id",
            InternalName = "self-assessment-internal-name",
            Slug = "self-assessment",
            Content = [
                new HeaderDbEntity()
                {
                    Id = "self-assessment-header-id",
                    Text = "Self Assessment",
                    Tag = HeaderTag.H1,
                    Size = HeaderSize.Large
                }
            ]
        });

        _db.SaveChanges();
    }
}
