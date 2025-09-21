using Microsoft.AspNetCore.Mvc;
using ClassManagement.Application.DTOs;
using ClassManagement.Application.Interfaces;
using ClassManagement.Domain.Enums;
using ClassManagement.Domain.Entities;

namespace ClassManagement.Web.Controllers
{
    public class StudentDashboardController : BaseController
    {
        private readonly IAssignmentService _assignmentService;
        private readonly IAttendanceService _attendanceService;
        private readonly IFinalGradeService _finalGradeService;

        public StudentDashboardController(
            ICourseRegistrationService courseRegistrationService,
            IAssignmentService assignmentService,
            IAttendanceService attendanceService,
            IFinalGradeService finalGradeService) : base(courseRegistrationService)
        {
            _assignmentService = assignmentService;
            _attendanceService = attendanceService;
            _finalGradeService = finalGradeService;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var userId = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Login", "Auth");
            }

            var studentId = int.Parse(userId);
            var registrations = await base._courseRegistrationService.GetStudentRegistrationsAsync(studentId);
            
            // Chỉ hiển thị lớp đã được phê duyệt
            var approvedRegistration = registrations.FirstOrDefault(r => r.Status == RegistrationStatus.Approved);
            
            if (approvedRegistration == null)
            {
                TempData["InfoMessage"] = "Bạn chưa có lớp học nào được phê duyệt.";
                return RedirectToAction("Index", "Home");
            }

            // Lấy thông tin lớp học
            var courseId = approvedRegistration.CourseId;
            var assignments = await _assignmentService.GetStudentAssignmentsAsync(studentId);
            var submissions = await _assignmentService.GetStudentSubmissionsAsync(studentId);
            var attendances = await _attendanceService.GetStudentAttendanceAsync(studentId);
            var finalGrade = await _finalGradeService.GetStudentFinalGradeAsync(studentId, courseId);

            var viewModel = new StudentDashboardViewModel
            {
                ApprovedRegistration = approvedRegistration,
                Assignments = assignments,
                Submissions = submissions,
                Attendances = attendances,
                FinalGrade = finalGrade
            };

            return View(viewModel);
        }

        [HttpGet]
        public async Task<IActionResult> Attendance()
        {
            var userId = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Login", "Auth");
            }

            var studentId = int.Parse(userId);
            var attendances = await _attendanceService.GetStudentAttendanceAsync(studentId);
            
            return View(attendances);
        }

        [HttpGet]
        public async Task<IActionResult> Assignments()
        {
            var userId = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Login", "Auth");
            }

            var studentId = int.Parse(userId);
            var assignments = await _assignmentService.GetStudentAssignmentsAsync(studentId);
            var submissions = await _assignmentService.GetStudentSubmissionsAsync(studentId);
            
            // Đảm bảo không null và sắp xếp
            assignments ??= new List<AssignmentDto>();
            submissions ??= new List<AssignmentSubmissionDto>();
            
            var viewModel = new StudentAssignmentsViewModel
            {
                Assignments = assignments.OrderByDescending(a => a.CreatedAt).ToList(),
                Submissions = submissions
            };

            return View(viewModel);
        }

        [HttpGet]
        public async Task<IActionResult> AssignmentsSimple()
        {
            var userId = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Login", "Auth");
            }

            var studentId = int.Parse(userId);
            var assignments = await _assignmentService.GetStudentAssignmentsAsync(studentId);
            var submissions = await _assignmentService.GetStudentSubmissionsAsync(studentId);
            
            // Đảm bảo không null và sắp xếp
            assignments ??= new List<AssignmentDto>();
            submissions ??= new List<AssignmentSubmissionDto>();
            
            var viewModel = new StudentAssignmentsViewModel
            {
                Assignments = assignments.OrderByDescending(a => a.CreatedAt).ToList(),
                Submissions = submissions
            };

            return View("AssignmentsSimple", viewModel);
        }

        [HttpGet]
        public async Task<IActionResult> Grades()
        {
            var userId = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Login", "Auth");
            }

            var studentId = int.Parse(userId);
            var submissions = await _assignmentService.GetStudentSubmissionsAsync(studentId);
            
            // Lấy thông tin lớp học để lấy final grade
            var registrations = await base._courseRegistrationService.GetStudentRegistrationsAsync(studentId);
            var approvedRegistration = registrations.FirstOrDefault(r => r.Status == RegistrationStatus.Approved);
            
            decimal? finalGrade = null;
            if (approvedRegistration != null)
            {
                var finalGradeEntity = await _finalGradeService.GetStudentFinalGradeAsync(studentId, approvedRegistration.CourseId);
                finalGrade = finalGradeEntity?.Grade;
            }

            var viewModel = new StudentGradesViewModel
            {
                AssignmentGrades = submissions.Where(s => s.IsGraded).ToList(),
                FinalGrade = finalGrade
            };

            return View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> SubmitAssignment(SubmitAssignmentDto dto)
        {
            var userId = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Login", "Auth");
            }

            var studentId = int.Parse(userId);
            var result = await _assignmentService.SubmitAssignmentAsync(studentId, dto);
            
            if (result != null)
            {
                TempData["SuccessMessage"] = "Nộp bài tập thành công!";
            }
            else
            {
                TempData["ErrorMessage"] = "Có lỗi xảy ra khi nộp bài tập.";
            }

            return RedirectToAction("Assignments");
        }
    }

    public class StudentDashboardViewModel
    {
        public CourseRegistrationDto ApprovedRegistration { get; set; } = null!;
        public List<AssignmentDto> Assignments { get; set; } = new List<AssignmentDto>();
        public List<AssignmentSubmissionDto> Submissions { get; set; } = new List<AssignmentSubmissionDto>();
        public List<AttendanceDto> Attendances { get; set; } = new List<AttendanceDto>();
        public FinalGrade? FinalGrade { get; set; }
    }

    public class StudentAssignmentsViewModel
    {
        public List<AssignmentDto> Assignments { get; set; } = new List<AssignmentDto>();
        public List<AssignmentSubmissionDto> Submissions { get; set; } = new List<AssignmentSubmissionDto>();
    }

    public class StudentGradesViewModel
    {
        public List<AssignmentSubmissionDto> AssignmentGrades { get; set; } = new List<AssignmentSubmissionDto>();
        public decimal? FinalGrade { get; set; }
    }
}
