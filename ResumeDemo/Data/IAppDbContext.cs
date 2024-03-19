using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using ResumeDemo.Models;

namespace ResumeDemo.Data;

public interface IAppDbContext
{
    DbSet<Resume>     Resumes     { get; }
    DbSet<Experience> Experiences { get; }

    DatabaseFacade Database      { get; }
    IModel         Model         { get; }
    ChangeTracker  ChangeTracker { get; }

    EntityEntry<TEntity> Add<TEntity>(TEntity    entity) where TEntity : class;
    EntityEntry<TEntity> Attach<TEntity>(TEntity entity) where TEntity : class;
    EntityEntry<TEntity> Update<TEntity>(TEntity entity) where TEntity : class;

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default);
}