using Microsoft.EntityFrameworkCore;
using ClassManagement.Application.DTOs;
using ClassManagement.Application.Interfaces;
using ClassManagement.Domain.Entities;
using ClassManagement.Domain.Enums;
using ClassManagement.Infrastructure.Data;

namespace ClassManagement.Application.Services
{
    public class UserService : IUserService
    {
        private readonly ClassManagementDbContext _context;
        private readonly IEmailService _emailService;

        public UserService(ClassManagementDbContext context, IEmailService emailService)
        {
            _context = context;
            _emailService = emailService;
        }

        public async Task<User?> RegisterStudentAsync(RegisterStudentDto dto)
        {
            // Check if email already exists
            if (await _context.Users.AnyAsync(u => u.Email == dto.Email))
            {
                return null;
            }

            var user = new User
            {
                Email = dto.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
                FullName = dto.FullName,
                Role = UserRole.Student,
                Grade = dto.Grade,
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return user;
        }

        public async Task<LoginResponseDto> LoginAsync(LoginDto dto)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Email == dto.Email);

            if (user == null)
            {
                return new LoginResponseDto
                {
                    Result = LoginResult.EmailNotFound,
                    Message = "Email không tồn tại trong hệ thống"
                };
            }

            if (!user.IsActive)
            {
                return new LoginResponseDto
                {
                    Result = LoginResult.AccountLocked,
                    Message = "Tài khoản của bạn đã bị khóa. Vui lòng liên hệ với giáo viên để được hỗ trợ."
                };
            }

            if (!BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
            {
                return new LoginResponseDto
                {
                    Result = LoginResult.WrongPassword,
                    Message = "Mật khẩu không đúng"
                };
            }

            // Update last login
            user.LastLoginAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return new LoginResponseDto
            {
                Result = LoginResult.Success,
                Message = "Đăng nhập thành công",
                User = user
            };
        }

        public async Task<bool> ForgotPasswordAsync(ForgotPasswordDto dto)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Email == dto.Email && u.IsActive);

            if (user == null)
            {
                return false;
            }

            var token = Guid.NewGuid().ToString();
            var resetToken = new PasswordResetToken
            {
                UserId = user.Id,
                Token = token,
                ExpiresAt = DateTime.UtcNow.AddHours(24),
                CreatedAt = DateTime.UtcNow
            };

            _context.PasswordResetTokens.Add(resetToken);
            await _context.SaveChangesAsync();

            await _emailService.SendPasswordResetEmailAsync(user.Email, token);

