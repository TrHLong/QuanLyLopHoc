using System.ComponentModel.DataAnnotations;
using ClassManagement.Domain.Enums;

namespace ClassManagement.Domain.Entities
{
    public class Attendance
    {
        public int Id { get; set; }
        
        [Required]
        public int StudentId { get; set; }
        
        [Required]
        public int CourseId { get; set; }
        
        [Required]
        public DateTime Date { get; set; }
        
        [Required]
        public TimeSlot TimeSlot { get; set; }
        
        [Required]
        public AttendanceStatus Status { get; set; }
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        // Navigation properties
        public virtual User Student { get; set; } = null!;
        public virtual Course Course { get; set; } = null!;
    }
}
