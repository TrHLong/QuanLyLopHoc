using Microsoft.AspNetCore.Mvc;
using ClassManagement.Application.DTOs;
using ClassManagement.Application.Interfaces;
using ClassManagement.Domain.Enums;

namespace ClassManagement.Web.Controllers
{
    public class CourseController : BaseController
    {
        private readonly ICourseService _courseService;
        private readonly ICourseRegistrationService _registrationService;

        public CourseController(ICourseService courseService, ICourseRegistrationService registrationService) 
            : base(registrationService)
        {
            _courseService = courseService;
            _registrationService = registrationService;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var userRole = HttpContext.Session.GetString("UserRole");
            var userId = HttpContext.Session.GetString("UserId");
            
            List<CourseDto> courses;
            
            if (userRole == "Student" && !string.IsNullOrEmpty(userId))
            {
                var studentId = int.Parse(userId);
                
                // Kiểm tra xem học viên đã có lớp được phê duyệt chưa
                var hasApprovedRegistration = await CheckStudentApprovedRegistrationAsync();
                
                if (hasApprovedRegistration)
                {
                    // Nếu đã có lớp được phê duyệt, chỉ hiển thị lớp đó
                    var registrations = await _courseRegistrationService.GetStudentRegistrationsAsync(studentId);
                    var approvedRegistration = registrations.FirstOrDefault(r => r.Status == RegistrationStatus.Approved);
                    
                    if (approvedRegistration != null)
                    {
                        // Lấy thông tin lớp đã được phê duyệt
                        var allCourses = await _courseService.GetCoursesAsync();
                        var approvedCourse = allCourses.FirstOrDefault(c => c.Id == approvedRegistration.CourseId);
                        
                        if (approvedCourse != null)
                        {
                            courses = new List<CourseDto> { approvedCourse };
                        }
                        else
                        {
                            courses = new List<CourseDto>();
                        }
                    }
                    else
                    {
                        courses = new List<CourseDto>();
                    }
                }
                else
                {
                    // Nếu chưa có lớp được phê duyệt, hiển thị tất cả lớp để đăng ký
                    courses = await _courseService.GetCoursesForStudentAsync(studentId);
                }
            }
            else
            {
                // Giáo viên thấy tất cả khóa học
                courses = await _courseService.GetCoursesAsync();
            }
            
            return View(courses);
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreateCourseDto dto)
        {
            try
            {
                var course = await _courseService.CreateCourseAsync(dto);
                if (course == null)
                {
                    ModelState.AddModelError("", "Không thể tạo khóa học. Có thể khóa học đã tồn tại.");
                    return View(dto);
                }

                TempData["SuccessMessage"] = "Tạo khóa học thành công";
                return RedirectToAction("Index");
            }
            catch (InvalidOperationException ex)
            {
                ModelState.AddModelError("", ex.Message);
                return View(dto);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Có lỗi xảy ra khi tạo khóa học: " + ex.Message);
                return View(dto);
            }
        }

        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var course = await _courseService.GetCourseByIdAsync(id);
            if (course == null)
            {
                return NotFound();
            }

            return View(course);
        }

        [HttpGet]
        public async Task<IActionResult> Register(int courseId)
        {
            var userId = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Login", "Auth");
            }

            // Kiểm tra xem học viên đã có lớp được phê duyệt chưa
            var hasApprovedRegistration = await CheckStudentApprovedRegistrationAsync();
            if (hasApprovedRegistration)
            {
                TempData["ErrorMessage"] = "Bạn đã có lớp học được phê duyệt. Không thể đăng ký lớp khác.";
                return RedirectToAction("Index");
            }

            var registration = await _registrationService.RegisterForCourseAsync(int.Parse(userId), courseId);
            if (registration == null)
            {
                TempData["ErrorMessage"] = "Không thể đăng ký khóa học";
            }
            else
            {
                TempData["SuccessMessage"] = "Đăng ký khóa học thành công. Vui lòng chờ giáo viên duyệt.";
            }

            return RedirectToAction("Index");
        }

        [HttpGet]
        public async Task<IActionResult> Registrations()
        {
            var registrations = await _registrationService.GetPendingRegistrationsAsync();
            return View(registrations);
        }

        [HttpPost]
        public async Task<IActionResult> ApproveRegistration(int registrationId, bool approved, string? rejectionReason = null)
        {
            var result = await _registrationService.ApproveRegistrationAsync(registrationId, approved, rejectionReason);
            if (result)
            {
                TempData["SuccessMessage"] = approved ? "Duyệt đăng ký thành công" : "Từ chối đăng ký thành công";
            }
            else
            {
                TempData["ErrorMessage"] = "Có lỗi xảy ra";
            }

            return RedirectToAction("Registrations");
        }

        [HttpPost]
        public async Task<IActionResult> RemoveStudent(int courseId, int studentId)
        {
            var result = await _courseService.DeleteStudentFromCourseAsync(courseId, studentId);
            if (result)
            {
                TempData["SuccessMessage"] = "Xóa học viên khỏi khóa thành công";
            }
            else
            {
                TempData["ErrorMessage"] = "Có lỗi xảy ra";
            }

            return RedirectToAction("Details", new { id = courseId });
        }

        [HttpGet]
        public async Task<IActionResult> Export(int courseId)
        {
            var data = await _courseService.ExportCourseDataAsync(courseId);
            if (data.Length == 0)
            {
                TempData["ErrorMessage"] = "Không có dữ liệu để xuất";
                return RedirectToAction("Details", new { id = courseId });
            }

            var course = await _courseService.GetCourseByIdAsync(courseId);
            var fileName = $"Course_{course?.Grade}_{course?.TimeSlot}_{DateTime.Now:yyyyMMdd}.csv";

            return File(data, "text/csv", fileName);
        }

        [HttpGet]
        public async Task<IActionResult> AbsentReport(int courseId)
        {
            var absentStudents = await _courseService.GetAbsentStudentsAsync(courseId);
            return View(absentStudents);
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int courseId)
        {
            try
            {
                var result = await _courseService.DeleteCourseAsync(courseId);
                if (result)
                {
                    TempData["SuccessMessage"] = "Xóa khóa học thành công";
                }
                else
                {
                    TempData["ErrorMessage"] = "Không tìm thấy khóa học";
                }
            }
            catch (InvalidOperationException ex)
            {
                TempData["ErrorMessage"] = ex.Message;
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Có lỗi xảy ra khi xóa khóa học: " + ex.Message;
            }

            return RedirectToAction("Index");
        }
    }
}

