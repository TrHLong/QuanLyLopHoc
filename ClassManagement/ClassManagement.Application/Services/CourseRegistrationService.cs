using Microsoft.EntityFrameworkCore;
using ClassManagement.Application.DTOs;
using ClassManagement.Application.Interfaces;
using ClassManagement.Domain.Entities;
using ClassManagement.Domain.Enums;
using ClassManagement.Infrastructure.Data;

namespace ClassManagement.Application.Services
{
    public class CourseRegistrationService : ICourseRegistrationService
    {
        private readonly ClassManagementDbContext _context;
        private readonly INotificationService _notificationService;
        private readonly IEmailService _emailService;

        public CourseRegistrationService(ClassManagementDbContext context, INotificationService notificationService, IEmailService emailService)
        {
            _context = context;
            _notificationService = notificationService;
            _emailService = emailService;
        }

        public async Task<CourseRegistration?> RegisterForCourseAsync(int studentId, int courseId)
        {
            // Check if student is already registered for this course
            var existingRegistration = await _context.CourseRegistrations
                .FirstOrDefaultAsync(cr => cr.StudentId == studentId && cr.CourseId == courseId);

            if (existingRegistration != null)
            {
                return null;
            }

            var registration = new CourseRegistration
            {
                StudentId = studentId,
                CourseId = courseId,
                Status = RegistrationStatus.Pending,
                RequestedAt = DateTime.UtcNow
            };

            _context.CourseRegistrations.Add(registration);
            await _context.SaveChangesAsync();

            return registration;
        }

        public async Task<List<CourseRegistrationDto>> GetPendingRegistrationsAsync()
        {
            var registrations = await _context.CourseRegistrations
                .Include(cr => cr.Student)
                .Include(cr => cr.Course)
                .Where(cr => cr.Status == RegistrationStatus.Pending)
                .Select(cr => new CourseRegistrationDto
                {
                    Id = cr.Id,
                    StudentName = cr.Student.FullName,
                    StudentEmail = cr.Student.Email,
                    Grade = cr.Course.Grade,
                    TimeSlot = cr.Course.TimeSlot,
                    RequestedAt = cr.RequestedAt,
                    Status = cr.Status,
                    CourseId = cr.CourseId
                })
                .ToListAsync();

            return registrations;
        }

        public async Task<bool> ApproveRegistrationAsync(int registrationId, bool approved, string? rejectionReason = null)
        {
            var registration = await _context.CourseRegistrations
                .Include(cr => cr.Student)
                .Include(cr => cr.Course)
                .FirstOrDefaultAsync(cr => cr.Id == registrationId);

            if (registration == null)
            {
                return false;
            }

            registration.Status = approved ? RegistrationStatus.Approved : RegistrationStatus.Rejected;
            registration.RejectionReason = rejectionReason;
            registration.ProcessedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            // Send notification and email
            if (approved)
            {
                await _notificationService.CreateNotificationAsync(
                    registration.StudentId,
                    "Đăng ký khóa học thành công",
                    $"Đăng ký khóa học {registration.Course.Grade} - Ca {registration.Course.TimeSlot} đã được duyệt.",
                    NotificationType.RegistrationApproved);

                await _emailService.SendRegistrationApprovalEmailAsync(
                    registration.Student.Email,
                    $"Khóa học {registration.Course.Grade} - Ca {registration.Course.TimeSlot}");
            }
            else
            {
                await _notificationService.CreateNotificationAsync(
                    registration.StudentId,
                    "Đăng ký khóa học bị từ chối",
                    $"Đăng ký khóa học {registration.Course.Grade} - Ca {registration.Course.TimeSlot} đã bị từ chối. Lý do: {rejectionReason}",
                    NotificationType.RegistrationRejected);
            }

            return true;
        }

        public async Task<List<CourseRegistrationDto>> GetStudentRegistrationsAsync(int studentId)
        {
            var registrations = await _context.CourseRegistrations
                .Include(cr => cr.Course)
                .Where(cr => cr.StudentId == studentId)
                .Select(cr => new CourseRegistrationDto
                {
                    Id = cr.Id,
                    StudentName = "", // Not needed for student view
                    StudentEmail = "", // Not needed for student view
                    Grade = cr.Course.Grade,
                    TimeSlot = cr.Course.TimeSlot,
                    RequestedAt = cr.RequestedAt,
                    Status = cr.Status,
                    RejectionReason = cr.RejectionReason,
                    CourseId = cr.CourseId
                })
                .ToListAsync();

            return registrations;
        }

        public async Task<bool> RemoveStudentFromCourseAsync(int studentId, int courseId)
        {
            var registration = await _context.CourseRegistrations
                .FirstOrDefaultAsync(cr => cr.StudentId == studentId && cr.CourseId == courseId);

            if (registration == null)
            {
                return false;
            }

            _context.CourseRegistrations.Remove(registration);
            await _context.SaveChangesAsync();

            // Gửi thông báo cho học viên
            await _notificationService.CreateNotificationAsync(
                studentId,
                "Đã được xóa khỏi lớp học",
                $"Bạn đã được xóa khỏi lớp học. Bạn có thể đăng ký lớp học mới.",
                NotificationType.RegistrationRejected);

            return true;
        }
    }
}


