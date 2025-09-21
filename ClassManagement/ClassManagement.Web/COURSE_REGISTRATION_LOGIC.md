# Logic Đăng Ký Lớp Học - Cập Nhật

## Tổng quan

Hệ thống đã được cập nhật để xử lý logic đăng ký lớp học một cách rõ ràng và hợp lý hơn.

## Logic Mới

### 1. **Khi học viên CHƯA có lớp được phê duyệt**

- **Trang "Danh sách lớp học"**: Hiển thị tất cả các lớp phù hợp với khối của học viên
- **Chức năng**: Có thể đăng ký bất kỳ lớp nào
- **Trạng thái**: Hiển thị nút "Đăng ký" cho từng lớp

### 2. **Khi học viên ĐÃ có lớp được phê duyệt**

- **Trang "Danh sách lớp học"**: Chỉ hiển thị lớp đã được phê duyệt
- **Thông báo**: Hiển thị alert xanh "Bạn đã được phê duyệt vào lớp học!"
- **Badge**: Hiển thị "Lớp của bạn" thay vì "Đã có lớp"
- **Chức năng**: Không thể đăng ký lớp khác

### 3. **Khi học viên muốn đổi lớp**

- **Quy trình**:
  1. Học viên liên hệ giáo viên qua điện thoại
  2. Giáo viên vào Course → Details → Click "Xóa khỏi khóa" cho học viên đó
  3. Hệ thống tự động gửi thông báo cho học viên
  4. Học viên vào "Danh sách lớp học" → Hiển thị lại tất cả lớp để đăng ký mới

## Các Thay Đổi Kỹ Thuật

### 1. **CourseController.Index()**

```csharp
// Logic mới:
if (hasApprovedRegistration) {
    // Chỉ hiển thị lớp đã được phê duyệt
    courses = new List<CourseDto> { approvedCourse };
} else {
    // Hiển thị tất cả lớp để đăng ký
    courses = await _courseService.GetCoursesForStudentAsync(studentId);
}
```

### 2. **Course/Index.cshtml**

- Thêm alert thông báo khi đã được phê duyệt
- Thay đổi badge từ "Đã có lớp" thành "Lớp của bạn"
- Hiển thị thông tin rõ ràng hơn

### 3. **TeacherController.RemoveStudentFromCourse()**

- Action mới để giáo viên xóa học viên khỏi lớp
- Tự động gửi thông báo cho học viên
- Cho phép học viên đăng ký lại

### 4. **CourseRegistrationService.RemoveStudentFromCourseAsync()**

- Xóa registration khỏi database
- Gửi notification cho học viên
- Reset trạng thái để học viên có thể đăng ký lại

## Workflow Hoàn Chỉnh

### **Scenario 1: Học viên mới**

1. Học viên đăng nhập → Vào "Danh sách lớp học"
2. Thấy tất cả lớp phù hợp → Chọn lớp → Click "Đăng ký"
3. Chờ giáo viên phê duyệt
4. Sau khi được phê duyệt → Vào "Danh sách lớp học" → Chỉ thấy lớp của mình

### **Scenario 2: Học viên muốn đổi lớp**

1. Học viên liên hệ giáo viên
2. Giáo viên vào Course → Details → Click "Xóa khỏi khóa"
3. Hệ thống gửi thông báo cho học viên
4. Học viên vào "Danh sách lớp học" → Thấy lại tất cả lớp
5. Đăng ký lớp mới → Chờ phê duyệt

### **Scenario 3: Giáo viên quản lý lớp**

1. Vào Course → Details
2. Thấy danh sách học viên đã được phê duyệt
3. Có thể xóa học viên khỏi lớp nếu cần
4. Học viên sẽ nhận thông báo và có thể đăng ký lại

## Lợi Ích

### **Cho Học Viên**

- ✅ Rõ ràng về trạng thái đăng ký
- ✅ Biết chính xác lớp nào đã được phê duyệt
- ✅ Dễ dàng đổi lớp khi cần thiết
- ✅ Nhận thông báo khi bị xóa khỏi lớp

### **Cho Giáo Viên**

- ✅ Dễ dàng quản lý học viên trong lớp
- ✅ Có thể xóa học viên khi cần thiết
- ✅ Hệ thống tự động thông báo cho học viên

### **Cho Hệ Thống**

- ✅ Logic rõ ràng, dễ hiểu
- ✅ Tránh confusion về trạng thái đăng ký
- ✅ Workflow hoàn chỉnh từ đăng ký đến đổi lớp

## Cách Sử Dụng

### **Học Viên**

1. **Đăng ký lớp**: Vào "Danh sách lớp học" → Chọn lớp → "Đăng ký"
2. **Xem lớp đã phê duyệt**: Vào "Danh sách lớp học" → Thấy lớp của mình
3. **Đổi lớp**: Liên hệ giáo viên → Giáo viên xóa → Đăng ký lại

### **Giáo Viên**

1. **Phê duyệt đăng ký**: Vào "Đăng ký" → Duyệt/từ chối
2. **Xóa học viên**: Vào Course → Details → "Xóa khỏi khóa"
3. **Quản lý lớp**: Xem danh sách học viên trong lớp

Hệ thống bây giờ hoạt động một cách logic và rõ ràng hơn nhiều!


