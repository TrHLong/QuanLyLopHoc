using System.ComponentModel.DataAnnotations;

namespace ClassManagement.Domain.Entities
{
    public class FinalGrade
    {
        public int Id { get; set; }
        
        [Required]
        public int StudentId { get; set; }
        
        [Required]
        public int CourseId { get; set; }
        
        [Required]
        [Range(0, 10)]
        public decimal Grade { get; set; }
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        // Navigation properties
        public virtual User Student { get; set; } = null!;
        public virtual Course Course { get; set; } = null!;
    }
}




