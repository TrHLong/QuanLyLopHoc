using Microsoft.EntityFrameworkCore;
using ClassManagement.Domain.Entities;

namespace ClassManagement.Infrastructure.Data
{
    public class ClassManagementDbContext : DbContext
    {
        public ClassManagementDbContext(DbContextOptions<ClassManagementDbContext> options) : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Course> Courses { get; set; }
        public DbSet<CourseRegistration> CourseRegistrations { get; set; }
        public DbSet<Attendance> Attendances { get; set; }
        public DbSet<Assignment> Assignments { get; set; }
        public DbSet<AssignmentSubmission> AssignmentSubmissions { get; set; }
        public DbSet<FinalGrade> FinalGrades { get; set; }
        public DbSet<Notification> Notifications { get; set; }
        public DbSet<PasswordResetToken> PasswordResetTokens { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // User configuration
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.Email).IsUnique();
                entity.Property(e => e.Email).HasMaxLength(255);
                entity.Property(e => e.FullName).HasMaxLength(255);
                entity.Property(e => e.PasswordHash).HasMaxLength(255);
            });

            // Course configuration
            modelBuilder.Entity<Course>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => new { e.Grade, e.TimeSlot, e.StartDate }).IsUnique();
            });

            // CourseRegistration configuration
            modelBuilder.Entity<CourseRegistration>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => new { e.StudentId, e.CourseId }).IsUnique();
                
                entity.HasOne(e => e.Student)
                    .WithMany(s => s.CourseRegistrations)
                    .HasForeignKey(e => e.StudentId)
                    .OnDelete(DeleteBehavior.Cascade);
                
                entity.HasOne(e => e.Course)
                    .WithMany(c => c.CourseRegistrations)
                    .HasForeignKey(e => e.CourseId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Attendance configuration
            modelBuilder.Entity<Attendance>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => new { e.StudentId, e.CourseId, e.Date, e.TimeSlot }).IsUnique();
                
                entity.HasOne(e => e.Student)
                    .WithMany(s => s.Attendances)
                    .HasForeignKey(e => e.StudentId)
                    .OnDelete(DeleteBehavior.Cascade);
                
                entity.HasOne(e => e.Course)
                    .WithMany(c => c.Attendances)
                    .HasForeignKey(e => e.CourseId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Assignment configuration
            modelBuilder.Entity<Assignment>(entity =>
            {
                entity.HasKey(e => e.Id);
                
                entity.HasOne(e => e.Course)
                    .WithMany(c => c.Assignments)
                    .HasForeignKey(e => e.CourseId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // AssignmentSubmission configuration
            modelBuilder.Entity<AssignmentSubmission>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => new { e.AssignmentId, e.StudentId }).IsUnique();
                
                entity.HasOne(e => e.Assignment)
                    .WithMany(a => a.Submissions)
                    .HasForeignKey(e => e.AssignmentId)
                    .OnDelete(DeleteBehavior.Cascade);
                
                entity.HasOne(e => e.Student)
                    .WithMany(s => s.AssignmentSubmissions)
                    .HasForeignKey(e => e.StudentId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // FinalGrade configuration
            modelBuilder.Entity<FinalGrade>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => new { e.StudentId, e.CourseId }).IsUnique();
                
                entity.HasOne(e => e.Student)
                    .WithMany(s => s.FinalGrades)
                    .HasForeignKey(e => e.StudentId)
                    .OnDelete(DeleteBehavior.Cascade);
                
                entity.HasOne(e => e.Course)
                    .WithMany(c => c.FinalGrades)
                    .HasForeignKey(e => e.CourseId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Notification configuration
            modelBuilder.Entity<Notification>(entity =>
            {
                entity.HasKey(e => e.Id);
                
                entity.HasOne(e => e.User)
                    .WithMany()
                    .HasForeignKey(e => e.UserId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // PasswordResetToken configuration
            modelBuilder.Entity<PasswordResetToken>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.Token).IsUnique();
                
                entity.HasOne(e => e.User)
                    .WithMany()
                    .HasForeignKey(e => e.UserId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Seed data for teacher account
            modelBuilder.Entity<User>().HasData(
                new User
                {
                    Id = 1,
                    Email = "teacher@example.com",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("123456"), // Default password
                    FullName = "Giáo viên Toán",
                    Role = Domain.Enums.UserRole.Teacher,
                    CreatedAt = DateTime.UtcNow,
                    IsActive = true
                }
            );
        }
    }
}




