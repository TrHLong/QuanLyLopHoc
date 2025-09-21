using System.ComponentModel.DataAnnotations;
using ClassManagement.Domain.Enums;

namespace ClassManagement.Domain.Entities
{
    public class Course
    {
        public int Id { get; set; }
        
        [Required]
        public Grade Grade { get; set; }
        
        [Required]
        public TimeSlot TimeSlot { get; set; }
        
        [Required]
        public DateTime StartDate { get; set; }
        
        [Required]
        public DateTime EndDate { get; set; }
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        public bool IsActive { get; set; } = true;
        
        public bool IsDataExported { get; set; } = false;
        
        // Navigation properties
        public virtual ICollection<CourseRegistration> CourseRegistrations { get; set; } = new List<CourseRegistration>();
        public virtual ICollection<Attendance> Attendances { get; set; } = new List<Attendance>();
        public virtual ICollection<Assignment> Assignments { get; set; } = new List<Assignment>();
        public virtual ICollection<FinalGrade> FinalGrades { get; set; } = new List<FinalGrade>();
        
        // Helper methods
        public bool IsEnded => DateTime.UtcNow > EndDate;
        public bool IsEndingSoon => DateTime.UtcNow.AddDays(3) >= EndDate && !IsEnded;
        
        public static DateTime CalculateEndDate(DateTime startDate)
        {
            return startDate.AddMonths(5);
        }
        
        public static List<DayOfWeek> GetScheduleDays(Grade grade)
        {
            return grade switch
            {
                Grade.Grade10 => new List<DayOfWeek> { DayOfWeek.Monday, DayOfWeek.Wednesday },
                Grade.Grade11 => new List<DayOfWeek> { DayOfWeek.Tuesday, DayOfWeek.Thursday },
                Grade.Grade12 => new List<DayOfWeek> { DayOfWeek.Friday, DayOfWeek.Saturday },
                _ => new List<DayOfWeek>()
            };
        }
        
        public bool IsTodayClassDay()
        {
            var today = DateTime.Today;
            var scheduleDays = GetScheduleDays(Grade);
            return scheduleDays.Contains(today.DayOfWeek) && 
                   today >= StartDate.Date && 
                   today <= EndDate.Date;
        }
        
        public bool IsClassTime(TimeSlot timeSlot)
        {
            var now = DateTime.Now;
            var today = DateTime.Today;
            
            if (!IsTodayClassDay()) return false;
            
            // Kiểm tra thời gian ca học
            return timeSlot switch
            {
                TimeSlot.Slot1 => now.Hour >= 14 && now.Hour < 16, // 14-16h
                TimeSlot.Slot2 => now.Hour >= 17 && now.Hour < 19, // 17-19h
                _ => false
            };
        }
        
        public string GetNextClassDay()
        {
            var today = DateTime.Today;
            var scheduleDays = GetScheduleDays(Grade);
            
            // Tìm ngày học tiếp theo
            for (int i = 0; i < 7; i++)
            {
                var checkDate = today.AddDays(i);
                if (scheduleDays.Contains(checkDate.DayOfWeek) && 
                    checkDate >= StartDate.Date && 
                    checkDate <= EndDate.Date)
                {
                    return checkDate.ToString("dd/MM/yyyy");
                }
            }
            
            return "Không có buổi học tiếp theo";
        }
    }
}
