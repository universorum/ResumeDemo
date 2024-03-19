using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ResumeDemo.Data;
using ResumeDemo.Extensions;
using ResumeDemo.Models;

namespace ResumeDemo.Services;

public partial class ResumeManager(ILogger<ResumeManager> logger, IAppDbContext context)
{
    private Task<bool> ExistsAsync(int id, CancellationToken ct = default)
    {
        return context.Resumes.AnyAsync(e => e.Id == id, ct);
    }

    private Task<bool> ExistsAsync(int id, Guid version, CancellationToken ct = default)
    {
        return context.Resumes.AnyAsync(e => e.Id == id && e.Version == version, ct);
    }

    public IAsyncEnumerable<Resume> GetAsync(
        string?   name           = null,
        DateOnly? birthDateStart = null,
        DateOnly? birthDateEnd   = null,
        string?   title          = null,
        bool      tracking       = false)
    {
        return context.Resumes.Where(x =>
                (name == null             || x.Name.Contains(name))               &&
                (!birthDateStart.HasValue || x.BirthDate >= birthDateStart.Value) &&
                (!birthDateEnd.HasValue   || x.BirthDate <= birthDateEnd.Value)   &&
                (title                                   == null || x.Experiences.Any(y => y.Title == title)))
            .AsTracking(tracking)
            .AsAsyncEnumerable();
    }

    public async Task<Pagination<Resume>> GetPaginationAsync(
        int               size           = 10,
        string?           name           = null,
        DateOnly?         birthDateStart = null,
        DateOnly?         birthDateEnd   = null,
        string?           title          = null,
        bool              tracking       = false,
        int?              before         = default,
        int?              after          = default,
        CancellationToken ct             = default)
    {
        if (before.HasValue && after.HasValue)
        {
            throw new ArgumentException("Only one of before and after can be specified");
        }

        Expression<Func<Resume, bool>> expression = x =>
            (!before.HasValue || x.Id > before) &&
            (!after.HasValue  || x.Id < after) &&
            (name                     == null || x.Name.Contains(name)) &&
            (!birthDateStart.HasValue         || x.BirthDate >= birthDateStart.Value) &&
            (!birthDateEnd.HasValue           || x.BirthDate <= birthDateEnd.Value) &&
            (title == null || x.Experiences.Any(y => y.Title.Contains(title)));

        var total    = await context.Resumes.Where(expression).Take(size + 1).CountAsync(ct);
        var haveNext = total > size;

        var result = before.HasValue
            ? context.Resumes.Where(expression)
                .OrderBy(x => x.Id)
                .Take(size)
                .OrderByDescending(x => x.Id)
                .AsTracking(tracking)
                .AsAsyncEnumerable()
            : context.Resumes.Where(expression)
                .OrderByDescending(x => x.Id)
                .Take(size)
                .AsTracking(tracking)
                .AsAsyncEnumerable();

        return new Pagination<Resume>(result, haveNext);
    }

    public Task<Resume?> GetByIdAsync(int id, bool tracking = false, CancellationToken ct = default)
    {
        return context.Resumes.Include(x => x.Experiences)
            .AsTracking(tracking)
            .FirstOrDefaultAsync(e => e.Id == id, ct);
    }

    public Task AddAsync(Resume resume, CancellationToken ct = default)
    {
        context.Resumes.Add(resume);
        resume.Version = Guid.NewGuid();
        foreach (var resumeExperience in resume.Experiences) { resumeExperience.Version = Guid.NewGuid(); }

        return context.SaveChangesAsync(ct);
    }

    public async Task UpdateAsync(Resume resume, CancellationToken ct = default)
    {
        await using var transaction = await context.Database.BeginTransactionAsync(ct).ConfigureAwait(false);

        var exists = await ExistsAsync(resume.Id, resume.Version, ct).ConfigureAwait(false);

        if (!exists)
        {
            if (await ExistsAsync(resume.Id, ct).ConfigureAwait(false))
            {
                LogResumeConcurrencyConflict(logger, resume.Id);
                throw new DbUpdateConcurrencyException();
            }

            LogResumeNotFound(logger, resume.Id);
            throw new DbUpdateException($"Resume {resume.Id} not found");
        }

        context.Resumes.Update(resume);
        var resumeEntry = context.Resumes.Entry(resume);
        resumeEntry.Property(x => x.Version).IsModified = false;
        resume.Version                                  = Guid.NewGuid();

        foreach (var experience in resume.Experiences.ToList())
        {
            var experienceEntry = context.Experiences.Entry(experience);
            experienceEntry.Property(e => e.ResumeId).IsModified = false;

            if (experience.Version == Guid.Empty) { continue; }

            experienceEntry.Property(x => x.Version).IsModified = false;
            experience.Version                                  = Guid.NewGuid();
        }

        await context.SaveChangesAsync(ct).ConfigureAwait(false);
        await transaction.CommitAsync(ct).ConfigureAwait(false);
    }

