# 🚀 QAgent PowerShell Scripts Guide
**Ngày tạo**: 28/05/2025  
**Mục đích**: Hướng dẫn sử dụng các scripts PowerShell để quản lý ứng dụng QAgent  
**Tác giả**: AI Assistant  

## 📋 **Danh Sách Scripts**

### 1. 🔨 **REBUILD_AND_RUN.ps1** - Build và Run Foreground
**Mục đích**: Rebuild và chạy ứng dụng ở chế độ foreground (interactive)
```powershell
.\REBUILD_AND_RUN.ps1
```
**Tính năng**:
- ✅ Dừng tất cả process dotnet hiện tại
- ✅ Tự động chuyển đến folder QAgentWeb 
- ✅ Clean → Restore → Build → Run
- ✅ Hiển thị thông tin chi tiết từng bước
- ✅ Error handling và troubleshooting tips
- ⚠️ **Lưu ý**: Chạy ở foreground, cần giữ cửa sổ PowerShell mở

---

### 2. ⚡ **QUICK_RUN.ps1** - Chạy Nhanh
**Mục đích**: Chạy nhanh ứng dụng khi đã build sẵn
```powershell
.\QUICK_RUN.ps1
```
**Tính năng**:
- ✅ Dừng process cũ
- ✅ Chuyển đến folder QAgentWeb
- ✅ Chạy ngay mà không build lại
- ⚡ **Nhanh**: Tiết kiệm thời gian khi không cần rebuild

---

### 3. 🌙 **REBUILD_AND_RUN_BACKGROUND.ps1** - Build và Run Background
**Mục đích**: Rebuild và chạy ứng dụng ở chế độ background (không chiếm terminal)
```powershell
.\REBUILD_AND_RUN_BACKGROUND.ps1
```
**Tính năng**:
- ✅ Tất cả tính năng của REBUILD_AND_RUN.ps1
- ✅ **Chạy background**: Sử dụng PowerShell Jobs
- ✅ Tự động kiểm tra trạng thái khởi động
- ✅ Cung cấp Management Commands
- 🎯 **Ưu điểm**: Có thể đóng terminal mà ứng dụng vẫn chạy

**Output Example**:
```
🚀 QAgentWeb started as background job!
📊 Job ID: 1
📊 Job Name: Job1
✅ Application is running successfully in background!

🔧 Management Commands:
   • Check status: Get-Job -Id 1
   • View output: Receive-Job -Id 1 -Keep  
   • Stop app: Stop-Job -Id 1; Remove-Job -Id 1
```

---

### 4. 🎛️ **MANAGE_BACKGROUND_APP.ps1** - Quản Lý Background Jobs
**Mục đích**: Quản lý các background jobs của ứng dụng QAgent
```powershell
.\MANAGE_BACKGROUND_APP.ps1 -Action <action>
```

#### **Available Actions**:

#### **📊 Status** - Kiểm tra trạng thái
```powershell
.\MANAGE_BACKGROUND_APP.ps1 -Action status
```
- Kiểm tra background jobs đang chạy
- Kiểm tra port 5174 có đang sử dụng không
- Hiển thị thông tin chi tiết

#### **⏹️ Stop** - Dừng tất cả
```powershell
.\MANAGE_BACKGROUND_APP.ps1 -Action stop
```
- Dừng tất cả background jobs
- Dừng tất cả dotnet processes
- Clean up hoàn toàn

#### **📋 Logs** - Xem logs
```powershell
.\MANAGE_BACKGROUND_APP.ps1 -Action logs
```
- Hiển thị logs từ background jobs
- Debug và troubleshooting

#### **🔄 Restart** - Khởi động lại
```powershell
.\MANAGE_BACKGROUND_APP.ps1 -Action restart
```
- Dừng tất cả instances hiện tại
- Tự động chạy lại REBUILD_AND_RUN_BACKGROUND.ps1

