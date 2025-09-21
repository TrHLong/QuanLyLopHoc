using ClassManagement.Application.DTOs;
using ClassManagement.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace ClassManagement.Web.Controllers
{
    public class AccountManagementController : Controller
    {
        private readonly IUserService _userService;

        public AccountManagementController(IUserService userService)
        {
            _userService = userService;
        }

        public async Task<IActionResult> Index()
        {
            var userRole = HttpContext.Session.GetString("UserRole");
            if (userRole != "Teacher")
            {
                return RedirectToAction("Index", "Home");
            }

            var students = await _userService.GetStudentsAsync();
            return View(students);
        }

        [HttpPost]
        public async Task<IActionResult> ToggleAccountStatus(int id)
        {
            var userRole = HttpContext.Session.GetString("UserRole");
            if (userRole != "Teacher")
            {
                return Json(new { success = false, message = "Không có quyền thực hiện hành động này" });
            }

            try
            {
                var success = await _userService.ToggleStudentStatusAsync(id);
                if (success)
                {
                    return Json(new { success = true, message = "Thay đổi trạng thái tài khoản thành công" });
                }
                else
                {
                    return Json(new { success = false, message = "Không tìm thấy học viên" });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Có lỗi xảy ra: " + ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> DeleteAccount(int id)
        {
            var userRole = HttpContext.Session.GetString("UserRole");
            if (userRole != "Teacher")
            {
                return Json(new { success = false, message = "Không có quyền thực hiện hành động này" });
            }

            try
            {
                var success = await _userService.DeleteStudentAsync(id);
                if (success)
                {
                    return Json(new { success = true, message = "Xóa tài khoản thành công" });
                }
                else
                {
                    return Json(new { success = false, message = "Không thể xóa tài khoản này" });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Có lỗi xảy ra: " + ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> AccountDetails(int id)
        {
            var userRole = HttpContext.Session.GetString("UserRole");
            if (userRole != "Teacher")
            {
                return Json(new { success = false, message = "Không có quyền xem thông tin này" });
            }

            try
            {
                var student = await _userService.GetStudentByIdAsync(id);
                if (student != null)
                {
                    return PartialView("_AccountDetails", student);
                }
                else
                {
                    return Json(new { success = false, message = "Không tìm thấy học viên" });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Có lỗi xảy ra: " + ex.Message });
            }
        }
    }
}


