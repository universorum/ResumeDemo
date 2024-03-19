using Microsoft.EntityFrameworkCore;
using ResumeDemo.Data;
using ResumeDemo.Extensions;
using ResumeDemo.Models;

namespace ResumeDemo.Server.Services;

public class AppDbContext(DbContextOptions options) : DbContext(options), IAppDbContext
{
    public required DbSet<Resume>     Resumes     { get; init; }
    public required DbSet<Experience> Experiences { get; init; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ConfigureResumeDemo();
    }
}