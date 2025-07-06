# QAgent Web API Documentation

QAgent Web API cung cấp đầy đủ RESTful endpoints để tích hợp với frontend applications. API hỗ trợ quản lý projects, screens, AI analysis, ISTQB rules, authentication và export functionality.

## Base URL
```
https://your-domain.com/api
```

## Authentication
API sử dụng session-based authentication. Sau khi login thành công, session sẽ được tự động maintain cho các requests tiếp theo.

## API Endpoints

### 1. Authentication API (`/api/Auth`)

#### POST `/api/Auth/login`
Đăng nhập người dùng
```json
{
  "email": "user@example.com",
  "password": "password123",
  "rememberMe": false
}
```

#### POST `/api/Auth/register`
Đăng ký tài khoản mới
```json
{
  "email": "user@example.com",
  "password": "password123",
  "confirmPassword": "password123",
  "firstName": "John",
  "lastName": "Doe",
  "phone": "+84123456789",
  "organization": "Company ABC",
  "jobTitle": "QA Engineer"
}
```

#### GET `/api/Auth/me`
Lấy thông tin user hiện tại

#### PUT `/api/Auth/me`
Cập nhật thông tin user hiện tại

#### POST `/api/Auth/logout`
Đăng xuất

#### POST `/api/Auth/change-password`
Đổi mật khẩu

#### POST `/api/Auth/forgot-password`
Quên mật khẩu

#### GET `/api/Auth/stats`
Lấy thống kê user

---

### 2. Projects API (`/api/Projects`)

#### GET `/api/Projects`
Lấy danh sách tất cả projects

#### GET `/api/Projects/{id}`
Lấy chi tiết project theo ID

#### POST `/api/Projects`
Tạo project mới
```json
{
  "name": "Project Name",
  "description": "Project Description",
  "domain": "Web Application"
}
```

#### PUT `/api/Projects/{id}`
Cập nhật project

#### DELETE `/api/Projects/{id}`
Xóa project (soft delete)

---

### 3. Screens API (`/api/Screens`)

#### GET `/api/Screens`
Lấy danh sách screens
- Query parameters: `projectId` (optional)

#### GET `/api/Screens/{id}`
Lấy chi tiết screen theo ID

#### POST `/api/Screens`
Upload screen mới (multipart/form-data)
```
name: string
description: string (optional)
projectId: number
screenType: string (default: "UI")
priority: string (default: "Medium")
file: file (required)
```

#### POST `/api/Screens/upload`
Upload nhiều screens cùng lúc (multipart/form-data)
```
projectId: number
files: file[] (required)
description: string (optional)
```

#### PUT `/api/Screens/{id}`
Cập nhật thông tin screen

#### DELETE `/api/Screens/{id}`
Xóa screen

---

### 4. Analysis API (`/api/Analysis`)

#### POST `/api/Analysis/analyze`
Phân tích screen với AI
```json
{
  "screenId": 1,
  "businessDescription": "Login page for web application",
  "analysisType": "Full",
  "includeUIElements": true,
  "includeTextExtraction": true,
  "includeISTQBRules": true,
  "generateChecklist": true
}
```

#### GET `/api/Analysis/result/{id}`
Lấy kết quả phân tích theo ID

#### GET `/api/Analysis/screen/{screenId}`
Lấy tất cả phân tích của một screen

#### POST `/api/Analysis/batch`
Phân tích nhiều screens cùng lúc
```json
{
  "screenIds": [1, 2, 3],
  "businessDescription": "E-commerce checkout flow",
  "analysisType": "Full",
  "includeUIElements": true,
  "includeTextExtraction": true,
  "includeISTQBRules": true,
  "generateChecklist": true
}
```

#### GET `/api/Analysis/progress/{batchId}`
Theo dõi tiến độ phân tích batch

#### POST `/api/Analysis/regenerate-checklist`
Tạo lại checklist cho screen

---

### 5. Rules API (`/api/Rules`)

#### GET `/api/Rules`
Lấy danh sách ISTQB rules
- Query parameters: `category`, `ruleType`, `isActive`, `priority`

#### GET `/api/Rules/{id}`
Lấy chi tiết rule theo ID

#### POST `/api/Rules`
Tạo rule mới
```json
{
  "ruleId": "UI_001",
  "ruleName": "Button Visibility Rule",
  "ruleCategory": "UI Testing",
  "ruleType": "Functional",
  "description": "All buttons must be visible and clickable",
  "priority": 8,
  "isActive": true,
  "ruleCategoryId": 1
}
```

#### PUT `/api/Rules/{id}`
Cập nhật rule

#### DELETE `/api/Rules/{id}`
Xóa rule

#### GET `/api/Rules/categories`
Lấy danh sách rule categories

#### POST `/api/Rules/categories`
Tạo rule category mới

