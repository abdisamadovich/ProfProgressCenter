using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ProfProgress.Domain.Entities;

namespace ProfProgress.Infrastructure.Persistence.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> b)
    {
        b.ToTable("users");
        b.HasKey(x => x.Id);
        b.Property(x => x.FullName).HasMaxLength(150).IsRequired();
        b.Property(x => x.Email).HasMaxLength(256).IsRequired();
        b.Property(x => x.Phone).HasMaxLength(20).IsRequired();
        b.Property(x => x.PasswordHash).IsRequired();
        b.Property(x => x.Role).HasConversion<string>().HasMaxLength(20);
        b.Property(x => x.RefreshToken).HasMaxLength(200);

        b.HasIndex(x => x.Email).IsUnique();
        b.HasIndex(x => x.RefreshToken);

        b.HasOne(x => x.Student)
            .WithOne(s => s.User)
            .HasForeignKey<Student>(s => s.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        b.HasOne(x => x.Teacher)
            .WithOne(t => t.User)
            .HasForeignKey<Teacher>(t => t.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class StudentConfiguration : IEntityTypeConfiguration<Student>
{
    public void Configure(EntityTypeBuilder<Student> b)
    {
        b.ToTable("students");
        b.HasKey(x => x.Id);
        b.Property(x => x.Address).HasMaxLength(300);
        b.Property(x => x.ParentPhone).HasMaxLength(20);
        b.HasIndex(x => x.UserId).IsUnique();
    }
}

public class TeacherConfiguration : IEntityTypeConfiguration<Teacher>
{
    public void Configure(EntityTypeBuilder<Teacher> b)
    {
        b.ToTable("teachers");
        b.HasKey(x => x.Id);
        b.Property(x => x.Specialization).HasMaxLength(200);
        b.Property(x => x.Bio).HasMaxLength(2000);
        b.HasIndex(x => x.UserId).IsUnique();
    }
}

public class CourseConfiguration : IEntityTypeConfiguration<Course>
{
    public void Configure(EntityTypeBuilder<Course> b)
    {
        b.ToTable("courses");
        b.HasKey(x => x.Id);
        b.Property(x => x.Title).HasMaxLength(200).IsRequired();
        b.Property(x => x.Description).HasMaxLength(4000);
        b.Property(x => x.Price).HasPrecision(18, 2);
        b.Property(x => x.Level).HasConversion<string>().HasMaxLength(20);
        b.HasIndex(x => x.Title);
    }
}

public class GroupConfiguration : IEntityTypeConfiguration<Group>
{
    public void Configure(EntityTypeBuilder<Group> b)
    {
        b.ToTable("groups");
        b.HasKey(x => x.Id);
        b.Property(x => x.Name).HasMaxLength(150).IsRequired();
        b.Property(x => x.RoomName).HasMaxLength(100);
        b.Property(x => x.Schedule).HasMaxLength(300);

        b.HasOne(x => x.Course)
            .WithMany(c => c.Groups)
            .HasForeignKey(x => x.CourseId)
            .OnDelete(DeleteBehavior.Restrict);

        b.HasOne(x => x.Teacher)
            .WithMany(t => t.Groups)
            .HasForeignKey(x => x.TeacherId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}

public class EnrollmentConfiguration : IEntityTypeConfiguration<Enrollment>
{
    public void Configure(EntityTypeBuilder<Enrollment> b)
    {
        b.ToTable("enrollments");
        b.HasKey(x => x.Id);
        b.Property(x => x.Status).HasConversion<string>().HasMaxLength(20);

        b.HasOne(x => x.Student)
            .WithMany(s => s.Enrollments)
            .HasForeignKey(x => x.StudentId)
            .OnDelete(DeleteBehavior.Cascade);

        b.HasOne(x => x.Group)
            .WithMany(g => g.Enrollments)
            .HasForeignKey(x => x.GroupId)
            .OnDelete(DeleteBehavior.Cascade);

        b.HasIndex(x => new { x.StudentId, x.GroupId });
    }
}

public class LessonConfiguration : IEntityTypeConfiguration<Lesson>
{
    public void Configure(EntityTypeBuilder<Lesson> b)
    {
        b.ToTable("lessons");
        b.HasKey(x => x.Id);
        b.Property(x => x.Topic).HasMaxLength(250).IsRequired();

        b.HasOne(x => x.Group)
            .WithMany(g => g.Lessons)
            .HasForeignKey(x => x.GroupId)
            .OnDelete(DeleteBehavior.Cascade);

        b.HasIndex(x => new { x.GroupId, x.Date });
    }
}

public class AttendanceConfiguration : IEntityTypeConfiguration<Attendance>
{
    public void Configure(EntityTypeBuilder<Attendance> b)
    {
        b.ToTable("attendances");
        b.HasKey(x => x.Id);
        b.Property(x => x.Status).HasConversion<string>().HasMaxLength(20);
        b.Property(x => x.Note).HasMaxLength(500);

        b.HasOne(x => x.Lesson)
            .WithMany(l => l.Attendances)
            .HasForeignKey(x => x.LessonId)
            .OnDelete(DeleteBehavior.Cascade);

        b.HasOne(x => x.Student)
            .WithMany(s => s.Attendances)
            .HasForeignKey(x => x.StudentId)
            .OnDelete(DeleteBehavior.Restrict);

        // Bitta o'quvchiga bitta darsda faqat bitta davomat yozuvi.
        b.HasIndex(x => new { x.LessonId, x.StudentId }).IsUnique();
    }
}

public class GradeConfiguration : IEntityTypeConfiguration<Grade>
{
    public void Configure(EntityTypeBuilder<Grade> b)
    {
        b.ToTable("grades");
        b.HasKey(x => x.Id);
        b.Property(x => x.Type).HasConversion<string>().HasMaxLength(20);
        b.Property(x => x.Comment).HasMaxLength(1000);

        b.HasOne(x => x.Student)
            .WithMany(s => s.Grades)
            .HasForeignKey(x => x.StudentId)
            .OnDelete(DeleteBehavior.Cascade);

        b.HasOne(x => x.Group)
            .WithMany()
            .HasForeignKey(x => x.GroupId)
            .OnDelete(DeleteBehavior.Restrict);

        b.HasOne(x => x.Lesson)
            .WithMany()
            .HasForeignKey(x => x.LessonId)
            .OnDelete(DeleteBehavior.SetNull);

        b.HasIndex(x => new { x.StudentId, x.GroupId });
    }
}