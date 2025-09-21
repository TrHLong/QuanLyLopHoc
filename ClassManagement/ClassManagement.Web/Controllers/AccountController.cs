using Microsoft.AspNetCore.Mvc;
using ClassManagement.Application.DTOs;
using ClassManagement.Application.Interfaces;

namespace ClassManagement.Web.Controllers
{
    public class AccountController : Controller
    {
        private readonly IUserService _userService;

        public AccountController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var userId = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Login", "Auth");
            }

            var user = await _userService.GetUserByIdAsync(int.Parse(userId));
            if (user == null)
            {
                return RedirectToAction("Login", "Auth");
            }

            return View(user);
        }

        [HttpGet]
        public IActionResult ChangePassword()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ChangePassword(ChangePasswordDto dto)
        {
            if (!ModelState.IsValid)
            {
                return View(dto);
            }

            var userId = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Login", "Auth");
            }

            try
            {
                var result = await _userService.ChangePasswordAsync(int.Parse(userId), dto);
                if (result)
                {
                    TempData["SuccessMessage"] = "Đổi mật khẩu thành công";
                    return RedirectToAction("Index");
                }
                else
                {
                    ModelState.AddModelError("", "Mật khẩu hiện tại không đúng");
                    return View(dto);
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Lỗi: {ex.Message}");
                return View(dto);
            }
        }

    }
}
