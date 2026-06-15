using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ProfProgress.Domain.Entities;

namespace ProfProgress.Infrastructure.Persistence.Configurations;

public class TestConfiguration : IEntityTypeConfiguration<Test>
{
    public void Configure(EntityTypeBuilder<Test> b)
    {
        b.ToTable("tests");
        b.HasKey(x => x.Id);
        b.Property(x => x.Title).HasMaxLength(250).IsRequired();
        b.Property(x => x.Description).HasMaxLength(2000);
        b.Property(x => x.TelegramFileId).HasMaxLength(300);
        b.Property(x => x.FileName).HasMaxLength(300);
        b.Property(x => x.AnswerKey).HasMaxLength(500).IsRequired();
        b.Property(x => x.Status).HasConversion<string>().HasMaxLength(20);
        b.Ignore(x => x.EndsAt);

        b.HasOne(x => x.Group)
            .WithMany()
            .HasForeignKey(x => x.GroupId)
            .OnDelete(DeleteBehavior.SetNull);

        b.HasOne(x => x.CreatedBy)
            .WithMany()
            .HasForeignKey(x => x.CreatedByUserId)
            .OnDelete(DeleteBehavior.Restrict);

        b.HasIndex(x => new { x.Status, x.StartsAt });
    }
}

public class TestBlockConfiguration : IEntityTypeConfiguration<TestBlock>
{
    public void Configure(EntityTypeBuilder<TestBlock> b)
    {
        b.ToTable("test_blocks");
        b.HasKey(x => x.Id);
        b.Property(x => x.Name).HasMaxLength(100);
        b.Property(x => x.PointsPerQuestion).HasPrecision(6, 2);

        b.HasOne(x => x.Test)
            .WithMany(t => t.Blocks)
            .HasForeignKey(x => x.TestId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class TestAttemptConfiguration : IEntityTypeConfiguration<TestAttempt>
{
    public void Configure(EntityTypeBuilder<TestAttempt> b)
    {
        b.ToTable("test_attempts");
        b.HasKey(x => x.Id);
        b.Property(x => x.Answers).HasMaxLength(500);
        b.Property(x => x.TotalScore).HasPrecision(8, 2);
        b.Property(x => x.Status).HasConversion<string>().HasMaxLength(20);

        b.HasOne(x => x.Test)
            .WithMany(t => t.Attempts)
            .HasForeignKey(x => x.TestId)
            .OnDelete(DeleteBehavior.Cascade);

        b.HasOne(x => x.Student)
            .WithMany()
            .HasForeignKey(x => x.StudentId)
            .OnDelete(DeleteBehavior.Cascade);

        // Bitta talaba bitta testni faqat bir marta topshiradi.
        b.HasIndex(x => new { x.TestId, x.StudentId }).IsUnique();
    }
}