            return true;
        }

        public async Task<bool> ResetPasswordAsync(ResetPasswordDto dto)
        {
            var resetToken = await _context.PasswordResetTokens
                .Include(rt => rt.User)
                .FirstOrDefaultAsync(rt => rt.Token == dto.Token && rt.IsValid);

            if (resetToken == null)
            {
                return false;
            }

            resetToken.User.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.NewPassword);
            resetToken.IsUsed = true;

            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<List<StudentDto>> GetStudentsAsync()
        {
            // Lấy danh sách học sinh từ database
            var students = await _context.Users
                .Where(u => u.Role == UserRole.Student)
                .Select(u => new StudentDto
                {
                    Id = u.Id,
                    Email = u.Email,
                    FullName = u.FullName,
                    Grade = Grade.Grade10, // Default grade, sẽ được cập nhật sau
                    CreatedAt = u.CreatedAt,
                    LastLoginAt = u.LastLoginAt,
                    IsActive = u.IsActive
                })
                .ToListAsync();

            // Cập nhật Grade từ course registrations
            foreach (var student in students)
            {
                student.Grade = await GetStudentGradeFromDatabase(student.Id);
            }

            return students;
        }

        public async Task<List<StudentDto>> GetRegisteredStudentsAsync()
        {
            // Lấy học viên đã đăng ký ít nhất 1 lớp học
            var students = await _context.Users
                .Where(u => u.Role == UserRole.Student && u.CourseRegistrations.Any())
                .Select(u => new StudentDto
                {
                    Id = u.Id,
                    Email = u.Email,
                    FullName = u.FullName,
                    Grade = u.Grade ?? Grade.Grade10,
                    CreatedAt = u.CreatedAt,
                    LastLoginAt = u.LastLoginAt,
                    IsActive = u.IsActive
                })
                .ToListAsync();

            // Cập nhật Grade từ course registrations
            foreach (var student in students)
            {
                student.Grade = await GetStudentGradeFromDatabase(student.Id);
            }

            return students;
        }

        public async Task<bool> DeleteStudentAsync(int studentId)
        {
            var student = await _context.Users
                .Include(u => u.CourseRegistrations)
                .Include(u => u.Attendances)
                .Include(u => u.AssignmentSubmissions)
                .Include(u => u.FinalGrades)
                .FirstOrDefaultAsync(u => u.Id == studentId && u.Role == UserRole.Student);

            if (student == null)
            {
                return false;
            }

            _context.Users.Remove(student);
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<bool> UpdateLastLoginAsync(int userId)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null)
            {
                return false;
            }

            user.LastLoginAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<bool> IsStudentInactiveAsync(int studentId)
        {
            var student = await _context.Users.FindAsync(studentId);
            if (student == null || student.LastLoginAt == null)
            {
                return true;
            }

            return DateTime.UtcNow.Subtract(student.LastLoginAt.Value).TotalDays > 30;
        }

        public async Task<User?> GetUserByIdAsync(int id)
        {
            return await _context.Users.FindAsync(id);
        }

        public async Task<StudentDto?> GetStudentByIdAsync(int id)
        {
            var user = await _context.Users
                .Where(u => u.Id == id && u.Role == UserRole.Student)
                .FirstOrDefaultAsync();

            if (user == null) return null;

            var grade = await GetStudentGradeFromDatabase(id);

            return new StudentDto
            {
                Id = user.Id,
                Email = user.Email,
                FullName = user.FullName,
                Grade = grade,
                CreatedAt = user.CreatedAt,
                LastLoginAt = user.LastLoginAt,
                IsActive = user.IsActive
            };
        }

        public async Task<bool> ToggleStudentStatusAsync(int studentId)
        {
            var student = await _context.Users
                .Where(u => u.Id == studentId && u.Role == UserRole.Student)
                .FirstOrDefaultAsync();

            if (student == null) return false;

            var wasActive = student.IsActive;
            student.IsActive = !student.IsActive;
            await _context.SaveChangesAsync();

            // Gửi thông báo cho học viên
            try
            {
                if (wasActive)
                {
                    // Tài khoản bị khóa
                    await _emailService.SendAccountLockedNotificationAsync(student.Email, student.FullName);
                }
                else
                {
                    // Tài khoản được mở khóa
                    await _emailService.SendAccountUnlockedNotificationAsync(student.Email, student.FullName);
                }
            }
            catch (Exception)
            {
                // Log lỗi gửi email nhưng không fail toàn bộ operation
                // Có thể thêm logging ở đây nếu cần
            }

            return true;
        }

        public async Task<bool> ChangePasswordAsync(int userId, ChangePasswordDto dto)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null) return false;

            // Kiểm tra mật khẩu hiện tại
            if (!BCrypt.Net.BCrypt.Verify(dto.CurrentPassword, user.PasswordHash))
            {
                return false;
            }

            // Cập nhật mật khẩu mới
            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.NewPassword);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> UpdateProfileAsync(int userId, UpdateProfileDto dto)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null) return false;

            // Kiểm tra email trùng lặp (trừ chính user hiện tại)
            var existingUser = await _context.Users
                .Where(u => u.Email == dto.Email && u.Id != userId)
                .FirstOrDefaultAsync();
            
            if (existingUser != null) return false;

            // Cập nhật thông tin
            user.FullName = dto.FullName;
            user.Email = dto.Email;
            user.Grade = dto.Grade;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> StudentChangePasswordAsync(int userId, StudentChangePasswordDto dto)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null) return false;

            // Kiểm tra mật khẩu hiện tại
            if (!BCrypt.Net.BCrypt.Verify(dto.CurrentPassword, user.PasswordHash))
            {
                return false;
            }

            // Cập nhật mật khẩu mới
            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.NewPassword);
            await _context.SaveChangesAsync();
            return true;
        }

        private async Task<Grade> GetStudentGradeFromDatabase(int studentId)
        {
            // Lấy grade từ course registration đầu tiên của học sinh
            var registration = await _context.CourseRegistrations
                .Include(cr => cr.Course)
                .Where(cr => cr.StudentId == studentId && cr.Status == RegistrationStatus.Approved)
                .FirstOrDefaultAsync();

            return registration?.Course?.Grade ?? Grade.Grade10; // Default grade nếu không tìm thấy
        }
    }
}

