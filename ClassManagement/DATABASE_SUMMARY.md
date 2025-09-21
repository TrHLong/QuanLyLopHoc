# 📋 TÓM TẮT CƠ SỞ DỮ LIỆU - CLASS MANAGEMENT SYSTEM

## 🗄️ **DANH SÁCH BẢNG VÀ MỐI QUAN HỆ**

| **STT** | **Tên Bảng**              | **Mục đích**         | **Số cột** | **Primary Key** | **Foreign Keys**        | **Unique Constraints**                |
| ------- | ------------------------- | -------------------- | ---------- | --------------- | ----------------------- | ------------------------------------- |
| 1       | **Users**                 | Quản lý tài khoản    | 9          | Id              | -                       | Email                                 |
| 2       | **Courses**               | Quản lý lớp học      | 8          | Id              | -                       | (Grade, TimeSlot, StartDate)          |
| 3       | **CourseRegistrations**   | Đăng ký lớp học      | 7          | Id              | StudentId, CourseId     | (StudentId, CourseId)                 |
| 4       | **Attendances**           | Điểm danh            | 7          | Id              | StudentId, CourseId     | (StudentId, CourseId, Date, TimeSlot) |
| 5       | **Assignments**           | Bài tập              | 8          | Id              | CourseId                | -                                     |
| 6       | **AssignmentSubmissions** | Nộp bài              | 9          | Id              | AssignmentId, StudentId | (AssignmentId, StudentId)             |
| 7       | **FinalGrades**           | Điểm cuối khóa       | 5          | Id              | StudentId, CourseId     | (StudentId, CourseId)                 |
| 8       | **Notifications**         | Thông báo            | 6          | Id              | UserId                  | -                                     |
| 9       | **PasswordResetTokens**   | Token reset mật khẩu | 6          | Id              | UserId                  | Token                                 |

---

## 🔗 **MỐI QUAN HỆ CHI TIẾT**

### **📊 Bảng chính (Main Tables):**

- **Users**: Bảng trung tâm, chứa thông tin người dùng
- **Courses**: Bảng trung tâm, chứa thông tin lớp học

### **📊 Bảng liên kết (Junction Tables):**

- **CourseRegistrations**: Liên kết Users ↔ Courses (Many-to-Many)
- **Attendances**: Liên kết Users ↔ Courses (One-to-Many)
- **FinalGrades**: Liên kết Users ↔ Courses (One-to-Many)

### **📊 Bảng phụ thuộc (Dependent Tables):**

- **Assignments**: Phụ thuộc vào Courses (One-to-Many)
- **AssignmentSubmissions**: Phụ thuộc vào Assignments và Users
- **Notifications**: Phụ thuộc vào Users (One-to-Many)
- **PasswordResetTokens**: Phụ thuộc vào Users (One-to-Many)

---

## 🎯 **ENUM VALUES**

| **Enum**               | **Values**                             | **Mô tả**            |
| ---------------------- | -------------------------------------- | -------------------- |
| **UserRole**           | 1=Teacher, 2=Student                   | Vai trò người dùng   |
| **Grade**              | 10, 11, 12                             | Khối lớp             |
| **TimeSlot**           | 1=14-16h, 2=17-19h                     | Ca học               |
| **RegistrationStatus** | 1=Pending, 2=Approved, 3=Rejected      | Trạng thái đăng ký   |
| **AttendanceStatus**   | 1=Present, 2=Absent, 3=Late, 4=Excused | Trạng thái điểm danh |
| **NotificationType**   | 1-9                                    | Loại thông báo       |

---

## 🔍 **CÁC RÀNG BUỘC QUAN TRỌNG**

### **🔒 Unique Constraints:**

- **Users.Email**: Email duy nhất trong hệ thống
- **Courses(Grade, TimeSlot, StartDate)**: Mỗi lớp học duy nhất
- **CourseRegistrations(StudentId, CourseId)**: Mỗi học sinh chỉ đăng ký 1 lần/lớp
- **Attendances(StudentId, CourseId, Date, TimeSlot)**: Mỗi học sinh chỉ điểm danh 1 lần/ngày/ca
- **AssignmentSubmissions(AssignmentId, StudentId)**: Mỗi học sinh chỉ nộp 1 lần/bài tập
- **FinalGrades(StudentId, CourseId)**: Mỗi học sinh chỉ có 1 điểm cuối khóa/lớp
- **PasswordResetTokens.Token**: Token duy nhất

### **🔗 Foreign Key Constraints:**

- Tất cả Foreign Key đều có **CASCADE DELETE**
- Đảm bảo tính toàn vẹn dữ liệu khi xóa

### **📊 Index Constraints:**

- Tất cả Primary Key đều có **CLUSTERED INDEX**
- Tất cả Foreign Key đều có **NON-CLUSTERED INDEX**
- Composite indexes cho các truy vấn phức tạp

---

## 🚀 **TÍNH NĂNG ĐẶC BIỆT**

### **🎓 Lịch học tự động:**

- **Khối 10**: Thứ 2, Thứ 4
- **Khối 11**: Thứ 3, Thứ 5
- **Khối 12**: Thứ 6, Thứ 7

### **⏰ Ca học:**

- **Ca 1**: 14:00-16:00
- **Ca 2**: 17:00-19:00

### **🔒 Bảo mật:**

- Mật khẩu hash bằng **BCrypt**
- Session management
- Token reset có thời hạn

### **📧 Thông báo tự động:**

- Email khi có bài tập mới
- Email khi tài khoản bị khóa/mở khóa
- Thông báo trong hệ thống

### **✅ Điểm danh thông minh:**

- Chỉ cho phép điểm danh đúng ngày học
- Chỉ cho phép điểm danh trong giờ học
- Cảnh báo khi điểm danh sai thời gian

### **📚 Quản lý bài tập:**

- Upload file đính kèm
- Chấm điểm và feedback
- Xóa bài tập (soft delete nếu có submission)

### **👥 Quản lý tài khoản:**

- Khóa/mở khóa tài khoản học sinh
- Theo dõi hoạt động đăng nhập
- Cảnh báo tài khoản không hoạt động

---

## 📊 **THỐNG KÊ DATABASE**

| **Metric**                     | **Value** |
| ------------------------------ | --------- |
| **Tổng số bảng**               | 9         |
| **Tổng số cột**                | 71        |
| **Tổng số Foreign Keys**       | 12        |
| **Tổng số Unique Constraints** | 7         |
| **Tổng số Indexes**            | 21        |
| **Tổng số Enums**              | 6         |

---

## 🎯 **KẾT LUẬN**

Database được thiết kế theo chuẩn **Normalization** với:

- **1NF**: Mỗi cell chứa 1 giá trị nguyên tử
- **2NF**: Loại bỏ partial dependency
- **3NF**: Loại bỏ transitive dependency

**Ưu điểm:**

- ✅ Cấu trúc rõ ràng, dễ hiểu
- ✅ Ràng buộc đầy đủ, đảm bảo tính toàn vẹn
- ✅ Index tối ưu cho performance
- ✅ Hỗ trợ đầy đủ business logic

**Hệ thống phù hợp cho:**

- 🎓 Trung tâm đào tạo nhỏ-trung bình
- 👨‍🏫 Quản lý lớp học theo ca
- 📚 Hệ thống bài tập và chấm điểm
- ✅ Điểm danh tự động theo lịch
