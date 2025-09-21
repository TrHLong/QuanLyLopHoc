using Microsoft.AspNetCore.Mvc;
using ClassManagement.Application.DTOs;
using ClassManagement.Application.Interfaces;
using ClassManagement.Domain.Enums;

namespace ClassManagement.Web.Controllers
{
    public class HomeController : BaseController
    {
        private readonly ICourseService _courseService;
        private readonly INotificationService _notificationService;

        public HomeController(ICourseService courseService, INotificationService notificationService, ICourseRegistrationService courseRegistrationService) 
            : base(courseRegistrationService)
        {
            _courseService = courseService;
            _notificationService = notificationService;
        }

        public async Task<IActionResult> Index()
        {
            var userId = HttpContext.Session.GetString("UserId");
            var userRole = HttpContext.Session.GetString("UserRole");
            var viewModel = new HomeViewModel();

            // Chỉ load dữ liệu nếu đã đăng nhập
            if (!string.IsNullOrEmpty(userId))
            {
                if (userRole == UserRole.Teacher.ToString())
                {
                    // Teacher dashboard
                    viewModel.Courses = await _courseService.GetCoursesAsync();
                    viewModel.Notifications = await _notificationService.GetUserNotificationsAsync(int.Parse(userId));
                }
                else if (userRole == UserRole.Student.ToString())
                {
                    // Student dashboard
                    viewModel.Courses = await _courseService.GetCoursesAsync();
                    viewModel.Notifications = await _notificationService.GetUserNotificationsAsync(int.Parse(userId));
                }
            }

            return View(viewModel);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new Models.ErrorViewModel { RequestId = HttpContext.TraceIdentifier });
        }
    }

    public class HomeViewModel
    {
        public List<CourseDto> Courses { get; set; } = new List<CourseDto>();
        public List<NotificationDto> Notifications { get; set; } = new List<NotificationDto>();
    }
}
