using Microsoft.EntityFrameworkCore;
using ClassManagement.Application.DTOs;
using ClassManagement.Application.Interfaces;
using ClassManagement.Domain.Entities;
using ClassManagement.Domain.Enums;
using ClassManagement.Infrastructure.Data;

namespace ClassManagement.Application.Services
{
    public class AssignmentService : IAssignmentService
    {
        private readonly ClassManagementDbContext _context;
        private readonly INotificationService _notificationService;
        private readonly IEmailService _emailService;

        public AssignmentService(ClassManagementDbContext context, INotificationService notificationService, IEmailService emailService)
        {
            _context = context;
            _notificationService = notificationService;
            _emailService = emailService;
        }

        public async Task<Assignment?> CreateAssignmentAsync(CreateAssignmentDto dto)
        {
            var assignment = new Assignment
            {
                CourseId = dto.CourseId,
                Title = dto.Title,
                Description = dto.Description,
                FileUrl = dto.FileUrl,
                DueDate = dto.DueDate,
                CreatedAt = DateTime.UtcNow
            };

            _context.Assignments.Add(assignment);
            await _context.SaveChangesAsync();

            // Notify all students in the course
            var students = await _context.CourseRegistrations
                .Include(cr => cr.Student)
                .Where(cr => cr.CourseId == dto.CourseId && cr.Status == RegistrationStatus.Approved)
                .ToListAsync();

            foreach (var student in students)
            {
                await _notificationService.CreateNotificationAsync(
                    student.StudentId,
                    "Bài tập mới",
                    $"Bài tập mới: {assignment.Title}",
                    NotificationType.AssignmentNew);

                await _emailService.SendAssignmentDueReminderEmailAsync(
                    student.Student.Email,
                    assignment.Title,
                    assignment.DueDate);
            }

            return assignment;
        }

        public async Task<List<AssignmentDto>> GetCourseAssignmentsAsync(int courseId)
        {
            var assignments = await _context.Assignments
                .Where(a => a.CourseId == courseId)
                .Select(a => new AssignmentDto
                {
                    Id = a.Id,
                    Title = a.Title,
                    Description = a.Description,
                    FileUrl = a.FileUrl,
                    DueDate = a.DueDate,
                    CreatedAt = a.CreatedAt,
                    IsOverdue = a.IsOverdue,
                    IsDueSoon = a.IsDueSoon
                })
                .OrderByDescending(a => a.CreatedAt)
                .ToListAsync();

            return assignments;
        }

        public async Task<List<AssignmentDto>> GetStudentAssignmentsAsync(int studentId)
        {
            var assignments = await _context.Assignments
                .Include(a => a.Course)
                .ThenInclude(c => c.CourseRegistrations)
                .Where(a => a.Course.CourseRegistrations.Any(cr => cr.StudentId == studentId && cr.Status == RegistrationStatus.Approved) 
                           && a.IsActive)
                .Select(a => new AssignmentDto
                {
                    Id = a.Id,
                    Title = a.Title,
                    Description = a.Description,
                    FileUrl = a.FileUrl,
                    DueDate = a.DueDate,
                    CreatedAt = a.CreatedAt,
                    IsOverdue = a.IsOverdue,
                    IsDueSoon = a.IsDueSoon
                })
                .OrderByDescending(a => a.CreatedAt)
                .ToListAsync();

            return assignments;
        }

        public async Task<AssignmentSubmission?> SubmitAssignmentAsync(int studentId, SubmitAssignmentDto dto)
        {
            var assignment = await _context.Assignments.FindAsync(dto.AssignmentId);
            if (assignment == null)
            {
                return null;
            }

            // Check if already submitted
            var existingSubmission = await _context.AssignmentSubmissions
                .FirstOrDefaultAsync(s => s.AssignmentId == dto.AssignmentId && s.StudentId == studentId);

            if (existingSubmission != null)
            {
                return null;
            }

            var submission = new AssignmentSubmission
            {
                AssignmentId = dto.AssignmentId,
                StudentId = studentId,
                TextAnswer = dto.TextAnswer,
                FileUrl = dto.FileUrl,
                SubmittedAt = DateTime.UtcNow
            };

            _context.AssignmentSubmissions.Add(submission);
            await _context.SaveChangesAsync();

            // Gửi email xác nhận nộp bài tập
            var student = await _context.Users.FindAsync(studentId);
            if (student != null)
            {
                try
                {
                    await _emailService.SendAssignmentSubmissionConfirmationEmailAsync(
                        student.Email,
                        student.FullName,
                        assignment.Title,
                        submission.SubmittedAt,
                        assignment.DueDate);
                }
                catch (Exception ex)
                {
                    // Log lỗi nhưng không làm fail submission
                    Console.WriteLine($"Lỗi gửi email xác nhận nộp bài: {ex.Message}");
                }
            }

            return submission;
        }

