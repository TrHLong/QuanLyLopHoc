using Microsoft.EntityFrameworkCore;
using ClassManagement.Application.DTOs;
using ClassManagement.Application.Interfaces;
using ClassManagement.Domain.Entities;
using ClassManagement.Domain.Enums;
using ClassManagement.Infrastructure.Data;

namespace ClassManagement.Application.Services
{
    public class CourseService : ICourseService
    {
        private readonly ClassManagementDbContext _context;

        public CourseService(ClassManagementDbContext context)
        {
            _context = context;
        }

        public async Task<Course?> CreateCourseAsync(CreateCourseDto dto)
        {
            // Kiểm tra ngày bắt đầu phải sau ngày hiện tại
            if (dto.StartDate.Date <= DateTime.UtcNow.Date)
            {
                throw new InvalidOperationException("Ngày bắt đầu lớp học phải sau ngày hiện tại");
            }

            // Kiểm tra trùng lịch học
            var conflictResult = await CheckScheduleConflictAsync(dto.Grade, dto.TimeSlot, dto.StartDate);
            if (conflictResult.HasConflict)
            {
                throw new InvalidOperationException(conflictResult.ErrorMessage);
            }

            var course = new Course
            {
                Grade = dto.Grade,
                TimeSlot = dto.TimeSlot,
                StartDate = dto.StartDate,
                EndDate = Course.CalculateEndDate(dto.StartDate),
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            };

            _context.Courses.Add(course);
            await _context.SaveChangesAsync();

            return course;
        }

        public async Task<List<CourseDto>> GetCoursesAsync(bool? isActive = null)
        {
            var query = _context.Courses.AsQueryable();

            if (isActive.HasValue)
            {
                query = query.Where(c => c.IsActive == isActive.Value);
            }

            var now = DateTime.UtcNow;
            var courses = await query
                .Select(c => new CourseDto
                {
                    Id = c.Id,
                    Grade = c.Grade,
                    TimeSlot = c.TimeSlot,
                    StartDate = c.StartDate,
                    EndDate = c.EndDate,
                    StudentCount = c.CourseRegistrations.Count(cr => cr.Status == RegistrationStatus.Approved),
                    IsActive = c.IsActive,
                    IsEnded = c.EndDate <= now,
                    IsEndingSoon = c.EndDate <= now.AddDays(3) && c.EndDate > now,
                    IsDataExported = c.IsDataExported
                })
                .ToListAsync();

            return courses;
        }

        public async Task<Course?> GetCourseByIdAsync(int courseId)
        {
            return await _context.Courses
                .Include(c => c.CourseRegistrations)
                .ThenInclude(cr => cr.Student)
                .Include(c => c.Assignments.Where(a => a.IsActive))
                .FirstOrDefaultAsync(c => c.Id == courseId);
        }

