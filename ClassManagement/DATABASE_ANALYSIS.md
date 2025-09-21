# 📊 PHÂN TÍCH CƠ SỞ DỮ LIỆU - CLASS MANAGEMENT SYSTEM

## 🗄️ **TỔNG QUAN DATABASE**

**Tên Database**: ClassManagement  
**Công nghệ**: SQL Server với Entity Framework Core  
**Kiến trúc**: Clean Architecture với Domain-Driven Design

---

## 📋 **DANH SÁCH CÁC BẢNG**

### **1. 👤 Users (Bảng người dùng)**

```sql
CREATE TABLE Users (
    Id int IDENTITY(1,1) PRIMARY KEY,
    Email nvarchar(255) NOT NULL UNIQUE,
    PasswordHash nvarchar(255) NOT NULL,
    FullName nvarchar(255) NOT NULL,
    Role int NOT NULL,                    -- 1=Teacher, 2=Student
    Grade int NULL,                       -- 10, 11, 12 (chỉ cho học sinh)
    CreatedAt datetime2 NOT NULL,
    LastLoginAt datetime2 NULL,
    IsActive bit NOT NULL DEFAULT 1
);
```

**Mục đích**: Lưu trữ thông tin tài khoản của giáo viên và học sinh  
**Ràng buộc**: Email duy nhất, Role bắt buộc

---

### **2. 📚 Courses (Bảng lớp học)**

```sql
CREATE TABLE Courses (
    Id int IDENTITY(1,1) PRIMARY KEY,
    Grade int NOT NULL,                   -- 10, 11, 12
    TimeSlot int NOT NULL,                -- 1=14-16h, 2=17-19h
    StartDate datetime2 NOT NULL,
    EndDate datetime2 NOT NULL,
    CreatedAt datetime2 NOT NULL,
    IsActive bit NOT NULL DEFAULT 1,
    IsDataExported bit NOT NULL DEFAULT 0
);
```

**Mục đích**: Quản lý các lớp học theo khối và ca học  
**Ràng buộc**: Unique constraint trên (Grade, TimeSlot, StartDate)

---

### **3. 📝 CourseRegistrations (Bảng đăng ký lớp)**

```sql
CREATE TABLE CourseRegistrations (
    Id int IDENTITY(1,1) PRIMARY KEY,
    StudentId int NOT NULL,
    CourseId int NOT NULL,
    Status int NOT NULL DEFAULT 1,        -- 1=Pending, 2=Approved, 3=Rejected
    RejectionReason nvarchar(max) NULL,
    RequestedAt datetime2 NOT NULL,
    ProcessedAt datetime2 NULL,

    FOREIGN KEY (StudentId) REFERENCES Users(Id) ON DELETE CASCADE,
    FOREIGN KEY (CourseId) REFERENCES Courses(Id) ON DELETE CASCADE,
    UNIQUE (StudentId, CourseId)
);
```

**Mục đích**: Quản lý việc đăng ký lớp học của học sinh  
**Ràng buộc**: Mỗi học sinh chỉ đăng ký 1 lần/lớp

---

### **4. ✅ Attendances (Bảng điểm danh)**

```sql
CREATE TABLE Attendances (
    Id int IDENTITY(1,1) PRIMARY KEY,
    StudentId int NOT NULL,
    CourseId int NOT NULL,
    Date datetime2 NOT NULL,
    TimeSlot int NOT NULL,
    Status int NOT NULL,                  -- 1=Present, 2=Absent, 3=Late, 4=Excused
    CreatedAt datetime2 NOT NULL,

    FOREIGN KEY (StudentId) REFERENCES Users(Id) ON DELETE CASCADE,
    FOREIGN KEY (CourseId) REFERENCES Courses(Id) ON DELETE CASCADE,
    UNIQUE (StudentId, CourseId, Date, TimeSlot)
);
```

**Mục đích**: Ghi nhận điểm danh hàng ngày của học sinh  
**Ràng buộc**: Mỗi học sinh chỉ điểm danh 1 lần/ngày/ca

---

### **5. 📋 Assignments (Bảng bài tập)**

```sql
CREATE TABLE Assignments (
    Id int IDENTITY(1,1) PRIMARY KEY,
    CourseId int NOT NULL,
    Title nvarchar(max) NOT NULL,
    Description nvarchar(max) NULL,
    FileUrl nvarchar(max) NULL,
    DueDate datetime2 NOT NULL,
    CreatedAt datetime2 NOT NULL,
    IsActive bit NOT NULL DEFAULT 1,

    FOREIGN KEY (CourseId) REFERENCES Courses(Id) ON DELETE CASCADE
);
```

**Mục đích**: Quản lý bài tập được giao cho học sinh  
**Ràng buộc**: Thuộc về 1 lớp học cụ thể

