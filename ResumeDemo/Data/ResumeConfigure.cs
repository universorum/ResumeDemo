using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ResumeDemo.Models;

namespace ResumeDemo.Data;

public class ResumeConfigure : IEntityTypeConfiguration<Resume>
{
    public void Configure(EntityTypeBuilder<Resume> builder)
    {
        builder.HasKey(e => e.Id);

        builder.Property(e => e.Id).ValueGeneratedOnAdd();
        builder.Property(e => e.Name).HasMaxLength(256);

        builder.HasIndex(e => e.Name);
        builder.HasIndex(e => e.BirthDate);
        builder.HasIndex(e => new { e.Name, e.BirthDate });
    }
}