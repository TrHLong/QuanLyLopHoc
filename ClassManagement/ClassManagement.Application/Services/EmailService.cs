using ClassManagement.Application.DTOs;
using ClassManagement.Application.Interfaces;
using Microsoft.Extensions.Configuration;
using System.Net;
using System.Net.Mail;

namespace ClassManagement.Application.Services
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;

        public EmailService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task<bool> SendEmailAsync(string to, string subject, string body)
        {
            try
            {
                var emailSettings = _configuration.GetSection("EmailSettings");
                var smtpHost = emailSettings["SmtpHost"];
                var smtpPort = int.Parse(emailSettings["SmtpPort"]!);
                var enableSsl = bool.Parse(emailSettings["EnableSsl"]!);
                var username = emailSettings["Username"];
                var password = emailSettings["Password"];
                var fromEmail = emailSettings["FromEmail"];
                var fromName = emailSettings["FromName"];

                using var client = new SmtpClient(smtpHost, smtpPort);
                client.EnableSsl = enableSsl;
                client.Credentials = new NetworkCredential(username, password);

                using var message = new MailMessage();
                message.From = new MailAddress(fromEmail!, fromName);
                message.To.Add(to);
                message.Subject = subject;
                message.Body = body;
                message.IsBodyHtml = true;

                await client.SendMailAsync(message);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> SendPasswordResetEmailAsync(string email, string resetToken)
        {
            var subject = "Đặt lại mật khẩu - ClassManagement";
            var body = $@"
                <h2>Đặt lại mật khẩu</h2>
                <p>Bạn đã yêu cầu đặt lại mật khẩu cho tài khoản ClassManagement.</p>
                <p>Vui lòng click vào link sau để đặt lại mật khẩu:</p>
                <a href='{resetToken}'>Đặt lại mật khẩu</a>
                <p>Link này sẽ hết hạn sau 24 giờ.</p>
                <p>Nếu bạn không yêu cầu đặt lại mật khẩu, vui lòng bỏ qua email này.</p>
            ";

            return await SendEmailAsync(email, subject, body);
        }

        public async Task<bool> SendRegistrationApprovalEmailAsync(string email, string courseInfo)
        {
            var subject = "Đăng ký khóa học thành công - ClassManagement";
            var body = $@"
                <h2>Đăng ký khóa học thành công</h2>
                <p>Chúc mừng! Đăng ký khóa học của bạn đã được duyệt.</p>
                <p><strong>Khóa học:</strong> {courseInfo}</p>
                <p>Vui lòng đăng nhập vào hệ thống để xem thông tin chi tiết.</p>
            ";

            return await SendEmailAsync(email, subject, body);
        }

        public async Task<bool> SendAssignmentDueReminderEmailAsync(string email, string assignmentTitle, DateTime dueDate)
        {
            var subject = "Nhắc nhở nộp bài tập - ClassManagement";
            var body = $@"
                <h2>Nhắc nhở nộp bài tập</h2>
                <p>Bạn có bài tập sắp đến hạn nộp:</p>
                <p><strong>Bài tập:</strong> {assignmentTitle}</p>
                <p><strong>Hạn nộp:</strong> {dueDate:dd/MM/yyyy HH:mm}</p>
                <p>Vui lòng đăng nhập vào hệ thống để nộp bài.</p>
            ";

            return await SendEmailAsync(email, subject, body);
        }

        public async Task<bool> SendCourseEndingReminderEmailAsync(string email, string courseInfo, DateTime endDate)
        {
            var subject = "Khóa học sắp kết thúc - ClassManagement";
            var body = $@"
                <h2>Khóa học sắp kết thúc</h2>
                <p>Khóa học của bạn sắp kết thúc:</p>
                <p><strong>Khóa học:</strong> {courseInfo}</p>
                <p><strong>Ngày kết thúc:</strong> {endDate:dd/MM/yyyy}</p>
                <p>Vui lòng đăng nhập vào hệ thống để xem thông tin chi tiết.</p>
            ";

            return await SendEmailAsync(email, subject, body);
        }

        public async Task<bool> SendRegistrationConfirmationEmailAsync(string email, string userName)
        {
            var subject = "Chào mừng bạn đến với ClassManagement!";
            var body = $@"
                <div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto;'>
                    <div style='background: linear-gradient(135deg, #667eea 0%, #764ba2 100%); color: white; padding: 30px; text-align: center; border-radius: 10px 10px 0 0;'>
                        <h1 style='margin: 0; font-size: 28px;'>🎓 ClassManagement</h1>
                        <p style='margin: 10px 0 0 0; font-size: 16px; opacity: 0.9;'>Trung Tâm Đào Tạo Thành Đạt</p>
                    </div>
                    
                    <div style='background: white; padding: 30px; border-radius: 0 0 10px 10px; box-shadow: 0 4px 6px rgba(0,0,0,0.1);'>
                        <h2 style='color: #333; margin-bottom: 20px;'>Chào mừng {userName}! 👋</h2>
                        
                        <p style='color: #666; line-height: 1.6; font-size: 16px;'>
                            Cảm ơn bạn đã đăng ký tài khoản tại <strong>ClassManagement</strong> - hệ thống quản lý lớp học của Trung Tâm Đào Tạo Thành Đạt.
                        </p>
                        
                        <div style='background: #f8f9fa; padding: 20px; border-radius: 8px; margin: 20px 0; border-left: 4px solid #667eea;'>
                            <h3 style='color: #333; margin-top: 0;'>📋 Thông tin tài khoản:</h3>
                            <p style='margin: 5px 0; color: #666;'><strong>Email:</strong> {email}</p>
                            <p style='margin: 5px 0; color: #666;'><strong>Tên người dùng:</strong> {userName}</p>
                            <p style='margin: 5px 0; color: #666;'><strong>Ngày đăng ký:</strong> {DateTime.Now:dd/MM/yyyy HH:mm}</p>
                        </div>
                        
                        <div style='background: #e8f5e8; padding: 20px; border-radius: 8px; margin: 20px 0; border-left: 4px solid #28a745;'>
                            <h3 style='color: #155724; margin-top: 0;'>✅ Tài khoản đã được kích hoạt!</h3>
                            <p style='color: #155724; margin: 0;'>Bạn có thể đăng nhập ngay bây giờ và bắt đầu sử dụng hệ thống.</p>
                        </div>
                        
                        <div style='text-align: center; margin: 30px 0;'>
                            <a href='#' style='background: linear-gradient(135deg, #667eea 0%, #764ba2 100%); color: white; padding: 15px 30px; text-decoration: none; border-radius: 25px; font-weight: bold; display: inline-block;'>
                                🚀 Đăng nhập ngay
                            </a>
                        </div>
                        
                        <div style='border-top: 1px solid #eee; padding-top: 20px; margin-top: 30px;'>
                            <p style='color: #999; font-size: 14px; text-align: center; margin: 0;'>
                                Nếu bạn có bất kỳ câu hỏi nào, vui lòng liên hệ với chúng tôi.<br>
                                <strong>Trung Tâm Đào Tạo Thành Đạt</strong>
                            </p>
                        </div>
                    </div>
                </div>
            ";

            return await SendEmailAsync(email, subject, body);
        }

        public async Task<bool> SendPasswordChangeConfirmationEmailAsync(string email, string userName)
        {
            var subject = "Xác nhận thay đổi mật khẩu - ClassManagement";
            var body = $@"
                <div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto;'>
                    <div style='background: linear-gradient(135deg, #667eea 0%, #764ba2 100%); color: white; padding: 30px; text-align: center; border-radius: 10px 10px 0 0;'>
                        <h1 style='margin: 0; font-size: 28px;'>🔐 ClassManagement</h1>
                        <p style='margin: 10px 0 0 0; font-size: 16px; opacity: 0.9;'>Trung Tâm Đào Tạo Thành Đạt</p>
                    </div>
                    
                    <div style='background: white; padding: 30px; border-radius: 0 0 10px 10px; box-shadow: 0 4px 6px rgba(0,0,0,0.1);'>
                        <h2 style='color: #333; margin-bottom: 20px;'>Mật khẩu đã được thay đổi thành công! ✅</h2>
                        
                        <p style='color: #666; line-height: 1.6; font-size: 16px;'>
                            Xin chào <strong>{userName}</strong>,
                        </p>
                        
                        <p style='color: #666; line-height: 1.6; font-size: 16px;'>
                            Chúng tôi xác nhận rằng mật khẩu tài khoản của bạn đã được thay đổi thành công vào lúc <strong>{DateTime.Now:dd/MM/yyyy 'lúc' HH:mm}</strong>.
                        </p>
                        
                        <div style='background: #fff3cd; padding: 20px; border-radius: 8px; margin: 20px 0; border-left: 4px solid #ffc107;'>
                            <h3 style='color: #856404; margin-top: 0;'>⚠️ Thông tin bảo mật:</h3>
                            <ul style='color: #856404; margin: 0; padding-left: 20px;'>
                                <li>Nếu bạn không thực hiện thay đổi này, vui lòng liên hệ với chúng tôi ngay lập tức</li>
                                <li>Để bảo mật tài khoản, hãy sử dụng mật khẩu mạnh và không chia sẻ với ai khác</li>
                                <li>Thay đổi mật khẩu định kỳ để tăng cường bảo mật</li>
                            </ul>
                        </div>
                        
                        <div style='background: #d4edda; padding: 20px; border-radius: 8px; margin: 20px 0; border-left: 4px solid #28a745;'>
                            <h3 style='color: #155724; margin-top: 0;'>✅ Tài khoản của bạn an toàn</h3>
                            <p style='color: #155724; margin: 0;'>Mật khẩu mới đã được áp dụng và bạn có thể tiếp tục sử dụng hệ thống bình thường.</p>
                        </div>
                        
                        <div style='text-align: center; margin: 30px 0;'>
                            <a href='#' style='background: linear-gradient(135deg, #667eea 0%, #764ba2 100%); color: white; padding: 15px 30px; text-decoration: none; border-radius: 25px; font-weight: bold; display: inline-block;'>
                                🔐 Đăng nhập với mật khẩu mới
                            </a>
                        </div>
                        
                        <div style='border-top: 1px solid #eee; padding-top: 20px; margin-top: 30px;'>
                            <p style='color: #999; font-size: 14px; text-align: center; margin: 0;'>
                                Nếu bạn có bất kỳ câu hỏi nào về bảo mật tài khoản, vui lòng liên hệ với chúng tôi.<br>
                                <strong>Trung Tâm Đào Tạo Thành Đạt</strong>
                            </p>
                        </div>
                    </div>
                </div>
            ";

            return await SendEmailAsync(email, subject, body);
        }

        public async Task<bool> SendAssignmentSubmissionConfirmationEmailAsync(string email, string studentName, string assignmentTitle, DateTime submittedAt, DateTime dueDate)
        {
            var isLate = submittedAt > dueDate;
            var timeStatus = isLate ? "trễ hạn" : "đúng hạn";
            var statusColor = isLate ? "#dc3545" : "#28a745";
            var statusIcon = isLate ? "⏰" : "✅";
            
            var subject = $"Xác nhận nộp bài tập - {assignmentTitle}";
            var body = $@"
                <div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto;'>
                    <div style='background: linear-gradient(135deg, #667eea 0%, #764ba2 100%); color: white; padding: 30px; text-align: center; border-radius: 10px 10px 0 0;'>
                        <h1 style='margin: 0; font-size: 28px;'>📝 ClassManagement</h1>
                        <p style='margin: 10px 0 0 0; font-size: 16px; opacity: 0.9;'>Trung Tâm Đào Tạo Thành Đạt</p>
                    </div>
                    
                    <div style='background: white; padding: 30px; border-radius: 0 0 10px 10px; box-shadow: 0 4px 6px rgba(0,0,0,0.1);'>
                        <h2 style='color: #333; margin-bottom: 20px;'>Xác nhận nộp bài tập thành công! {statusIcon}</h2>
                        
                        <p style='color: #666; line-height: 1.6; font-size: 16px;'>
                            Xin chào <strong>{studentName}</strong>,
                        </p>
                        
                        <p style='color: #666; line-height: 1.6; font-size: 16px;'>
                            Chúng tôi xác nhận rằng bạn đã nộp bài tập thành công vào lúc <strong>{submittedAt:dd/MM/yyyy 'lúc' HH:mm}</strong>.
                        </p>
                        
                        <div style='background: #f8f9fa; padding: 20px; border-radius: 8px; margin: 20px 0; border-left: 4px solid #667eea;'>
                            <h3 style='color: #333; margin-top: 0;'>📋 Thông tin bài tập:</h3>
                            <p style='margin: 5px 0; color: #666;'><strong>Tên bài tập:</strong> {assignmentTitle}</p>
                            <p style='margin: 5px 0; color: #666;'><strong>Thời gian nộp:</strong> {submittedAt:dd/MM/yyyy HH:mm}</p>
                            <p style='margin: 5px 0; color: #666;'><strong>Hạn nộp:</strong> {dueDate:dd/MM/yyyy HH:mm}</p>
                        </div>
                        
                        <div style='background: {(isLate ? "#f8d7da" : "#d4edda")}; padding: 20px; border-radius: 8px; margin: 20px 0; border-left: 4px solid {statusColor};'>
                            <h3 style='color: {(isLate ? "#721c24" : "#155724")}; margin-top: 0;'>{statusIcon} Trạng thái nộp bài:</h3>
                            <p style='color: {(isLate ? "#721c24" : "#155724")}; margin: 0; font-weight: bold;'>
                                {(isLate ? "Bài tập đã được nộp TRỄ HẠN. Vui lòng chú ý thời gian nộp bài trong tương lai." : "Bài tập đã được nộp ĐÚNG HẠN. Chúc mừng bạn!")}
                            </p>
                        </div>
                        
                        {(isLate ? @"
                        <div style='background: #fff3cd; padding: 20px; border-radius: 8px; margin: 20px 0; border-left: 4px solid #ffc107;'>
                            <h3 style='color: #856404; margin-top: 0;'>⚠️ Lưu ý:</h3>
                            <ul style='color: #856404; margin: 0; padding-left: 20px;'>
                                <li>Bài tập nộp trễ hạn có thể bị trừ điểm</li>
                                <li>Hãy chú ý thời gian hạn nộp bài trong tương lai</li>
                                <li>Liên hệ với giáo viên nếu có vấn đề về thời gian</li>
                            </ul>
                        </div>
                        " : @"
                        <div style='background: #d1ecf1; padding: 20px; border-radius: 8px; margin: 20px 0; border-left: 4px solid #17a2b8;'>
                            <h3 style='color: #0c5460; margin-top: 0;'>💡 Thông tin:</h3>
                            <ul style='color: #0c5460; margin: 0; padding-left: 20px;'>
                                <li>Bài tập đã được nộp thành công và đúng hạn</li>
                                <li>Giáo viên sẽ chấm bài và thông báo kết quả</li>
                                <li>Tiếp tục theo dõi các bài tập mới</li>
                            </ul>
                        </div>
                        ")}
                        
                        <div style='text-align: center; margin: 30px 0;'>
                            <a href='#' style='background: linear-gradient(135deg, #667eea 0%, #764ba2 100%); color: white; padding: 15px 30px; text-decoration: none; border-radius: 25px; font-weight: bold; display: inline-block;'>
                                📚 Xem bài tập khác
                            </a>
                        </div>
                        
                        <div style='border-top: 1px solid #eee; padding-top: 20px; margin-top: 30px;'>
                            <p style='color: #999; font-size: 14px; text-align: center; margin: 0;'>
                                Nếu bạn có bất kỳ câu hỏi nào về bài tập, vui lòng liên hệ với giáo viên.<br>
                                <strong>Trung Tâm Đào Tạo Thành Đạt</strong>
                            </p>
                        </div>
                    </div>
                </div>
            ";

            return await SendEmailAsync(email, subject, body);
        }

        public async Task<bool> SendAccountLockedNotificationAsync(string email, string userName)
        {
            var subject = "Tài khoản đã bị khóa - ClassManagement";
            var body = $@"
                <div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto;'>
                    <div style='background: linear-gradient(135deg, #dc3545 0%, #c82333 100%); color: white; padding: 30px; text-align: center; border-radius: 10px 10px 0 0;'>
                        <h1 style='margin: 0; font-size: 28px;'>🔒 ClassManagement</h1>
                        <p style='margin: 10px 0 0 0; font-size: 16px; opacity: 0.9;'>Trung Tâm Đào Tạo Thành Đạt</p>
                    </div>
                    
                    <div style='background: white; padding: 30px; border-radius: 0 0 10px 10px; box-shadow: 0 4px 6px rgba(0,0,0,0.1);'>
                        <h2 style='color: #333; margin-bottom: 20px;'>Tài khoản đã bị khóa ⚠️</h2>
                        
                        <p style='color: #666; line-height: 1.6; font-size: 16px;'>
                            Xin chào <strong>{userName}</strong>,
                        </p>
                        
                        <p style='color: #666; line-height: 1.6; font-size: 16px;'>
                            Chúng tôi thông báo rằng tài khoản của bạn đã bị khóa vào lúc <strong>{DateTime.Now:dd/MM/yyyy 'lúc' HH:mm}</strong>.
                        </p>
                        
                        <div style='background: #f8d7da; padding: 20px; border-radius: 8px; margin: 20px 0; border-left: 4px solid #dc3545;'>
                            <h3 style='color: #721c24; margin-top: 0;'>🚫 Tài khoản đã bị khóa</h3>
                            <p style='color: #721c24; margin: 0;'>Hiện tại bạn không thể đăng nhập vào hệ thống ClassManagement.</p>
                        </div>
                        
                        <div style='background: #fff3cd; padding: 20px; border-radius: 8px; margin: 20px 0; border-left: 4px solid #ffc107;'>
                            <h3 style='color: #856404; margin-top: 0;'>📞 Liên hệ để được hỗ trợ</h3>
                            <p style='color: #856404; margin: 0;'>
                                Để mở khóa tài khoản, vui lòng liên hệ trực tiếp với giáo viên hoặc trung tâm để được hỗ trợ.
                            </p>
                        </div>
                        
                        <div style='border-top: 1px solid #eee; padding-top: 20px; margin-top: 30px;'>
                            <p style='color: #999; font-size: 14px; text-align: center; margin: 0;'>
                                Nếu bạn có bất kỳ câu hỏi nào, vui lòng liên hệ với chúng tôi.<br>
                                <strong>Trung Tâm Đào Tạo Thành Đạt</strong>
                            </p>
                        </div>
                    </div>
                </div>
            ";

            return await SendEmailAsync(email, subject, body);
        }

        public async Task<bool> SendAccountUnlockedNotificationAsync(string email, string userName)
        {
            var subject = "Tài khoản đã được mở khóa - ClassManagement";
            var body = $@"
                <div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto;'>
                    <div style='background: linear-gradient(135deg, #28a745 0%, #20c997 100%); color: white; padding: 30px; text-align: center; border-radius: 10px 10px 0 0;'>
                        <h1 style='margin: 0; font-size: 28px;'>🔓 ClassManagement</h1>
                        <p style='margin: 10px 0 0 0; font-size: 16px; opacity: 0.9;'>Trung Tâm Đào Tạo Thành Đạt</p>
                    </div>
                    
                    <div style='background: white; padding: 30px; border-radius: 0 0 10px 10px; box-shadow: 0 4px 6px rgba(0,0,0,0.1);'>
                        <h2 style='color: #333; margin-bottom: 20px;'>Tài khoản đã được mở khóa! ✅</h2>
                        
                        <p style='color: #666; line-height: 1.6; font-size: 16px;'>
                            Xin chào <strong>{userName}</strong>,
                        </p>
                        
                        <p style='color: #666; line-height: 1.6; font-size: 16px;'>
                            Chúng tôi thông báo rằng tài khoản của bạn đã được mở khóa vào lúc <strong>{DateTime.Now:dd/MM/yyyy 'lúc' HH:mm}</strong>.
                        </p>
                        
                        <div style='background: #d4edda; padding: 20px; border-radius: 8px; margin: 20px 0; border-left: 4px solid #28a745;'>
                            <h3 style='color: #155724; margin-top: 0;'>✅ Tài khoản đã hoạt động trở lại</h3>
                            <p style='color: #155724; margin: 0;'>Bạn có thể đăng nhập vào hệ thống ClassManagement và tiếp tục học tập bình thường.</p>
                        </div>
                        
                        <div style='background: #d1ecf1; padding: 20px; border-radius: 8px; margin: 20px 0; border-left: 4px solid #17a2b8;'>
                            <h3 style='color: #0c5460; margin-top: 0;'>💡 Thông tin quan trọng:</h3>
                            <ul style='color: #0c5460; margin: 0; padding-left: 20px;'>
                                <li>Tài khoản của bạn đã được kích hoạt và sẵn sàng sử dụng</li>
                                <li>Bạn có thể đăng nhập và truy cập tất cả chức năng</li>
                                <li>Hãy tiếp tục theo dõi các bài tập và thông báo mới</li>
                            </ul>
                        </div>
                        
                        <div style='text-align: center; margin: 30px 0;'>
                            <a href='#' style='background: linear-gradient(135deg, #28a745 0%, #20c997 100%); color: white; padding: 15px 30px; text-decoration: none; border-radius: 25px; font-weight: bold; display: inline-block;'>
                                🚀 Đăng nhập ngay
                            </a>
                        </div>
                        
                        <div style='border-top: 1px solid #eee; padding-top: 20px; margin-top: 30px;'>
                            <p style='color: #999; font-size: 14px; text-align: center; margin: 0;'>
                                Nếu bạn có bất kỳ câu hỏi nào, vui lòng liên hệ với chúng tôi.<br>
                                <strong>Trung Tâm Đào Tạo Thành Đạt</strong>
                            </p>
                        </div>
                    </div>
                </div>
            ";

            return await SendEmailAsync(email, subject, body);
        }
    }
}
