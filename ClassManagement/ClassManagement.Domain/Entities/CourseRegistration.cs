using System.ComponentModel.DataAnnotations;
using ClassManagement.Domain.Enums;

namespace ClassManagement.Domain.Entities
{
    public class CourseRegistration
    {
        public int Id { get; set; }
        
        [Required]
        public int StudentId { get; set; }
        
        [Required]
        public int CourseId { get; set; }
        
        [Required]
        public RegistrationStatus Status { get; set; } = RegistrationStatus.Pending;
        
        public string? RejectionReason { get; set; }
        
        public DateTime RequestedAt { get; set; } = DateTime.UtcNow;
        
        public DateTime? ProcessedAt { get; set; }
        
        // Navigation properties
        public virtual User Student { get; set; } = null!;
        public virtual Course Course { get; set; } = null!;
    }
}
