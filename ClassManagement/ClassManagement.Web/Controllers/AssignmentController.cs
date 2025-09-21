using Microsoft.AspNetCore.Mvc;
using ClassManagement.Application.DTOs;
using ClassManagement.Application.Interfaces;

namespace ClassManagement.Web.Controllers
{
    public class AssignmentController : Controller
    {
        private readonly IAssignmentService _assignmentService;

        public AssignmentController(IAssignmentService assignmentService)
        {
            _assignmentService = assignmentService;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var userId = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Login", "Auth");
            }

            var assignments = await _assignmentService.GetStudentAssignmentsAsync(int.Parse(userId));
            return View(assignments);
        }

        [HttpGet]
        public IActionResult Submit(int assignmentId)
        {
            var dto = new SubmitAssignmentDto { AssignmentId = assignmentId };
            return View(dto);
        }

        [HttpPost]
        public async Task<IActionResult> Submit(SubmitAssignmentDto dto)
        {
            var userId = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Login", "Auth");
            }

            var submission = await _assignmentService.SubmitAssignmentAsync(int.Parse(userId), dto);
            if (submission == null)
            {
                TempData["ErrorMessage"] = "Không thể nộp bài. Có thể bạn đã nộp rồi.";
            }
            else
            {
                TempData["SuccessMessage"] = "Nộp bài thành công";
            }

            return RedirectToAction("Index");
        }
    }
}