    public async Task UpdateResumeByIdAsync(int id, string name, Guid version, CancellationToken ct = default)
    {
        var newVersion = Guid.NewGuid();

        var count = await context.Resumes.Where(x => x.Id == id && x.Version == version)
            .ExecuteUpdateAsync(setter => setter.SetProperty(e => e.Name, name).SetProperty(e => e.Version, newVersion),
                ct)
            .ConfigureAwait(false);

        if (count != 0) { return; }

        if (await ExistsAsync(id, ct))
        {
            LogResumeConcurrencyConflict(logger, id, version);
            throw new DbUpdateConcurrencyException($"Resume {id} has been updated concurrently");
        }

        LogResumeNotFound(logger, id);
        throw new DbUpdateException($"Resume {id} not found");
    }

    public async Task<bool> UpdateResumeByIdAsync(
        int                     id,
        IEnumerable<Experience> experience,
        Guid                    version,
        CancellationToken       ct = default)
    {
        await using var transaction = await context.Database.BeginTransactionAsync(ct).ConfigureAwait(false);

        var resume = await context.Resumes.Include(x => x.Experiences)
            .FirstOrDefaultAsync(e => e.Id == id, ct)
            .ConfigureAwait(false);

        if (resume == null)
        {
            LogResumeNotFound(logger, id);
            return false;
        }

        if (resume.Version != version)
        {
            LogResumeConcurrencyConflict(logger, id, resume.Version);
            throw new DbUpdateConcurrencyException($"Resume {id} has been updated concurrently");
        }

        resume.Experiences.Clear();
        foreach (var detail in experience) { resume.Experiences.Add(detail); }

        await context.SaveChangesAsync(ct).ConfigureAwait(false);
        await transaction.CommitAsync(ct).ConfigureAwait(false);

        return false;
    }

    public async Task DeleteByIdAsync(int id, Guid version, CancellationToken ct = default)
    {
        var count = await context.Resumes.Where(x => x.Id == id && x.Version == version)
            .ExecuteDeleteAsync(ct)
            .ConfigureAwait(false);

        if (count != 0) { return; }

        if (await ExistsAsync(id, ct))
        {
            LogResumeConcurrencyConflict(logger, id, version);
            throw new DbUpdateConcurrencyException($"Resume {id} has been updated concurrently");
        }

        LogResumeNotFound(logger, id);
        throw new DbUpdateException($"Resume {id} not found");
    }

    public async Task DeleteExperienceByIdAsync(
        int               resumeId,
        int               experienceId,
        Guid              resumeVersion,
        CancellationToken ct = default)
    {
        var             newVersion  = Guid.NewGuid();
        await using var transaction = await context.Database.BeginTransactionAsync(ct).ConfigureAwait(false);

        var result = await context.Resumes.Where(x => x.Id == resumeId && x.Version == resumeVersion)
            .ExecuteUpdateAsync(setter => setter.SetProperty(e => e.Version, newVersion), ct)
            .ConfigureAwait(false);

        if (result == 0)
        {
            if (await ExistsAsync(resumeId, ct))
            {
                LogResumeConcurrencyConflict(logger, resumeId);
                throw new DbUpdateConcurrencyException($"Resume {resumeId} has been updated concurrently");
            }

            LogResumeGone(logger, resumeId, resumeVersion);
            throw new DbUpdateException($"Resume {resumeId} with version {resumeVersion} is gone");
        }

        result = await context.Experiences.Where(x => x.Id == experienceId)
            .ExecuteDeleteAsync(ct)
            .ConfigureAwait(false);
        if (result == 0)
        {
            LogExperienceNotFound(logger, experienceId);
            throw new DbUpdateException($"Experience not found: {experienceId}");
        }

        await transaction.CommitAsync(ct).ConfigureAwait(false);
    }

    [LoggerMessage(LogLevel.Debug, "Resume not found: {Id}")]
    private static partial void LogResumeNotFound(ILogger logger, int id);

    [LoggerMessage(LogLevel.Debug, "Experience not found: {Id}")]
    private static partial void LogExperienceNotFound(ILogger logger, int id);

    [LoggerMessage(LogLevel.Debug, "Resume concurrency conflicts: {Id}, current version: {version}")]
    private static partial void LogResumeConcurrencyConflict(ILogger logger, int id, Guid version);

    [LoggerMessage(LogLevel.Debug, "Resume concurrency conflicts: {Id}, current version: unknown")]
    private static partial void LogResumeConcurrencyConflict(ILogger logger, int id);

    [LoggerMessage(LogLevel.Warning, "Resume is gone: {id}, version: {version}")]
    private static partial void LogResumeGone(ILogger logger, int id, Guid version);
}