using Microsoft.EntityFrameworkCore;
using ClassManagement.Application.DTOs;
using ClassManagement.Application.Interfaces;
using ClassManagement.Domain.Entities;
using ClassManagement.Domain.Enums;
using ClassManagement.Infrastructure.Data;

namespace ClassManagement.Application.Services
{
    public class NotificationService : INotificationService
    {
        private readonly ClassManagementDbContext _context;

        public NotificationService(ClassManagementDbContext context)
        {
            _context = context;
        }

        public async Task<Notification> CreateNotificationAsync(int userId, string title, string message, NotificationType type)
        {
            var notification = new Notification
            {
                UserId = userId,
                Title = title,
                Message = message,
                Type = type,
                IsRead = false,
                CreatedAt = DateTime.UtcNow
            };

            _context.Notifications.Add(notification);
            await _context.SaveChangesAsync();

            return notification;
        }

        public async Task<List<NotificationDto>> GetUserNotificationsAsync(int userId)
        {
            var notifications = await _context.Notifications
                .Where(n => n.UserId == userId)
                .Select(n => new NotificationDto
                {
                    Id = n.Id,
                    Title = n.Title,
                    Message = n.Message,
                    Type = n.Type,
                    IsRead = n.IsRead,
                    CreatedAt = n.CreatedAt
                })
                .OrderByDescending(n => n.CreatedAt)
                .ToListAsync();

            return notifications;
        }

        public async Task<bool> MarkNotificationAsReadAsync(int notificationId)
        {
            var notification = await _context.Notifications.FindAsync(notificationId);
            if (notification == null)
            {
                return false;
            }

            notification.IsRead = true;
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<bool> SendAssignmentDueReminderAsync()
        {
            var assignments = await _context.Assignments
                .Include(a => a.Course)
                .ThenInclude(c => c.CourseRegistrations)
                .ThenInclude(cr => cr.Student)
                .Where(a => a.IsDueSoon && !a.IsOverdue)
                .ToListAsync();

            foreach (var assignment in assignments)
            {
                var students = assignment.Course.CourseRegistrations
                    .Where(cr => cr.Status == RegistrationStatus.Approved)
                    .Select(cr => cr.Student)
                    .ToList();

                foreach (var student in students)
                {
                    await CreateNotificationAsync(
                        student.Id,
                        "Nhắc nhở nộp bài",
                        $"Bài tập '{assignment.Title}' sắp đến hạn nộp.",
                        NotificationType.AssignmentDue);
                }
            }

            return true;
        }

        public async Task<bool> SendCourseEndingReminderAsync()
        {
            var courses = await _context.Courses
                .Include(c => c.CourseRegistrations)
                .ThenInclude(cr => cr.Student)
                .Where(c => c.IsEndingSoon)
                .ToListAsync();

            foreach (var course in courses)
            {
                var students = course.CourseRegistrations
                    .Where(cr => cr.Status == RegistrationStatus.Approved)
                    .Select(cr => cr.Student)
                    .ToList();

                foreach (var student in students)
                {
                    await CreateNotificationAsync(
                        student.Id,
                        "Khóa học sắp kết thúc",
                        $"Khóa học {course.Grade} - Ca {course.TimeSlot} sắp kết thúc vào {course.EndDate:dd/MM/yyyy}.",
                        NotificationType.CourseEndingSoon);
                }
            }

            return true;
        }
    }
}