        public async Task<bool> DeleteStudentFromCourseAsync(int courseId, int studentId)
        {
            var registration = await _context.CourseRegistrations
                .FirstOrDefaultAsync(cr => cr.CourseId == courseId && cr.StudentId == studentId);

            if (registration == null)
            {
                return false;
            }

            _context.CourseRegistrations.Remove(registration);
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<List<StudentAbsentReportDto>> GetAbsentStudentsAsync(int courseId)
        {
            // This is a simplified implementation
            // In a real scenario, you'd calculate consecutive absent days
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

        public async Task<byte[]> ExportCourseDataAsync(int courseId)
        {
            // This is a simplified implementation
            // In a real scenario, you'd generate a proper CSV file
            var course = await GetCourseByIdAsync(courseId);
            if (course == null)
            {
                return Array.Empty<byte>();
            }

            var csvContent = $"Course ID,Grade,TimeSlot,StartDate,EndDate\n";
            csvContent += $"{course.Id},{course.Grade},{course.TimeSlot},{course.StartDate:yyyy-MM-dd},{course.EndDate:yyyy-MM-dd}\n";

            return System.Text.Encoding.UTF8.GetBytes(csvContent);
        }

        public async Task<bool> IsCourseEndingSoonAsync(int courseId)
        {
            var course = await _context.Courses.FindAsync(courseId);
            if (course == null) return false;
            
            var now = DateTime.UtcNow;
            return course.EndDate <= now.AddDays(3) && course.EndDate > now;
        }

        public async Task<List<CourseDto>> GetAvailableCoursesAsync(Grade grade)
        {
            var now = DateTime.UtcNow;
            var courses = await _context.Courses
                .Where(c => c.Grade == grade && c.IsActive && c.EndDate > now)
                .Select(c => new CourseDto
                {
                    Id = c.Id,
                    Grade = c.Grade,
                    TimeSlot = c.TimeSlot,
                    StartDate = c.StartDate,
                    EndDate = c.EndDate,
                    StudentCount = c.CourseRegistrations.Count(cr => cr.Status == RegistrationStatus.Approved),
                    IsActive = c.IsActive,
                    IsEnded = c.EndDate <= now,
                    IsEndingSoon = c.EndDate <= now.AddDays(3) && c.EndDate > now,
                    IsDataExported = c.IsDataExported
                })
                .ToListAsync();

            return courses;
        }

        // Lấy khóa học theo khối của học sinh đã đăng nhập
        public async Task<List<CourseDto>> GetCoursesForStudentAsync(int studentId)
        {
            // Lấy thông tin học sinh để biết khối
            var student = await _context.Users.FindAsync(studentId);
            if (student == null || student.Grade == null)
            {
                return new List<CourseDto>();
            }

            return await GetAvailableCoursesAsync(student.Grade.Value);
        }

        // Kiểm tra trùng lịch học
        private async Task<ScheduleConflictResult> CheckScheduleConflictAsync(Grade grade, TimeSlot timeSlot, DateTime startDate)
        {
            // Lấy các ngày học trong tuần cho khối này
            var scheduleDays = Course.GetScheduleDays(grade);
            var dayOfWeek = startDate.DayOfWeek;

            // Kiểm tra xem ngày bắt đầu có trong lịch học không
            if (!scheduleDays.Contains(dayOfWeek))
            {
                var scheduleDayNames = scheduleDays.Select(GetDayOfWeekName).ToArray();
                return new ScheduleConflictResult
                {
                    HasConflict = true,
                    ErrorMessage = $"❌ Ngày bắt đầu không phù hợp!\n\n" +
                                 $"Khối {grade} học vào: {string.Join(", ", scheduleDayNames)}\n" +
                                 $"Bạn chọn: {GetDayOfWeekName(dayOfWeek)}\n\n" +
                                 $"Ví dụ: Nếu chọn Khối 10, ngày bắt đầu phải là Thứ 2 hoặc Thứ 4"
                };
            }

            // Kiểm tra trùng lịch với các khóa học khác trong cùng khối và ca
            var conflictingCourses = await _context.Courses
                .Where(c => c.IsActive && 
                           c.Grade == grade && 
                           c.TimeSlot == timeSlot)
                .ToListAsync();

            // Kiểm tra xem có khóa học nào đang diễn ra trong khoảng thời gian này không
            foreach (var existingCourse in conflictingCourses)
            {
                if (startDate >= existingCourse.StartDate && startDate <= existingCourse.EndDate)
                {
                    return new ScheduleConflictResult
                    {
                        HasConflict = true,
                    ErrorMessage = $"❌ Trùng lịch học!\n\n" +
                                 $"Đã có lớp học Khối {existingCourse.Grade} - Ca {existingCourse.TimeSlot}\n" +
                                 $"Thời gian: {existingCourse.StartDate:dd/MM/yyyy} đến {existingCourse.EndDate:dd/MM/yyyy}\n" +
                                 $"Lịch học: {string.Join(", ", Course.GetScheduleDays(existingCourse.Grade).Select(GetDayOfWeekName))}"
                    };
                }
            }

            return new ScheduleConflictResult { HasConflict = false };
        }

        // Xóa khóa học nếu chưa có học viên
        public async Task<bool> DeleteCourseAsync(int courseId)
        {
            var course = await _context.Courses
                .Include(c => c.CourseRegistrations)
                .FirstOrDefaultAsync(c => c.Id == courseId);

            if (course == null)
            {
                return false;
            }

            // Kiểm tra xem có học viên nào đã đăng ký không
            if (course.CourseRegistrations.Any())
            {
                throw new InvalidOperationException("Không thể xóa lớp học đã có học viên đăng ký");
            }

            _context.Courses.Remove(course);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<List<StudentDto>> GetRegisteredStudentsAsync(int courseId)
        {
            var students = await _context.CourseRegistrations
                .Include(cr => cr.Student)
                .Where(cr => cr.CourseId == courseId && cr.Status == RegistrationStatus.Approved)
                .Select(cr => new StudentDto
                {
                    Id = cr.Student.Id,
                    Email = cr.Student.Email,
                    FullName = cr.Student.FullName,
                    Grade = cr.Student.Grade ?? Grade.Grade10,
                    CreatedAt = cr.Student.CreatedAt,
                    LastLoginAt = cr.Student.LastLoginAt,
                    IsActive = cr.Student.IsActive
                })
                .ToListAsync();

            return students;
        }

        // Lấy tên ngày trong tuần
        private string GetDayOfWeekName(DayOfWeek dayOfWeek)
        {
            return dayOfWeek switch
            {
                DayOfWeek.Monday => "Thứ 2",
                DayOfWeek.Tuesday => "Thứ 3",
                DayOfWeek.Wednesday => "Thứ 4",
                DayOfWeek.Thursday => "Thứ 5",
                DayOfWeek.Friday => "Thứ 6",
                DayOfWeek.Saturday => "Thứ 7",
                DayOfWeek.Sunday => "Chủ nhật",
                _ => dayOfWeek.ToString()
            };
        }
    }

    // Class để trả về kết quả kiểm tra trùng lịch
    public class ScheduleConflictResult
    {
        public bool HasConflict { get; set; }
        public string ErrorMessage { get; set; } = string.Empty;
    }
}