---

### **6. 📤 AssignmentSubmissions (Bảng nộp bài)**

```sql
CREATE TABLE AssignmentSubmissions (
    Id int IDENTITY(1,1) PRIMARY KEY,
    AssignmentId int NOT NULL,
    StudentId int NOT NULL,
    TextAnswer nvarchar(max) NULL,
    FileUrl nvarchar(max) NULL,
    SubmittedAt datetime2 NOT NULL,
    Grade decimal(18,2) NULL,
    Feedback nvarchar(max) NULL,
    GradedAt datetime2 NULL,

    FOREIGN KEY (AssignmentId) REFERENCES Assignments(Id) ON DELETE CASCADE,
    FOREIGN KEY (StudentId) REFERENCES Users(Id) ON DELETE CASCADE,
    UNIQUE (AssignmentId, StudentId)
);
```

**Mục đích**: Lưu trữ bài nộp và điểm số của học sinh  
**Ràng buộc**: Mỗi học sinh chỉ nộp 1 lần/bài tập

---

### **7. 🎯 FinalGrades (Bảng điểm cuối khóa)**

```sql
CREATE TABLE FinalGrades (
    Id int IDENTITY(1,1) PRIMARY KEY,
    StudentId int NOT NULL,
    CourseId int NOT NULL,
    Grade decimal(18,2) NOT NULL,        -- 0-10
    CreatedAt datetime2 NOT NULL,

    FOREIGN KEY (StudentId) REFERENCES Users(Id) ON DELETE CASCADE,
    FOREIGN KEY (CourseId) REFERENCES Courses(Id) ON DELETE CASCADE,
    UNIQUE (StudentId, CourseId)
);
```

**Mục đích**: Lưu điểm tổng kết cuối khóa của học sinh  
**Ràng buộc**: Mỗi học sinh chỉ có 1 điểm/lớp

---

### **8. 🔔 Notifications (Bảng thông báo)**

```sql
CREATE TABLE Notifications (
    Id int IDENTITY(1,1) PRIMARY KEY,
    UserId int NOT NULL,
    Title nvarchar(max) NOT NULL,
    Message nvarchar(max) NOT NULL,
    Type int NOT NULL,                   -- 1-9 (AssignmentNew, GradeReceived, etc.)
    IsRead bit NOT NULL DEFAULT 0,
    CreatedAt datetime2 NOT NULL,

    FOREIGN KEY (UserId) REFERENCES Users(Id) ON DELETE CASCADE
);
```

**Mục đích**: Quản lý thông báo trong hệ thống  
**Ràng buộc**: Thuộc về 1 người dùng cụ thể

---

### **9. 🔑 PasswordResetTokens (Bảng token reset mật khẩu)**

```sql
CREATE TABLE PasswordResetTokens (
    Id int IDENTITY(1,1) PRIMARY KEY,
    UserId int NOT NULL,
    Token nvarchar(450) NOT NULL UNIQUE,
    ExpiresAt datetime2 NOT NULL,
    IsUsed bit NOT NULL DEFAULT 0,
    CreatedAt datetime2 NOT NULL,

    FOREIGN KEY (UserId) REFERENCES Users(Id) ON DELETE CASCADE
);
```

**Mục đích**: Quản lý token để reset mật khẩu  
**Ràng buộc**: Token duy nhất, có thời hạn

---

## 🔗 **MỐI QUAN HỆ ERD (Entity Relationship Diagram)**

### **📊 Sơ đồ mối quan hệ chính:**

```
Users (1) ──────────── (N) CourseRegistrations (N) ──────────── (1) Courses
   │                                                                      │
   │                                                                      │
   │ (1) ──────────── (N) Attendances (N) ──────────── (1)              │
   │                                                                      │
   │ (1) ──────────── (N) AssignmentSubmissions (N) ──────────── (1) Assignments
   │                                                                      │
   │ (1) ──────────── (N) FinalGrades (N) ──────────── (1)              │
   │                                                                      │
   │ (1) ──────────── (N) Notifications                                  │
   │                                                                      │
   │ (1) ──────────── (N) PasswordResetTokens                            │
```

### **🔍 Chi tiết mối quan hệ:**

#### **1. Users ↔ Courses (Many-to-Many qua CourseRegistrations)**

- **Mối quan hệ**: Học sinh đăng ký nhiều lớp, lớp có nhiều học sinh
- **Bảng trung gian**: CourseRegistrations
- **Ràng buộc**: Unique (StudentId, CourseId)

#### **2. Users ↔ Courses (One-to-Many qua Attendances)**

- **Mối quan hệ**: Học sinh có nhiều điểm danh, lớp có nhiều điểm danh
- **Ràng buộc**: Unique (StudentId, CourseId, Date, TimeSlot)

