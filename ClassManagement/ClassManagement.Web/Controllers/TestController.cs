using Microsoft.AspNetCore.Mvc;
using ClassManagement.Application.DTOs;
using ClassManagement.Application.Interfaces;
using ClassManagement.Domain.Enums;

namespace ClassManagement.Web.Controllers
{
    public class TestController : Controller
    {
        private readonly IAssignmentService _assignmentService;
        private readonly ICourseService _courseService;

        public TestController(IAssignmentService assignmentService, ICourseService courseService)
        {
            _assignmentService = assignmentService;
            _courseService = courseService;
        }

        [HttpGet]
        public async Task<IActionResult> CreateTestAssignment()
        {
            // Lấy course đầu tiên để test
            var courses = await _courseService.GetCoursesAsync();
            if (!courses.Any())
            {
                return Content("Không có course nào để test");
            }

            var course = courses.First();
            
            var dto = new CreateAssignmentDto
            {
                CourseId = course.Id,
                Title = "Bài tập test - Toán học",
                Description = "Đây là bài tập test để kiểm tra chức năng hiển thị bài tập cho học viên. Hãy làm bài tập này một cách cẩn thận.",
                FileUrl = "/uploads/assignments/test-assignment.pdf", // File test
                DueDate = DateTime.UtcNow.AddDays(7)
            };

            var assignment = await _assignmentService.CreateAssignmentAsync(dto);
            if (assignment != null)
            {
                return Content($"Tạo bài tập test thành công! Assignment ID: {assignment.Id}");
            }
            else
            {
                return Content("Không thể tạo bài tập test");
            }
        }

        [HttpGet]
        public async Task<IActionResult> CheckAssignments(int studentId)
        {
            var assignments = await _assignmentService.GetStudentAssignmentsAsync(studentId);
            var result = $"Số bài tập cho student {studentId}: {assignments.Count}\n";
            
            foreach (var assignment in assignments)
            {
                result += $"ID: {assignment.Id}, Title: {assignment.Title}, FileUrl: {assignment.FileUrl ?? "null"}\n";
            }
            
            return Content(result);
        }

        [HttpGet]
        public async Task<IActionResult> DebugAssignment(int assignmentId)
        {
            var assignment = await _assignmentService.GetAssignmentByIdAsync(assignmentId);
            if (assignment == null)
            {
                return Content("Assignment không tồn tại");
            }

            var result = $"Assignment ID: {assignment.Id}\n";
            result += $"Title: {assignment.Title}\n";
            result += $"Description: {assignment.Description}\n";
            result += $"FileUrl: {assignment.FileUrl ?? "NULL"}\n";
            result += $"DueDate: {assignment.DueDate}\n";
            result += $"CourseId: {assignment.CourseId}\n";

            // Kiểm tra file có tồn tại không
            if (!string.IsNullOrEmpty(assignment.FileUrl))
            {
                var filePath = Path.Combine("wwwroot", assignment.FileUrl.TrimStart('/'));
                var fileExists = System.IO.File.Exists(filePath);
                result += $"File exists: {fileExists}\n";
                result += $"File path: {filePath}\n";
            }

            return Content(result);
        }

        [HttpGet]
        public IActionResult TestFileUpload()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> TestFileUpload(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                return Content("Không có file được upload");
            }

            try
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
                
                var fileUrl = $"/uploads/assignments/{fileName}";
                
                return Content($"File upload thành công!\nFileName: {fileName}\nFilePath: {filePath}\nFileUrl: {fileUrl}\nFileSize: {file.Length} bytes");
            }
            catch (Exception ex)
            {
                return Content($"Lỗi upload file: {ex.Message}");
            }
        }
    }
}
