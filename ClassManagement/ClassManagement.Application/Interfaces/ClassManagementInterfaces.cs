using ClassManagement.Application.DTOs;
using ClassManagement.Domain.Entities;
using ClassManagement.Domain.Enums;

namespace ClassManagement.Application.Interfaces
{
    public interface IUserService
    {
        Task<User?> RegisterStudentAsync(RegisterStudentDto dto);
        Task<LoginResponseDto> LoginAsync(LoginDto dto);
        Task<bool> ForgotPasswordAsync(ForgotPasswordDto dto);
        Task<bool> ResetPasswordAsync(ResetPasswordDto dto);
        Task<List<StudentDto>> GetStudentsAsync();
        Task<List<StudentDto>> GetRegisteredStudentsAsync();
        Task<bool> DeleteStudentAsync(int studentId);
        Task<bool> UpdateLastLoginAsync(int userId);
        Task<bool> IsStudentInactiveAsync(int studentId);
        Task<User?> GetUserByIdAsync(int id);
        Task<StudentDto?> GetStudentByIdAsync(int id);
        Task<bool> ToggleStudentStatusAsync(int studentId);
        Task<bool> ChangePasswordAsync(int userId, ChangePasswordDto dto);
        Task<bool> UpdateProfileAsync(int userId, UpdateProfileDto dto);
        Task<bool> StudentChangePasswordAsync(int userId, StudentChangePasswordDto dto);
    }

    public interface ICourseService
    {
        Task<Course?> CreateCourseAsync(CreateCourseDto dto);
        Task<List<CourseDto>> GetCoursesAsync(bool? isActive = null);
        Task<Course?> GetCourseByIdAsync(int courseId);
        Task<bool> DeleteStudentFromCourseAsync(int courseId, int studentId);
        Task<List<StudentAbsentReportDto>> GetAbsentStudentsAsync(int courseId);
        Task<byte[]> ExportCourseDataAsync(int courseId);
        Task<bool> IsCourseEndingSoonAsync(int courseId);
        Task<List<CourseDto>> GetAvailableCoursesAsync(Grade grade);
        Task<List<CourseDto>> GetCoursesForStudentAsync(int studentId);
        Task<bool> DeleteCourseAsync(int courseId);
        Task<List<StudentDto>> GetRegisteredStudentsAsync(int courseId);
    }

    public interface ICourseRegistrationService
    {
        Task<CourseRegistration?> RegisterForCourseAsync(int studentId, int courseId);
        Task<List<CourseRegistrationDto>> GetPendingRegistrationsAsync();
        Task<bool> ApproveRegistrationAsync(int registrationId, bool approved, string? rejectionReason = null);
        Task<List<CourseRegistrationDto>> GetStudentRegistrationsAsync(int studentId);
        Task<bool> RemoveStudentFromCourseAsync(int studentId, int courseId);
    }

    public interface IAttendanceService
    {
        Task<bool> MarkAttendanceAsync(MarkAttendanceDto dto);
        Task<List<AttendanceDto>> GetStudentAttendanceAsync(int studentId);
        Task<List<AttendanceDto>> GetCourseAttendanceAsync(int courseId, DateTime date, TimeSlot timeSlot);
        Task<decimal> GetStudentAttendanceRateAsync(int studentId, int courseId);
        Task<List<StudentAbsentReportDto>> GetConsecutiveAbsentStudentsAsync(int courseId);
    }

    public interface IAssignmentService
    {
        Task<Assignment?> CreateAssignmentAsync(CreateAssignmentDto dto);
        Task<List<AssignmentDto>> GetCourseAssignmentsAsync(int courseId);
        Task<List<AssignmentDto>> GetStudentAssignmentsAsync(int studentId);
        Task<AssignmentSubmission?> SubmitAssignmentAsync(int studentId, SubmitAssignmentDto dto);
        Task<bool> GradeAssignmentAsync(GradeAssignmentDto dto);
        Task<List<AssignmentSubmissionDto>> GetAssignmentSubmissionsAsync(int assignmentId);
        Task<AssignmentSubmissionDto?> GetSubmissionByIdAsync(int submissionId);
        Task<List<AssignmentSubmissionDto>> GetStudentSubmissionsAsync(int studentId);
        Task<Assignment?> GetAssignmentByIdAsync(int assignmentId);
        Task<bool> DeleteAssignmentAsync(int assignmentId);
    }

    public interface IFinalGradeService
    {
        Task<bool> SetFinalGradeAsync(SetFinalGradeDto dto);
        Task<FinalGrade?> GetStudentFinalGradeAsync(int studentId, int courseId);
        Task<List<FinalGradeDto>> GetCourseFinalGradesAsync(int courseId);
    }

    public interface INotificationService
    {
        Task<Notification> CreateNotificationAsync(int userId, string title, string message, Domain.Enums.NotificationType type);
        Task<List<NotificationDto>> GetUserNotificationsAsync(int userId);
        Task<bool> MarkNotificationAsReadAsync(int notificationId);
        Task<bool> SendAssignmentDueReminderAsync();
        Task<bool> SendCourseEndingReminderAsync();
    }

    public interface IEmailService
    {
        Task<bool> SendEmailAsync(string to, string subject, string body);
        Task<bool> SendPasswordResetEmailAsync(string email, string resetToken);
        Task<bool> SendRegistrationApprovalEmailAsync(string email, string courseInfo);
        Task<bool> SendAssignmentDueReminderEmailAsync(string email, string assignmentTitle, DateTime dueDate);
        Task<bool> SendCourseEndingReminderEmailAsync(string email, string courseInfo, DateTime endDate);
        Task<bool> SendRegistrationConfirmationEmailAsync(string email, string userName);
        Task<bool> SendPasswordChangeConfirmationEmailAsync(string email, string userName);
        Task<bool> SendAssignmentSubmissionConfirmationEmailAsync(string email, string studentName, string assignmentTitle, DateTime submittedAt, DateTime dueDate);
        Task<bool> SendAccountLockedNotificationAsync(string email, string userName);
        Task<bool> SendAccountUnlockedNotificationAsync(string email, string userName);
    }
}
