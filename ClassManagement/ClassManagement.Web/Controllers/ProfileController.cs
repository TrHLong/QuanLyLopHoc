using ClassManagement.Application.DTOs;
using ClassManagement.Application.Interfaces;
using ClassManagement.Domain.Enums;
using Microsoft.AspNetCore.Mvc;

namespace ClassManagement.Web.Controllers
{
    public class ProfileController : Controller
    {
        private readonly IUserService _userService;
        private readonly IEmailService _emailService;

        public ProfileController(IUserService userService, IEmailService emailService)
        {
            _userService = userService;
            _emailService = emailService;
        }

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

            var profileDto = new UpdateProfileDto
            {
                FullName = user.FullName,
                Email = user.Email,
                Grade = user.Grade
            };

            return View(profileDto);
        }

        [HttpPost]
        public async Task<IActionResult> UpdateProfile(UpdateProfileDto dto)
        {
            var userId = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Login", "Auth");
            }

            if (!ModelState.IsValid)
            {
                return View("Index", dto);
            }

            try
            {
                var success = await _userService.UpdateProfileAsync(int.Parse(userId), dto);
                if (success)
                {
                    TempData["SuccessMessage"] = "Cập nhật hồ sơ thành công!";
                    return RedirectToAction("Index");
                }
                else
                {
                    ModelState.AddModelError("", "Email đã được sử dụng bởi tài khoản khác.");
                    return View("Index", dto);
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Có lỗi xảy ra khi cập nhật hồ sơ: " + ex.Message);
                return View("Index", dto);
            }
        }

        [HttpGet]
        public IActionResult ChangePassword()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ChangePassword(StudentChangePasswordDto dto)
        {
            var userId = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Login", "Auth");
            }

            if (!ModelState.IsValid)
            {
                return View(dto);
            }

            try
            {
                var success = await _userService.StudentChangePasswordAsync(int.Parse(userId), dto);
                if (success)
                {
                    // Lấy thông tin user để gửi email
                    var user = await _userService.GetUserByIdAsync(int.Parse(userId));
                    if (user != null)
                    {
                        // Gửi email xác nhận đổi mật khẩu
                        try
                        {
                            await _emailService.SendPasswordChangeConfirmationEmailAsync(user.Email, user.FullName);
                        }
                        catch (Exception ex)
                        {
                            // Log lỗi nhưng không làm fail password change
                            Console.WriteLine($"Lỗi gửi email xác nhận: {ex.Message}");
                        }
                    }

                    TempData["SuccessMessage"] = "Đổi mật khẩu thành công! Email xác nhận đã được gửi đến hộp thư của bạn.";
                    return RedirectToAction("Index");
                }
                else
                {
                    ModelState.AddModelError("", "Mật khẩu hiện tại không đúng.");
                    return View(dto);
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Có lỗi xảy ra khi đổi mật khẩu: " + ex.Message);
                return View(dto);
            }
        }
    }
}
