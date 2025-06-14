﻿using System.Linq.Expressions;
using Dfe.PlanTech.Domain.Establishments.Models;
using Dfe.PlanTech.Domain.Groups.Models;
using Dfe.PlanTech.Domain.SignIns.Models;
using Dfe.PlanTech.Domain.Submissions.Models;
using Dfe.PlanTech.Domain.Users.Models;
using Microsoft.EntityFrameworkCore;

namespace Dfe.PlanTech.Application.Persistence.Interfaces;

/// <summary>
/// Interface for the DbContext that handles user related tables (establishments, users, submissions, responses, etc.)
/// </summary>
public interface IPlanTechDbContext
{
    // User Table & SignIn Table
    public void AddUser(User user);
    public void AddSignIn(SignIn signIn);

    // Submission Table
    public IQueryable<Submission> GetSubmissions { get; }

    public IQueryable<ResponseAnswer> GetAnswers { get; }

    public void AddEstablishment(Establishment establishment);

    public void AddSubmission(Submission submission);

    public DbSet<Submission> Submissions { get; }

    Task<Submission?> GetSubmissionById(int submissionId, CancellationToken cancellationToken = default);

    public Task<int> SaveChangesAsync();

    Task<int> CallStoredProcedureWithReturnInt(string sprocName, IEnumerable<object> parameters, CancellationToken cancellationToken = default);

    IQueryable<SectionStatusDto> GetSectionStatuses(string sectionIds, int establishmentId);

    Task<User?> GetUserBy(Expression<Func<User, bool>> predicate);

    Task<Establishment?> GetEstablishmentBy(Expression<Func<Establishment, bool>> predicate);

    Task<List<EstablishmentLink>> GetGroupEstablishmentsBy(Expression<Func<Establishment, bool>> predicate);

    public IQueryable<GroupReadActivity> GetGroupReadActivities { get; }

    Task<List<T>> ToListAsync<T>(IQueryable<T> queryable, CancellationToken cancellationToken = default);

    Task<T?> FirstOrDefaultAsync<T>(IQueryable<T> queryable, CancellationToken cancellationToken = default);

    Task<int> ExecuteSqlAsync(FormattableString sql, CancellationToken cancellationToken = default);
}