#### **3. Users ↔ Assignments (One-to-Many qua AssignmentSubmissions)**

- **Mối quan hệ**: Học sinh nộp nhiều bài, bài tập có nhiều bài nộp
- **Ràng buộc**: Unique (AssignmentId, StudentId)

#### **4. Users ↔ Courses (One-to-Many qua FinalGrades)**

- **Mối quan hệ**: Học sinh có nhiều điểm cuối khóa, lớp có nhiều điểm
- **Ràng buộc**: Unique (StudentId, CourseId)

#### **5. Courses ↔ Assignments (One-to-Many)**

- **Mối quan hệ**: Lớp có nhiều bài tập, bài tập thuộc 1 lớp
- **Ràng buộc**: Foreign Key CourseId

#### **6. Users ↔ Notifications (One-to-Many)**

- **Mối quan hệ**: Người dùng có nhiều thông báo
- **Ràng buộc**: Foreign Key UserId

#### **7. Users ↔ PasswordResetTokens (One-to-Many)**

- **Mối quan hệ**: Người dùng có nhiều token reset
- **Ràng buộc**: Foreign Key UserId, Unique Token

---

## 📈 **CÁC INDEX QUAN TRỌNG**

### **🔍 Primary Keys:**

- Tất cả bảng đều có `Id` làm Primary Key với IDENTITY(1,1)

### **🔍 Unique Indexes:**

- `Users.Email` - Email duy nhất
- `Courses(Grade, TimeSlot, StartDate)` - Lớp học duy nhất
- `CourseRegistrations(StudentId, CourseId)` - Đăng ký duy nhất
- `Attendances(StudentId, CourseId, Date, TimeSlot)` - Điểm danh duy nhất
- `AssignmentSubmissions(AssignmentId, StudentId)` - Nộp bài duy nhất
- `FinalGrades(StudentId, CourseId)` - Điểm cuối khóa duy nhất
- `PasswordResetTokens.Token` - Token duy nhất

### **🔍 Foreign Key Indexes:**

- Tất cả Foreign Key đều có index để tối ưu performance

---

## 🎯 **ENUM VALUES**

### **UserRole:**

- `1` = Teacher (Giáo viên)
- `2` = Student (Học sinh)

### **Grade:**

- `10` = Grade10 (Khối 10)
- `11` = Grade11 (Khối 11)
- `12` = Grade12 (Khối 12)

### **TimeSlot:**

- `1` = Slot1 (14:00-16:00)
- `2` = Slot2 (17:00-19:00)

### **RegistrationStatus:**

- `1` = Pending (Chờ duyệt)
- `2` = Approved (Đã duyệt)
- `3` = Rejected (Từ chối)

### **AttendanceStatus:**

- `1` = Present (Có mặt)
- `2` = Absent (Vắng mặt)
- `3` = Late (Đi muộn)
- `4` = Excused (Có phép)

### **NotificationType:**

- `1` = AssignmentNew (Bài tập mới)
- `2` = AssignmentDue (Bài tập sắp hết hạn)
- `3` = GradeReceived (Có điểm mới)
- `4` = RegistrationApproved (Đăng ký được duyệt)
- `5` = RegistrationRejected (Đăng ký bị từ chối)
- `6` = CourseEndingSoon (Khóa học sắp kết thúc)
- `7` = AttendanceWarning (Cảnh báo điểm danh)
- `8` = AssignmentCancelled (Bài tập bị hủy)
- `9` = General (Thông báo chung)

---

## 🚀 **TÍNH NĂNG ĐẶC BIỆT**

### **🔒 Bảo mật:**

- Mật khẩu được hash bằng BCrypt
- Token reset mật khẩu có thời hạn
- Session management cho authentication

### **📊 Tối ưu hóa:**

- Composite indexes cho các truy vấn phức tạp
- Cascade delete để đảm bảo tính toàn vẹn dữ liệu
- Soft delete với `IsActive` flag

### **🔄 Business Logic:**

- Lịch học tự động theo khối (10: T2,T4 | 11: T3,T5 | 12: T6,T7)
- Kiểm tra thời gian điểm danh (chỉ trong giờ học)
- Tính toán điểm danh và cảnh báo vắng mặt

### **📧 Thông báo tự động:**

- Email khi có bài tập mới
- Email khi tài khoản bị khóa/mở khóa
- Thông báo trong hệ thống

---

## 📝 **KẾT LUẬN**

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

- 🎓 Trung tâm đào tạo nhỏ- trung bình
- 👨‍🏫 Quản lý lớp học theo ca
- 📚 Hệ thống bài tập và chấm điểm
- ✅ Điểm danh tự động theo lịch
