using Microsoft.AspNetCore.Mvc;
using ClassManagement.Application.DTOs;
using ClassManagement.Application.Interfaces;
using ClassManagement.Domain.Enums;

namespace ClassManagement.Web.Controllers
{
    public class AuthController : Controller
    {
        private readonly IUserService _userService;
        private readonly IEmailService _emailService;

        public AuthController(IUserService userService, IEmailService emailService)
        {
            _userService = userService;
            _emailService = emailService;
        }

        [HttpGet]
        public async Task<IActionResult> Login()
        {
            // Kiểm tra Remember Me cookie
            if (Request.Cookies["RememberMe"] != null && Request.Cookies["UserRole"] != null)
            {
                var userId = Request.Cookies["RememberMe"];
                var userRole = Request.Cookies["UserRole"];
                
                if (int.TryParse(userId, out int id))
                {
                    // Tự động đăng nhập từ cookie
                    var user = await _userService.GetUserByIdAsync(id);
                    if (user != null && user.IsActive)
                    {
                        HttpContext.Session.SetString("UserId", user.Id.ToString());
                        HttpContext.Session.SetString("UserRole", user.Role.ToString());
                        HttpContext.Session.SetString("UserName", user.FullName);
                        HttpContext.Session.SetString("UserEmail", user.Email);
                        HttpContext.Session.SetString("LastLogin", DateTime.UtcNow.ToString());
                        
                        return RedirectToAction("Index", "Home");
                    }
                    else
                    {
                        // Nếu tài khoản bị khóa, xóa cookie
                        Response.Cookies.Delete("RememberMe");
                        Response.Cookies.Delete("UserRole");
                    }
                }
            }
            
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginDto dto)
        {
            var loginResult = await _userService.LoginAsync(dto);
            
            if (loginResult.Result != LoginResult.Success)
            {
                ModelState.AddModelError("", loginResult.Message);
                return View(dto);
            }

            var user = loginResult.User!;

            // Lưu thông tin vào session
            HttpContext.Session.SetString("UserId", user.Id.ToString());
            HttpContext.Session.SetString("UserRole", user.Role.ToString());
            HttpContext.Session.SetString("UserName", user.FullName);
            HttpContext.Session.SetString("UserEmail", user.Email);
            HttpContext.Session.SetString("LastLogin", DateTime.UtcNow.ToString());

            // Nếu có Remember Me, tạo cookie persistent
            if (dto.RememberMe)
            {
                var cookieOptions = new CookieOptions
                {
                    Expires = DateTime.Now.AddDays(30), // 30 ngày
                    HttpOnly = true,
                    Secure = Request.IsHttps,
                    SameSite = SameSiteMode.Lax
                };
                
                Response.Cookies.Append("RememberMe", user.Id.ToString(), cookieOptions);
                Response.Cookies.Append("UserRole", user.Role.ToString(), cookieOptions);
            }

            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Register(RegisterStudentDto dto)
        {
            var user = await _userService.RegisterStudentAsync(dto);
            if (user == null)
            {
                ModelState.AddModelError("", "Email đã tồn tại hoặc có lỗi xảy ra");
                return View(dto);
            }

            // Gửi email xác nhận đăng ký
            try
            {
                await _emailService.SendRegistrationConfirmationEmailAsync(user.Email, user.FullName);
            }
            catch (Exception ex)
            {
                // Log lỗi nhưng không làm fail registration
                Console.WriteLine($"Lỗi gửi email xác nhận: {ex.Message}");
            }

            TempData["SuccessMessage"] = "Đăng ký thành công! Email xác nhận đã được gửi đến hộp thư của bạn.";
            return RedirectToAction("Login");
        }

        [HttpGet]
        public IActionResult ForgotPassword()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordDto dto)
        {
            var result = await _userService.ForgotPasswordAsync(dto);
            if (result)
            {
                TempData["SuccessMessage"] = "Email đặt lại mật khẩu đã được gửi";
            }
            else
            {
                TempData["ErrorMessage"] = "Email không tồn tại";
            }

            return View(dto);
        }

        [HttpGet]
        public IActionResult ResetPassword(string token)
        {
            var dto = new ResetPasswordDto { Token = token };
            return View(dto);
        }

        [HttpPost]
        public async Task<IActionResult> ResetPassword(ResetPasswordDto dto)
        {
            var result = await _userService.ResetPasswordAsync(dto);
            if (result)
            {
                TempData["SuccessMessage"] = "Đặt lại mật khẩu thành công";
                return RedirectToAction("Login");
            }
            else
            {
                TempData["ErrorMessage"] = "Token không hợp lệ hoặc đã hết hạn";
                return View(dto);
            }
        }

        [HttpPost]
        public IActionResult Logout()
        {
            // Xóa session
            HttpContext.Session.Clear();
            
            // Xóa Remember Me cookies
            Response.Cookies.Delete("RememberMe");
            Response.Cookies.Delete("UserRole");
            
            return RedirectToAction("Index", "Home");
        }
    }
}

