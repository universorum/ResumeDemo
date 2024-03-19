using Microsoft.EntityFrameworkCore;
using ResumeDemo.Data;
using ResumeDemo.Models;

namespace ResumeDemo.Extensions;

public static class ModelBuilderExtension
{
    public static void ConfigureResumeDemo(this ModelBuilder modelBuilder)
    {
        new ResumeConfigure().Configure(modelBuilder.Entity<Resume>());
        new ExperienceConfigure().Configure(modelBuilder.Entity<Experience>());
    }
}