using System.ComponentModel.DataAnnotations;

namespace ClassManagement.Domain.Entities
{
    public class PasswordResetToken
    {
        public int Id { get; set; }
        
        [Required]
        public int UserId { get; set; }
        
        [Required]
        public string Token { get; set; } = string.Empty;
        
        [Required]
        public DateTime ExpiresAt { get; set; }
        
        public bool IsUsed { get; set; } = false;
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        // Navigation properties
        public virtual User User { get; set; } = null!;
        
        // Helper methods
        public bool IsExpired => DateTime.UtcNow > ExpiresAt;
        public bool IsValid => !IsUsed && !IsExpired;
    }
}




