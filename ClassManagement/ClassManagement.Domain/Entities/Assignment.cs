using System.ComponentModel.DataAnnotations;

namespace ClassManagement.Domain.Entities
{
    public class Assignment
    {
        public int Id { get; set; }
        
        [Required]
        public string Title { get; set; } = string.Empty;
        
        public string? Description { get; set; }
        
        [Required]
        public DateTime DueDate { get; set; }
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        public bool IsActive { get; set; } = true;
        
        public string? FileUrl { get; set; }
        
        // Foreign key
        public int CourseId { get; set; }
        
        // Navigation properties
        public virtual Course Course { get; set; } = null!;
        public virtual ICollection<AssignmentSubmission> Submissions { get; set; } = new List<AssignmentSubmission>();
        
        // Helper methods
        public bool IsDueSoon => DateTime.UtcNow.AddDays(3) >= DueDate && !IsOverdue;
        public bool IsOverdue => DateTime.UtcNow > DueDate;
    }
}