#### POST `/api/Rules/apply`
Áp dụng rules cho screen
```json
{
  "screenId": 1,
  "businessDescription": "Login functionality",
  "specificRuleIds": ["UI_001", "FUNC_002"],
  "ruleCategory": "UI Testing",
  "forceReapply": false
}
```

#### GET `/api/Rules/applications`
Lấy lịch sử áp dụng rules

---

### 6. Export API (`/api/Export`)

#### POST `/api/Export/excel`
Export dữ liệu ra Excel
```json
{
  "exportType": "excel",
  "dataType": "project",
  "itemIds": [1, 2, 3],
  "includeMetadata": true,
  "includeAnalysisResults": true,
  "includeChecklistItems": true
}
```

#### POST `/api/Export/csv`
Export dữ liệu ra CSV

#### POST `/api/Export/google-sheets`
Export ra Google Sheets
```json
{
  "dataType": "screen",
  "itemIds": [1, 2, 3],
  "createNewSpreadsheet": true,
  "shareWithUser": true,
  "userEmail": "user@example.com",
  "worksheetName": "QAgent Export"
}
```

#### GET `/api/Export/result/{exportId}`
Lấy kết quả export

#### GET `/api/Export/download/{exportId}`
Download file đã export

#### POST `/api/Export/bulk`
Export nhiều loại dữ liệu cùng lúc

#### GET `/api/Export/templates`
Lấy danh sách export templates

#### GET `/api/Export/history`
Lấy lịch sử export

---

### 7. Dashboard API (`/api/Dashboard`)

#### GET `/api/Dashboard/overview`
Lấy tổng quan dashboard

#### GET `/api/Dashboard/analytics`
Lấy dữ liệu analytics
- Query parameters: `days` (default: 30)

#### GET `/api/Dashboard/projects/recent`
Lấy projects gần đây
- Query parameters: `limit` (default: 10)

#### GET `/api/Dashboard/screens/pending-analysis`
Lấy screens chưa phân tích

#### GET `/api/Dashboard/quick-stats`
Lấy thống kê nhanh (hôm nay, tuần này, tháng này)

#### GET `/api/Dashboard/system-health`
Kiểm tra tình trạng hệ thống

---

## Error Handling

API trả về các HTTP status codes chuẩn:
- `200 OK`: Thành công
- `201 Created`: Tạo mới thành công  
- `400 Bad Request`: Dữ liệu đầu vào không hợp lệ
- `401 Unauthorized`: Chưa đăng nhập
- `403 Forbidden`: Không có quyền truy cập
- `404 Not Found`: Không tìm thấy resource
- `500 Internal Server Error`: Lỗi server

Error response format:
```json
{
  "message": "Error description",
  "error": "Detailed error message",
  "errors": ["List of validation errors"]
}
```

## Rate Limiting

API có rate limiting để bảo vệ hệ thống:
- 1000 requests/hour cho authenticated users
- 100 requests/hour cho unauthenticated requests

## File Upload

Các endpoint upload file hỗ trợ:
- **Supported formats**: JPG, PNG, PDF, BMP, GIF, TIFF
- **Max file size**: 50MB per file
- **Content-Type**: `multipart/form-data`

## Pagination

Các endpoints trả về danh sách hỗ trợ pagination:
```json
{
  "data": [...],
  "pagination": {
    "page": 1,
    "pageSize": 20,
    "totalPages": 5,
    "totalCount": 100
  }
}
```

## Webhooks (Future)

QAgent sẽ hỗ trợ webhooks để notify khi:
- Analysis hoàn thành
- Export hoàn thành  
- Có lỗi xảy ra trong quá trình xử lý

## SDK Support

Chúng tôi sẽ cung cấp SDK cho:
- JavaScript/TypeScript
- Python
- C#/.NET
- Java

## API Versioning

API sử dụng URL versioning:
- Current version: `v1` (default)
- Future versions: `/api/v2/`, `/api/v3/`

## Examples

### Complete workflow example:

1. **Login**
```bash
curl -X POST /api/Auth/login \
  -H "Content-Type: application/json" \
  -d '{"email":"user@example.com","password":"password123"}'
```

2. **Create Project**
```bash
curl -X POST /api/Projects \
  -H "Content-Type: application/json" \
  -d '{"name":"Test Project","domain":"Web App"}'
```

3. **Upload Screen**
```bash
curl -X POST /api/Screens \
  -F "name=Login Screen" \
  -F "projectId=1" \
  -F "file=@login.png"
```

4. **Analyze Screen**
```bash
curl -X POST /api/Analysis/analyze \
  -H "Content-Type: application/json" \
  -d '{"screenId":1,"businessDescription":"Login page"}'
```

5. **Export Results**
```bash
curl -X POST /api/Export/excel \
  -H "Content-Type: application/json" \
  -d '{"dataType":"screen","itemIds":[1]}'
```

## Support

Để được hỗ trợ về API:
- Email: api-support@qagent.com
- Documentation: https://docs.qagent.com
- GitHub Issues: https://github.com/qagent/qagent-web/issues