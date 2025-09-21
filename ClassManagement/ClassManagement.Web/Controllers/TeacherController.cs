using Microsoft.AspNetCore.Mvc;
using ClassManagement.Application.DTOs;
using ClassManagement.Application.Interfaces;
using ClassManagement.Domain.Enums;

namespace ClassManagement.Web.Controllers
{
    public class TeacherController : Controller
    {
        private readonly IAssignmentService _assignmentService;
        private readonly IFinalGradeService _finalGradeService;
        private readonly ICourseService _courseService;
        private readonly ICourseRegistrationService _courseRegistrationService;

        public TeacherController(IAssignmentService assignmentService, IFinalGradeService finalGradeService, ICourseService courseService, ICourseRegistrationService courseRegistrationService)
        {
            _assignmentService = assignmentService;
            _finalGradeService = finalGradeService;
            _courseService = courseService;
            _courseRegistrationService = courseRegistrationService;
        }

        [HttpGet]
        public IActionResult CreateAssignment(int courseId)
        {
            var dto = new CreateAssignmentDto { CourseId = courseId };
            return View(dto);
        }

        [HttpPost]
        public async Task<IActionResult> CreateAssignment(CreateAssignmentDto dto, IFormFile file)
        {
            // Xử lý file upload nếu có
            if (file != null && file.Length > 0)
            {
                // Tạo tên file unique
                var fileName = $"{Guid.NewGuid()}_{file.FileName}";
                var filePath = Path.Combine("wwwroot", "uploads", "assignments", fileName);
                
                // Tạo thư mục nếu chưa có
                var directory = Path.GetDirectoryName(filePath);
                if (!Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory!);
                }
                
                // Lưu file
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }
                
                // Set FileUrl
                dto.FileUrl = $"/uploads/assignments/{fileName}";
            }

            var assignment = await _assignmentService.CreateAssignmentAsync(dto);
            if (assignment == null)
            {
                ModelState.AddModelError("", "Không thể tạo bài tập");
                return View(dto);
            }

            TempData["SuccessMessage"] = "Tạo bài tập thành công";
            return RedirectToAction("Details", "Course", new { id = dto.CourseId });
        }

        [HttpPost]
        public async Task<IActionResult> RemoveStudentFromCourse(int studentId, int courseId)
        {
            try
            {
                var result = await _courseRegistrationService.RemoveStudentFromCourseAsync(studentId, courseId);
                if (result)
                {
                    TempData["SuccessMessage"] = "Đã xóa học viên khỏi lớp học thành công";
                }
                else
                {
                    TempData["ErrorMessage"] = "Không thể xóa học viên khỏi lớp học";
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Lỗi: {ex.Message}";
            }

            return RedirectToAction("Details", "Course", new { id = courseId });
        }

        [HttpGet]
        public async Task<IActionResult> GradeAssignment(int assignmentId)
        {
            var submissions = await _assignmentService.GetAssignmentSubmissionsAsync(assignmentId);
            return View(submissions);
        }

        [HttpPost]
        public async Task<IActionResult> GradeAssignment(GradeAssignmentDto dto)
        {
            var result = await _assignmentService.GradeAssignmentAsync(dto);
            if (result)
            {
                TempData["SuccessMessage"] = "Chấm điểm thành công";
            }
            else
            {
                TempData["ErrorMessage"] = "Có lỗi xảy ra khi chấm điểm";
            }

            // Lấy AssignmentId từ submission để redirect
            var submission = await _assignmentService.GetSubmissionByIdAsync(dto.SubmissionId);
            return RedirectToAction("GradeAssignment", new { assignmentId = submission?.AssignmentId ?? 0 });
        }

        [HttpGet]
        public async Task<IActionResult> SetFinalGrade(int courseId)
        {
            var course = await _courseService.GetCourseByIdAsync(courseId);
            if (course == null)
            {
                return NotFound();
            }

            var finalGrades = await _finalGradeService.GetCourseFinalGradesAsync(courseId);
            return View(finalGrades);
        }

        [HttpPost]
        public async Task<IActionResult> SetFinalGrade(SetFinalGradeDto dto)
        {
            var result = await _finalGradeService.SetFinalGradeAsync(dto);
            if (result)
            {
                TempData["SuccessMessage"] = "Nhập điểm cuối khóa thành công";
            }
            else
            {
                TempData["ErrorMessage"] = "Có lỗi xảy ra khi nhập điểm";
            }

            return RedirectToAction("SetFinalGrade", new { courseId = dto.CourseId });
        }

        [HttpPost]
        public async Task<IActionResult> DeleteAssignment(int assignmentId)
        {
            var result = await _assignmentService.DeleteAssignmentAsync(assignmentId);
            if (result)
            {
                TempData["SuccessMessage"] = "Xóa bài tập thành công";
            }
            else
            {
                TempData["ErrorMessage"] = "Không thể xóa bài tập này";
            }

            // Redirect về trang trước đó hoặc danh sách lớp
            return RedirectToAction("Index", "Course");
        }
    }
}


