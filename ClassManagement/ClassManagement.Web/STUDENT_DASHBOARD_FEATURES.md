# Chức năng Student Dashboard

## Tổng quan

Hệ thống đã được cập nhật để cung cấp một bảng điều khiển chuyên dụng cho học viên sau khi đã được phê duyệt vào lớp học.

## Các chức năng chính

### 1. Student Dashboard (`/StudentDashboard`)

- **Mục đích**: Trang chính cho học viên đã được phê duyệt
- **Tính năng**:
  - Hiển thị thông tin lớp học đã được phê duyệt
  - Thống kê tổng quan (số bài tập, số bài đã nộp, điểm danh, điểm cuối kỳ)
  - Truy cập nhanh đến các chức năng khác
  - Hiển thị bài tập gần đây

### 2. Điểm danh (`/StudentDashboard/Attendance`)

- **Mục đích**: Xem lịch sử điểm danh
- **Tính năng**:
  - Danh sách tất cả buổi điểm danh
  - Thống kê điểm danh (có mặt, vắng mặt, đi muộn, có phép)
  - Tỷ lệ điểm danh với biểu đồ tiến trình
  - Hiển thị trạng thái chi tiết cho từng buổi học

### 3. Bài tập (`/StudentDashboard/Assignments`)

- **Mục đích**: Xem và nộp bài tập
- **Tính năng**:
  - Danh sách tất cả bài tập được giao
  - Xem chi tiết bài tập (mô tả, hạn nộp, tài liệu)
  - Nộp bài tập với câu trả lời text và file đính kèm
  - Xem trạng thái nộp bài (đã nộp, chưa nộp, nộp muộn)
  - Xem điểm và nhận xét từ giáo viên
  - Cảnh báo bài tập sắp đến hạn và quá hạn

### 4. Điểm số (`/StudentDashboard/Grades`)

- **Mục đích**: Xem điểm bài tập và điểm cuối kỳ
- **Tính năng**:
  - Danh sách điểm bài tập đã được chấm
  - Thống kê điểm (trung bình, cao nhất, thấp nhất)
  - Điểm cuối kỳ (chỉ hiển thị khi giáo viên đã nhập)
  - Biểu đồ điểm số (nếu có đủ dữ liệu)
  - Nhận xét từ giáo viên

## Logic nghiệp vụ

### Kiểm soát đăng ký lớp

- Học viên chỉ có thể đăng ký lớp khi chưa có lớp nào được phê duyệt
- Sau khi được phê duyệt, học viên không thể đăng ký lớp khác
- Navigation menu sẽ thay đổi để hiển thị các chức năng của Student Dashboard

### Hiển thị điểm số

- **Điểm bài tập**: Chỉ hiển thị sau khi giáo viên chấm và nhập điểm
- **Điểm cuối kỳ**: Chỉ hiển thị sau khi giáo viên nhập điểm kiểm tra cuối kỳ
- Nếu chưa chấm/nhập điểm, sẽ hiển thị "Chưa có" hoặc "Chưa chấm"

### Trạng thái bài tập

- **Chưa nộp**: Hiển thị nút "Nộp bài" (nếu chưa quá hạn)
- **Đã nộp**: Hiển thị thời gian nộp và trạng thái (đúng hạn/nộp muộn)
- **Đã chấm**: Hiển thị điểm và nhận xét
- **Chưa chấm**: Hiển thị "Chưa chấm"

## Cấu trúc file

### Controllers

- `StudentDashboardController.cs`: Controller chính cho Student Dashboard
- `BaseController.cs`: Controller cơ sở để kiểm tra trạng thái đăng ký
- Cập nhật `CourseController.cs`: Thêm logic ngăn đăng ký lớp khác
- Cập nhật `HomeController.cs`: Kế thừa từ BaseController

### Views

- `Views/StudentDashboard/Index.cshtml`: Trang chính của Student Dashboard
- `Views/StudentDashboard/Attendance.cshtml`: Trang xem điểm danh
- `Views/StudentDashboard/Assignments.cshtml`: Trang bài tập
- `Views/StudentDashboard/Grades.cshtml`: Trang điểm số
- Cập nhật `Views/Shared/_Layout.cshtml`: Navigation menu động
- Cập nhật `Views/Course/Index.cshtml`: Ẩn nút đăng ký cho học viên đã có lớp

### Services & DTOs

- Cập nhật `CourseRegistrationDto.cs`: Thêm trường `CourseId`
- Cập nhật `CourseRegistrationService.cs`: Thêm `CourseId` vào các phương thức

## Cách sử dụng

1. **Học viên đăng ký lớp**: Sử dụng chức năng đăng ký như bình thường
2. **Giáo viên phê duyệt**: Duyệt đăng ký trong phần "Đăng ký"
3. **Học viên truy cập Student Dashboard**: Sau khi được phê duyệt, navigation menu sẽ thay đổi
4. **Sử dụng các chức năng**: Điểm danh, Bài tập, Điểm số sẽ có sẵn trong menu

## Lưu ý kỹ thuật

- Sử dụng `ViewBag.HasApprovedRegistration` để kiểm tra trạng thái trong layout
- `BaseController` được sử dụng để tự động kiểm tra trạng thái đăng ký
- Tất cả các controller liên quan đều kế thừa từ `BaseController`
- Navigation menu sẽ tự động thay đổi dựa trên trạng thái đăng ký của học viên


