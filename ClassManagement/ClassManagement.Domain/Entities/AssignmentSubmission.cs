using System.ComponentModel.DataAnnotations;

namespace ClassManagement.Domain.Entities
{
    public class AssignmentSubmission
    {
        public int Id { get; set; }
        
        public string? Content { get; set; }
        
        public string? TextAnswer { get; set; }
        
        public string? FilePath { get; set; }
        
        public string? FileUrl { get; set; }
        
        public DateTime SubmittedAt { get; set; } = DateTime.UtcNow;
        
        public decimal? Grade { get; set; }
        
        public string? Feedback { get; set; }
        
        public DateTime? GradedAt { get; set; }
        
        // Foreign keys
        public int AssignmentId { get; set; }
        public int StudentId { get; set; }
        
        // Navigation properties
        public virtual Assignment Assignment { get; set; } = null!;
        public virtual User Student { get; set; } = null!;
        
        // Helper methods
        public bool IsLate => SubmittedAt > Assignment.DueDate;
        public bool IsGraded => Grade.HasValue;
    }
}
