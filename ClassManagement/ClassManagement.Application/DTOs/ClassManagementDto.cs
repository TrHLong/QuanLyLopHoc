using ClassManagement.Domain.Enums;
using ClassManagement.Domain.Entities;

namespace ClassManagement.Application.DTOs
{
    public class RegisterStudentDto
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public Grade Grade { get; set; }
    }

    public class LoginDto
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public bool RememberMe { get; set; } = false;
    }

    public enum LoginResult
    {
        Success = 0,
        EmailNotFound = 1,
        AccountLocked = 2,
        WrongPassword = 3
    }

    public class LoginResponseDto
    {
        public LoginResult Result { get; set; }
        public string Message { get; set; } = string.Empty;
        public User? User { get; set; }
    }

    public class ForgotPasswordDto
    {
        public string Email { get; set; } = string.Empty;
    }

    public class ChangePasswordDto
    {
        public string CurrentPassword { get; set; } = string.Empty;
        public string NewPassword { get; set; } = string.Empty;
        public string ConfirmPassword { get; set; } = string.Empty;
    }

    public class UpdateProfileDto
    {
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public Grade? Grade { get; set; }
    }

    public class StudentChangePasswordDto
    {
        public string CurrentPassword { get; set; } = string.Empty;
        public string NewPassword { get; set; } = string.Empty;
        public string ConfirmPassword { get; set; } = string.Empty;
    }

    public class EmailConfirmationDto
    {
        public string Email { get; set; } = string.Empty;
        public string Token { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
    }

    public class ResetPasswordDto
    {
        public string Token { get; set; } = string.Empty;
        public string NewPassword { get; set; } = string.Empty;
    }

    public class StudentDto
    {
        public int Id { get; set; }
        public string Email { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public Grade Grade { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? LastLoginAt { get; set; }
        public bool IsActive { get; set; }
        
        // Trạng thái tài khoản: true = đã khóa, false = hoạt động
        public bool IsInactive => !IsActive;
        
        // Kiểm tra tài khoản không hoạt động (không đăng nhập > 30 ngày)
        public bool IsDormant => LastLoginAt.HasValue && 
            DateTime.UtcNow.Subtract(LastLoginAt.Value).TotalDays > 30;
    }

    public class CreateCourseDto
    {
        public Grade Grade { get; set; }
        public TimeSlot TimeSlot { get; set; }
        public DateTime StartDate { get; set; }
    }

    public class CourseDto
    {
        public int Id { get; set; }
        public Grade Grade { get; set; }
        public TimeSlot TimeSlot { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int StudentCount { get; set; }
        public bool IsActive { get; set; }
        public bool IsEnded { get; set; }
        public bool IsEndingSoon { get; set; }
        public bool IsDataExported { get; set; }
    }

    public class CourseRegistrationDto
    {
        public int Id { get; set; }
        public string StudentName { get; set; } = string.Empty;
        public string StudentEmail { get; set; } = string.Empty;
        public Grade Grade { get; set; }
        public TimeSlot TimeSlot { get; set; }
        public DateTime RequestedAt { get; set; }
        public RegistrationStatus Status { get; set; }
        public string? RejectionReason { get; set; }
        public int CourseId { get; set; }
    }

    public class AttendanceDto
    {
        public int Id { get; set; }
        public int StudentId { get; set; }
        public string StudentName { get; set; } = string.Empty;
        public DateTime Date { get; set; }
        public TimeSlot TimeSlot { get; set; }
        public AttendanceStatus Status { get; set; }
    }

    public class MarkAttendanceDto
    {
        public int CourseId { get; set; }
        public DateTime Date { get; set; }
        public TimeSlot TimeSlot { get; set; }
        public List<StudentAttendanceDto> Students { get; set; } = new List<StudentAttendanceDto>();
    }

    public class StudentAttendanceDto
    {
        public int StudentId { get; set; }
        public AttendanceStatus Status { get; set; }
    }

    public class StudentAttendanceViewModel
    {
        public int StudentId { get; set; }
        public string StudentName { get; set; } = string.Empty;
        public string StudentEmail { get; set; } = string.Empty;
        public AttendanceStatus Status { get; set; }
    }

    public class AssignmentDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string? FileUrl { get; set; }
        public DateTime DueDate { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool IsOverdue { get; set; }
        public bool IsDueSoon { get; set; }
    }

    public class CreateAssignmentDto
    {
        public int CourseId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string? FileUrl { get; set; }
        public DateTime DueDate { get; set; }
    }

    public class AssignmentSubmissionDto
    {
        public int Id { get; set; }
        public int AssignmentId { get; set; }
        public string AssignmentTitle { get; set; } = string.Empty;
        public string? TextAnswer { get; set; }
        public string? FileUrl { get; set; }
        public DateTime SubmittedAt { get; set; }
        public decimal? Grade { get; set; }
        public string? Feedback { get; set; }
        public bool IsLate { get; set; }
        public bool IsGraded { get; set; }
    }

    public class SubmitAssignmentDto
    {
        public int AssignmentId { get; set; }
        public string? TextAnswer { get; set; }
        public string? FileUrl { get; set; }
    }

    public class GradeAssignmentDto
    {
        public int SubmissionId { get; set; }
        public decimal Grade { get; set; }
        public string? Feedback { get; set; }
    }

    public class FinalGradeDto
    {
        public int Id { get; set; }
        public int StudentId { get; set; }
        public string StudentName { get; set; } = string.Empty;
        public decimal Grade { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class SetFinalGradeDto
    {
        public int CourseId { get; set; }
        public int StudentId { get; set; }
        public decimal Grade { get; set; }
    }

    public class NotificationDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public NotificationType Type { get; set; }
        public bool IsRead { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class StudentAbsentReportDto
    {
        public int StudentId { get; set; }
        public string StudentName { get; set; } = string.Empty;
        public string StudentEmail { get; set; } = string.Empty;
        public int ConsecutiveAbsentDays { get; set; }
        public bool ShouldBeRemoved => ConsecutiveAbsentDays >= 8;
    }

    public class CourseExportDto
    {
        public int CourseId { get; set; }
        public string Grade { get; set; } = string.Empty;
        public string TimeSlot { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public List<StudentExportDto> Students { get; set; } = new List<StudentExportDto>();
    }

    public class StudentExportDto
    {
        public string StudentName { get; set; } = string.Empty;
        public Grade Grade { get; set; }
        public TimeSlot TimeSlot { get; set; }
        public decimal AttendanceRate { get; set; }
        public Dictionary<string, decimal> AssignmentGrades { get; set; } = new Dictionary<string, decimal>();
        public decimal? FinalGrade { get; set; }
    }
}

