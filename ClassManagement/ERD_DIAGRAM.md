# 🗺️ ERD DIAGRAM - CLASS MANAGEMENT SYSTEM

## 📊 **ENTITY RELATIONSHIP DIAGRAM**

```
┌─────────────────────────────────────────────────────────────────────────────────┐
│                                CLASS MANAGEMENT DATABASE                        │
└─────────────────────────────────────────────────────────────────────────────────┘

┌─────────────────┐    ┌─────────────────┐    ┌─────────────────┐
│      USERS      │    │     COURSES     │    │   ASSIGNMENTS  │
├─────────────────┤    ├─────────────────┤    ├─────────────────┤
│ PK: Id          │    │ PK: Id          │    │ PK: Id          │
│ Email (UNIQUE)  │    │ Grade           │    │ CourseId (FK)   │
│ PasswordHash    │    │ TimeSlot        │    │ Title           │
│ FullName        │    │ StartDate       │    │ Description     │
│ Role            │    │ EndDate         │    │ FileUrl         │
│ Grade           │    │ CreatedAt       │    │ DueDate         │
│ CreatedAt       │    │ IsActive        │    │ CreatedAt       │
│ LastLoginAt     │    │ IsDataExported  │    │ IsActive        │
│ IsActive        │    └─────────────────┘    └─────────────────┘
└─────────────────┘            │                        │
         │                      │                        │
         │                      │                        │
         │ (1:N)               │ (1:N)                  │ (1:N)
         │                      │                        │
         ▼                      ▼                        ▼
┌─────────────────┐    ┌─────────────────┐    ┌─────────────────┐
│COURSEREGISTRATIONS│    │   ATTENDANCES   │    │ASSIGNMENTSUBMISSIONS│
├─────────────────┤    ├─────────────────┤    ├─────────────────┤
│ PK: Id          │    │ PK: Id          │    │ PK: Id          │
│ StudentId (FK)   │    │ StudentId (FK)  │    │ AssignmentId (FK)│
│ CourseId (FK)    │    │ CourseId (FK)    │    │ StudentId (FK)  │
│ Status           │    │ Date            │    │ TextAnswer      │
│ RejectionReason  │    │ TimeSlot        │    │ FileUrl         │
│ RequestedAt      │    │ Status          │    │ SubmittedAt     │
│ ProcessedAt      │    │ CreatedAt       │    │ Grade           │
│ UNIQUE(Stu,Course)│   │ UNIQUE(Stu,Course│   │ Feedback        │
└─────────────────┘    │ ,Date,TimeSlot) │    │ GradedAt        │
                       └─────────────────┘    │ UNIQUE(Ass,Stu) │
                                              └─────────────────┘
         │
         │ (1:N)
         │
         ▼
┌─────────────────┐    ┌─────────────────┐    ┌─────────────────┐
│   FINALGRADES   │    │  NOTIFICATIONS  │    │PASSWORDRESETTOKENS│
├─────────────────┤    ├─────────────────┤    ├─────────────────┤
│ PK: Id          │    │ PK: Id          │    │ PK: Id          │
│ StudentId (FK)  │    │ UserId (FK)     │    │ UserId (FK)     │
│ CourseId (FK)   │    │ Title           │    │ Token (UNIQUE)  │
│ Grade           │    │ Message         │    │ ExpiresAt       │
│ CreatedAt       │    │ Type            │    │ IsUsed          │
│ UNIQUE(Stu,Course)│ │ IsRead          │    │ CreatedAt       │
└─────────────────┘    │ CreatedAt       │    └─────────────────┘
                       └─────────────────┘
```

## 🔗 **MỐI QUAN HỆ CHI TIẾT**

### **1. USERS ↔ COURSES (Many-to-Many)**

```
Users (1) ──────────── (N) CourseRegistrations (N) ──────────── (1) Courses
```

- **Mục đích**: Học sinh đăng ký lớp học
- **Ràng buộc**: Unique(StudentId, CourseId)
- **Trạng thái**: Pending → Approved/Rejected

### **2. USERS ↔ COURSES (One-to-Many qua Attendances)**

```
Users (1) ──────────── (N) Attendances (N) ──────────── (1) Courses
```

- **Mục đích**: Điểm danh hàng ngày
- **Ràng buộc**: Unique(StudentId, CourseId, Date, TimeSlot)
- **Trạng thái**: Present/Absent/Late/Excused

### **3. USERS ↔ ASSIGNMENTS (One-to-Many qua Submissions)**

```
Users (1) ──────────── (N) AssignmentSubmissions (N) ──────────── (1) Assignments
```

- **Mục đích**: Học sinh nộp bài tập
- **Ràng buộc**: Unique(AssignmentId, StudentId)
- **Chức năng**: Nộp bài, chấm điểm, feedback

### **4. COURSES ↔ ASSIGNMENTS (One-to-Many)**

```
Courses (1) ──────────── (N) Assignments
```

- **Mục đích**: Lớp học có nhiều bài tập
- **Ràng buộc**: Foreign Key CourseId
- **Chức năng**: Giao bài, quản lý deadline

### **5. USERS ↔ FINALGRADES (One-to-Many)**

```
Users (1) ──────────── (N) FinalGrades (N) ──────────── (1) Courses
```

- **Mục đích**: Điểm cuối khóa
- **Ràng buộc**: Unique(StudentId, CourseId)
- **Thang điểm**: 0-10

### **6. USERS ↔ NOTIFICATIONS (One-to-Many)**

```
Users (1) ──────────── (N) Notifications
```

- **Mục đích**: Thông báo trong hệ thống
- **Loại**: 9 loại thông báo khác nhau
- **Trạng thái**: Read/Unread

### **7. USERS ↔ PASSWORDRESETTOKENS (One-to-Many)**

```
Users (1) ──────────── (N) PasswordResetTokens
```

- **Mục đích**: Reset mật khẩu
- **Ràng buộc**: Token unique, có thời hạn
- **Trạng thái**: Used/Unused, Expired/Valid

## 📊 **BUSINESS RULES**

### **🎓 Lịch học tự động:**

- **Khối 10**: Thứ 2, Thứ 4
- **Khối 11**: Thứ 3, Thứ 5
- **Khối 12**: Thứ 6, Thứ 7

### **⏰ Ca học:**

- **Ca 1**: 14:00-16:00
- **Ca 2**: 17:00-19:00

### **🔒 Bảo mật:**

- Mật khẩu hash bằng BCrypt
- Session management
- Token reset có thời hạn

### **📧 Thông báo tự động:**

- Email khi có bài tập mới
- Email khi tài khoản bị khóa/mở khóa
- Thông báo trong hệ thống

## 🎯 **TÍNH NĂNG ĐẶC BIỆT**

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

### **📊 Báo cáo và thống kê:**

- Thống kê điểm danh
- Thống kê nộp bài
- Xuất dữ liệu Excel
