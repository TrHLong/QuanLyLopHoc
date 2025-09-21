using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using ClassManagement.Application.Interfaces;
using ClassManagement.Domain.Enums;

namespace ClassManagement.Web.Controllers
{
    public abstract class BaseController : Controller
    {
        protected readonly ICourseRegistrationService _courseRegistrationService;

        protected BaseController(ICourseRegistrationService courseRegistrationService)
        {
            _courseRegistrationService = courseRegistrationService;
        }

        protected async Task<bool> CheckStudentApprovedRegistrationAsync()
        {
            var userId = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userId))
            {
                return false;
            }

            var studentId = int.Parse(userId);
            var registrations = await _courseRegistrationService.GetStudentRegistrationsAsync(studentId);
            return registrations.Any(r => r.Status == RegistrationStatus.Approved);
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            base.OnActionExecuting(context);
            
            // Kiểm tra nếu là học viên
            var userRole = HttpContext.Session.GetString("UserRole");
            if (userRole == "Student")
            {
                // Set ViewBag để layout có thể sử dụng
                ViewBag.HasApprovedRegistration = CheckStudentApprovedRegistrationAsync().Result;
            }
        }
    }
}
