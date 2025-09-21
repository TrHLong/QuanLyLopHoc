using System.ComponentModel.DataAnnotations;
using ClassManagement.Domain.Enums;

namespace ClassManagement.Domain.Entities
{
    public class User
    {
        public int Id { get; set; }
        
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;
        
        [Required]
        public string PasswordHash { get; set; } = string.Empty;
        
        [Required]
        public string FullName { get; set; } = string.Empty;
        
        public UserRole Role { get; set; }
        
        public Grade? Grade { get; set; } // Chỉ áp dụng cho học sinh
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        public DateTime? LastLoginAt { get; set; }
        
        public bool IsActive { get; set; } = true;
        
        // Navigation properties
        public virtual ICollection<CourseRegistration> CourseRegistrations { get; set; } = new List<CourseRegistration>();
        public virtual ICollection<Attendance> Attendances { get; set; } = new List<Attendance>();
        public virtual ICollection<AssignmentSubmission> AssignmentSubmissions { get; set; } = new List<AssignmentSubmission>();
        public virtual ICollection<FinalGrade> FinalGrades { get; set; } = new List<FinalGrade>();
    }
}
