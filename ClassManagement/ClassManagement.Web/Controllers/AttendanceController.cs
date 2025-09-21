using Microsoft.AspNetCore.Mvc;
using ClassManagement.Application.DTOs;
using ClassManagement.Application.Interfaces;
using ClassManagement.Domain.Enums;

namespace ClassManagement.Web.Controllers
{
    public class AttendanceController : Controller
    {
        private readonly IAttendanceService _attendanceService;
        private readonly ICourseService _courseService;

        public AttendanceController(IAttendanceService attendanceService, ICourseService courseService)
        {
            _attendanceService = attendanceService;
            _courseService = courseService;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var userId = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Login", "Auth");
            }

            var attendances = await _attendanceService.GetStudentAttendanceAsync(int.Parse(userId));
            return View(attendances);
        }

        [HttpGet]
        public async Task<IActionResult> Mark(int courseId)
        {
            var course = await _courseService.GetCourseByIdAsync(courseId);
            if (course == null)
            {
                return NotFound();
            }

            // Kiểm tra xem hôm nay có phải ngày học không
            if (!course.IsTodayClassDay())
            {
                TempData["ErrorMessage"] = $"Hôm nay không phải ngày học của lớp này. Buổi học tiếp theo: {course.GetNextClassDay()}";
                return RedirectToAction("Index", "Course");
            }

            // Kiểm tra xem có đúng giờ học không (cho phép điểm danh trong khoảng thời gian)
            var now = DateTime.Now;
            var isClassTime = course.IsClassTime(course.TimeSlot);
            
            // Cho phép điểm danh trước giờ học 30 phút và sau giờ học 30 phút
            var allowAttendance = false;
            var timeSlot = course.TimeSlot;
            
            if (timeSlot == TimeSlot.Slot1) // 14-16h
            {
                allowAttendance = now.Hour >= 13 && now.Hour <= 16; // 13h30 - 16h30
            }
            else if (timeSlot == TimeSlot.Slot2) // 17-19h
            {
                allowAttendance = now.Hour >= 16 && now.Hour <= 19; // 16h30 - 19h30
            }

            if (!allowAttendance)
            {
                var timeSlotText = timeSlot == TimeSlot.Slot1 ? "14:00-16:00" : "17:00-19:00";
                TempData["ErrorMessage"] = $"Chưa đến giờ điểm danh. Giờ học: {timeSlotText}. Buổi học tiếp theo: {course.GetNextClassDay()}";
                return RedirectToAction("Index", "Course");
            }

            // Lấy danh sách học viên đã được duyệt trong lớp này
            var students = await _courseService.GetRegisteredStudentsAsync(courseId);
            
            var viewModel = new MarkAttendanceViewModel
            {
                CourseId = courseId,
                CourseName = $"Khối {course.Grade} - Ca {course.TimeSlot}",
                Date = DateTime.Today,
                TimeSlot = course.TimeSlot,
                Students = students.Select(s => new StudentAttendanceViewModel
                {
                    StudentId = s.Id,
                    StudentName = s.FullName,
                    StudentEmail = s.Email,
                    Status = Domain.Enums.AttendanceStatus.Present // Mặc định là có mặt
                }).ToList()
            };

            return View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> Mark(MarkAttendanceDto dto)
        {
            var result = await _attendanceService.MarkAttendanceAsync(dto);
            if (result)
            {
                TempData["SuccessMessage"] = "Điểm danh thành công";
            }
            else
            {
                TempData["ErrorMessage"] = "Có lỗi xảy ra khi điểm danh";
            }

            return RedirectToAction("Mark", new { courseId = dto.CourseId });
        }

        [HttpGet]
        public async Task<IActionResult> Report(int courseId)
        {
            var absentStudents = await _attendanceService.GetConsecutiveAbsentStudentsAsync(courseId);
            return View(absentStudents);
        }
    }

    public class MarkAttendanceViewModel
    {
        public int CourseId { get; set; }
        public string CourseName { get; set; } = string.Empty;
        public DateTime Date { get; set; }
        public TimeSlot TimeSlot { get; set; }
        public List<StudentAttendanceViewModel> Students { get; set; } = new List<StudentAttendanceViewModel>();
    }
}