#### **📋 List** - Liệt kê tất cả
```powershell
.\MANAGE_BACKGROUND_APP.ps1 -Action list
```
- Hiển thị tất cả PowerShell jobs
- Hiển thị tất cả dotnet processes
- Format table dễ đọc

---

## 🎯 **Kịch Bản Sử Dụng**

### **🔰 Lần Đầu Setup**
```powershell
# Build và chạy lần đầu
.\REBUILD_AND_RUN_BACKGROUND.ps1

# Kiểm tra trạng thái
.\MANAGE_BACKGROUND_APP.ps1 -Action status
```

### **⚡ Development Workflow**
```powershell
# Chỉnh sửa code...

# Restart để apply changes
.\MANAGE_BACKGROUND_APP.ps1 -Action restart
```

### **🔍 Debug & Troubleshooting**
```powershell
# Xem logs
.\MANAGE_BACKGROUND_APP.ps1 -Action logs

# Kiểm tra chi tiết
.\MANAGE_BACKGROUND_APP.ps1 -Action list

# Dừng và chạy manual để debug
.\MANAGE_BACKGROUND_APP.ps1 -Action stop
.\REBUILD_AND_RUN.ps1  # Foreground mode để xem chi tiết
```

### **🛑 Cleanup**
```powershell
# Dừng tất cả khi kết thúc work
.\MANAGE_BACKGROUND_APP.ps1 -Action stop
```

---

## 🌐 **URLs Quan Trọng**

Sau khi chạy scripts thành công:
- **🏠 Home**: http://localhost:5174
- **🎯 UC02**: http://localhost:5174/UC02
- **📋 UC01**: http://localhost:5174/UC01

---

## ⚠️ **Lưu Ý Quan Trọng**

### **📂 Directory Requirements**
- ✅ Scripts phải được chạy từ: `C:\Customize\01.QAgent`
- ✅ Project phải tồn tại tại: `C:\Customize\01.QAgent\qagent-app\QAgentWeb`

### **🔧 Prerequisites**
- ✅ .NET 8 SDK installed: `dotnet --version`
- ✅ PowerShell với quyền Administrator (recommended)
- ✅ Port 5174 không bị chiếm bởi ứng dụng khác

### **🚫 Common Issues**
1. **Port conflict**: Dùng `netstat -an | Select-String :5174` để kiểm tra
2. **Permission denied**: Chạy PowerShell as Administrator
3. **Project not found**: Kiểm tra đường dẫn project
4. **Background job failed**: Dùng `Get-Job` và `Receive-Job` để debug

---

## 🎉 **Best Practices**

### **🔄 Daily Workflow**
```powershell
# Morning: Start background app
.\REBUILD_AND_RUN_BACKGROUND.ps1

# During development: Quick restart when needed
.\MANAGE_BACKGROUND_APP.ps1 -Action restart

# Evening: Stop all
.\MANAGE_BACKGROUND_APP.ps1 -Action stop
```

### **🧪 Testing**
```powershell
# Before committing code
.\MANAGE_BACKGROUND_APP.ps1 -Action restart
# Test on http://localhost:5174/UC02
```

### **📊 Monitoring**
```powershell
# Regular health check
.\MANAGE_BACKGROUND_APP.ps1 -Action status
```

---

## 🏆 **Summary**

| Script | Use Case | Mode | Best For |
|--------|----------|------|----------|
| `REBUILD_AND_RUN.ps1` | Full rebuild + run | Foreground | Initial setup, debugging |
| `QUICK_RUN.ps1` | Quick start | Foreground | Fast testing |
| `REBUILD_AND_RUN_BACKGROUND.ps1` | Full rebuild + run | Background | Daily development |
| `MANAGE_BACKGROUND_APP.ps1` | Job management | Utility | Operations & monitoring |

---

**📞 Support**: Tất cả scripts đều có error handling và troubleshooting tips built-in.  
**🚀 Ready**: Scripts đã được test và sẵn sàng sử dụng production! 