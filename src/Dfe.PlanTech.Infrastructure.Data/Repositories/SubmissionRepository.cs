using Dfe.PlanTech.Infrastructure.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace Dfe.PlanTech.Infrastructure.Data.Repositories;

public class SubmissionRepository
{
    protected readonly PlanTechDbContext _db;

    public SubmissionRepository(PlanTechDbContext dbContext)
    {
        _db = dbContext;
    }

    public Task<SubmissionEntity?> GetSubmissionByIdAsync(int submissionId)
    {
        return _db.Submissions
            .Where(s => s.Id == submissionId)
            .Include(s => s.Responses)
                .ThenInclude(r => r.Question)
            .Include(s => s.Responses)
                .ThenInclude(r => r.Answer)
            .FirstOrDefaultAsync();
    }

    public Task DeleteCurrentSubmission(int establishmentId, int sectionId)
    {
        return _db.Database.ExecuteSqlAsync(
            $@"EXEC DeleteCurrentSubmission
            @establishmentId={establishmentId},
            @sectionId={sectionId}"
        );
    }
}
