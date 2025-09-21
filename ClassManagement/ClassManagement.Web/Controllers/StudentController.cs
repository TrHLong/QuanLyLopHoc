using Microsoft.AspNetCore.Mvc;
using ClassManagement.Application.DTOs;
using ClassManagement.Application.Interfaces;

namespace ClassManagement.Web.Controllers
{
    public class StudentController : Controller
    {
        private readonly IUserService _userService;

        public StudentController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var students = await _userService.GetRegisteredStudentsAsync();
            return View(students);
        }

        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var student = await _userService.GetStudentByIdAsync(id);
            if (student == null)
            {
                return NotFound();
            }

            return PartialView("_StudentDetails", student);
        }

        [HttpPost]
        public async Task<IActionResult> ToggleStatus(int id)
        {
            try
            {
                var result = await _userService.ToggleStudentStatusAsync(id);
                if (result)
                {
                    TempData["SuccessMessage"] = "Thay đổi trạng thái học viên thành công";
                }
                else
                {
                    TempData["ErrorMessage"] = "Có lỗi xảy ra khi thay đổi trạng thái học viên";
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Lỗi: {ex.Message}";
            }

            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var result = await _userService.DeleteStudentAsync(id);
                if (result)
                {
                    TempData["SuccessMessage"] = "Xóa học viên thành công";
                }
                else
                {
                    TempData["ErrorMessage"] = "Có lỗi xảy ra khi xóa học viên";
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Lỗi: {ex.Message}";
            }

            return RedirectToAction("Index");
        }
    }
}

