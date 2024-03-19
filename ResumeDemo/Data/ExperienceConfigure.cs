using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ResumeDemo.Models;

namespace ResumeDemo.Data;

public class ExperienceConfigure : IEntityTypeConfiguration<Experience>
{
    public void Configure(EntityTypeBuilder<Experience> builder)
    {
        builder.HasKey(e => e.Id);

        builder.Property(e => e.Id).ValueGeneratedOnAdd();
        builder.Property(e => e.Company).HasMaxLength(256);
        builder.Property(e => e.Title).HasMaxLength(32);

        builder.HasIndex(e => e.Title);

        builder.HasOne(e => e.Resume).WithMany(e => e.Experiences).HasForeignKey(e => e.ResumeId);
    }
}