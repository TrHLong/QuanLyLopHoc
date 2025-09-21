using Microsoft.EntityFrameworkCore;
using ClassManagement.Application.DTOs;
using ClassManagement.Application.Interfaces;
using ClassManagement.Domain.Entities;
using ClassManagement.Domain.Enums;
using ClassManagement.Infrastructure.Data;

namespace ClassManagement.Application.Services
{
    public class AttendanceService : IAttendanceService
    {
        private readonly ClassManagementDbContext _context;

        public AttendanceService(ClassManagementDbContext context)
        {
            _context = context;
        }

        public async Task<bool> MarkAttendanceAsync(MarkAttendanceDto dto)
        {
            foreach (var studentAttendance in dto.Students)
            {
                var existingAttendance = await _context.Attendances
                    .FirstOrDefaultAsync(a => a.StudentId == studentAttendance.StudentId &&
                                             a.CourseId == dto.CourseId &&
                                             a.Date.Date == dto.Date.Date &&
                                             a.TimeSlot == dto.TimeSlot);

                if (existingAttendance != null)
                {
                    existingAttendance.Status = studentAttendance.Status;
                }
                else
                {
                    var attendance = new Attendance
                    {
                        StudentId = studentAttendance.StudentId,
                        CourseId = dto.CourseId,
                        Date = dto.Date,
                        TimeSlot = dto.TimeSlot,
                        Status = studentAttendance.Status,
                        CreatedAt = DateTime.UtcNow
                    };

                    _context.Attendances.Add(attendance);
                }
            }

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<List<AttendanceDto>> GetStudentAttendanceAsync(int studentId)
        {
            var attendances = await _context.Attendances
                .Include(a => a.Course)
                .Where(a => a.StudentId == studentId)
                .Select(a => new AttendanceDto
                {
                    Id = a.Id,
                    StudentId = a.StudentId,
                    StudentName = "", // Would need to join with User
                    Date = a.Date,
                    TimeSlot = a.TimeSlot,
                    Status = a.Status
                })
                .OrderByDescending(a => a.Date)
                .ToListAsync();

            return attendances;
        }

        public async Task<List<AttendanceDto>> GetCourseAttendanceAsync(int courseId, DateTime date, TimeSlot timeSlot)
        {
            var attendances = await _context.Attendances
                .Include(a => a.Student)
                .Where(a => a.CourseId == courseId && 
                           a.Date.Date == date.Date && 
                           a.TimeSlot == timeSlot)
                .Select(a => new AttendanceDto
                {
                    Id = a.Id,
                    StudentId = a.StudentId,
                    StudentName = a.Student.FullName,
                    Date = a.Date,
                    TimeSlot = a.TimeSlot,
                    Status = a.Status
                })
                .ToListAsync();

            return attendances;
        }

        public async Task<decimal> GetStudentAttendanceRateAsync(int studentId, int courseId)
        {
            var attendances = await _context.Attendances
                .Where(a => a.StudentId == studentId && a.CourseId == courseId)
                .ToListAsync();

            if (!attendances.Any())
            {
                return 0;
            }

            var presentCount = attendances.Count(a => a.Status == AttendanceStatus.Present);
            return Math.Round((decimal)presentCount / attendances.Count * 100, 2);
        }

        public async Task<List<StudentAbsentReportDto>> GetConsecutiveAbsentStudentsAsync(int courseId)
        {
            // This is a simplified implementation
            // In a real scenario, you'd calculate consecutive absent days properly
            var students = await _context.CourseRegistrations
                .Include(cr => cr.Student)
                .Where(cr => cr.CourseId == courseId && cr.Status == RegistrationStatus.Approved)
                .Select(cr => new StudentAbsentReportDto
                {
                    StudentId = cr.StudentId,
                    StudentName = cr.Student.FullName,
                    StudentEmail = cr.Student.Email,
                    ConsecutiveAbsentDays = 0 // This would be calculated
                })
                .ToListAsync();

            return students;
        }
    }
}