        public async Task<bool> GradeAssignmentAsync(GradeAssignmentDto dto)
        {
            var submission = await _context.AssignmentSubmissions
                .Include(s => s.Student)
                .Include(s => s.Assignment)
                .FirstOrDefaultAsync(s => s.Id == dto.SubmissionId);

            if (submission == null)
            {
                return false;
            }

            submission.Grade = dto.Grade;
            submission.Feedback = dto.Feedback;
            submission.GradedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            // Notify student
            await _notificationService.CreateNotificationAsync(
                submission.StudentId,
                "Điểm bài tập",
                $"Bạn đã nhận điểm {dto.Grade} cho bài tập: {submission.Assignment.Title}",
                NotificationType.GradeReceived);

            return true;
        }

        public async Task<List<AssignmentSubmissionDto>> GetAssignmentSubmissionsAsync(int assignmentId)
        {
            var submissions = await _context.AssignmentSubmissions
                .Include(s => s.Student)
                .Include(s => s.Assignment)
                .Where(s => s.AssignmentId == assignmentId)
                .Select(s => new AssignmentSubmissionDto
                {
                    Id = s.Id,
                    AssignmentId = s.AssignmentId,
                    AssignmentTitle = s.Assignment.Title,
                    TextAnswer = s.TextAnswer,
                    FileUrl = s.FileUrl,
                    SubmittedAt = s.SubmittedAt,
                    Grade = s.Grade,
                    Feedback = s.Feedback,
                    IsLate = s.IsLate,
                    IsGraded = s.IsGraded
                })
                .ToListAsync();

            return submissions;
        }

        public async Task<AssignmentSubmissionDto?> GetSubmissionByIdAsync(int submissionId)
        {
            var submission = await _context.AssignmentSubmissions
                .Include(s => s.Student)
                .Include(s => s.Assignment)
                .Where(s => s.Id == submissionId)
                .Select(s => new AssignmentSubmissionDto
                {
                    Id = s.Id,
                    AssignmentId = s.AssignmentId,
                    AssignmentTitle = s.Assignment.Title,
                    TextAnswer = s.TextAnswer,
                    FileUrl = s.FileUrl,
                    SubmittedAt = s.SubmittedAt,
                    Grade = s.Grade,
                    Feedback = s.Feedback,
                    IsLate = s.IsLate,
                    IsGraded = s.IsGraded
                })
                .FirstOrDefaultAsync();

            return submission;
        }

        public async Task<List<AssignmentSubmissionDto>> GetStudentSubmissionsAsync(int studentId)
        {
            var submissions = await _context.AssignmentSubmissions
                .Include(s => s.Assignment)
                .Where(s => s.StudentId == studentId)
                .Select(s => new AssignmentSubmissionDto
                {
                    Id = s.Id,
                    AssignmentId = s.AssignmentId,
                    AssignmentTitle = s.Assignment.Title,
                    TextAnswer = s.TextAnswer,
                    FileUrl = s.FileUrl,
                    SubmittedAt = s.SubmittedAt,
                    Grade = s.Grade,
                    Feedback = s.Feedback,
                    IsLate = s.IsLate,
                    IsGraded = s.IsGraded
                })
                .OrderByDescending(s => s.SubmittedAt)
                .ToListAsync();

            return submissions;
        }

        public async Task<Assignment?> GetAssignmentByIdAsync(int assignmentId)
        {
            return await _context.Assignments
                .Include(a => a.Course)
                .FirstOrDefaultAsync(a => a.Id == assignmentId);
        }

        public async Task<bool> DeleteAssignmentAsync(int assignmentId)
        {
            var assignment = await _context.Assignments
                .Include(a => a.Submissions)
                .FirstOrDefaultAsync(a => a.Id == assignmentId);

            if (assignment == null)
            {
                return false;
            }

            // Kiểm tra xem có bài nộp nào không
            if (assignment.Submissions.Any())
            {
                // Nếu có bài nộp, chỉ đánh dấu là không hoạt động thay vì xóa
                assignment.IsActive = false;
                await _context.SaveChangesAsync();
                
                // Thông báo cho học sinh
                var students = await _context.CourseRegistrations
                    .Include(cr => cr.Student)
                    .Where(cr => cr.CourseId == assignment.CourseId && cr.Status == RegistrationStatus.Approved)
                    .ToListAsync();

                foreach (var student in students)
                {
                    await _notificationService.CreateNotificationAsync(
                        student.StudentId,
                        "Bài tập đã hủy",
                        $"Bài tập '{assignment.Title}' đã bị hủy bởi giáo viên",
                        NotificationType.AssignmentCancelled);
                }
            }
            else
            {
                // Nếu chưa có bài nộp, xóa hoàn toàn
                _context.Assignments.Remove(assignment);
                await _context.SaveChangesAsync();
            }

            return true;
        }
    }
}

