# AI Product Defect Inspection & QC Assistant

Ứng dụng giúp nhân viên QC kiểm tra lỗi sản phẩm bằng ảnh.
Tải ảnh lên, AI (Google Gemini) tự nhận diện lỗi và tạo **phiếu kiểm định nháp**.
QC bấm **Xác nhận** thì hệ thống cập nhật số lượng đạt / hỏng trong kho.

Sơ đồ luồng và quy tắc chi tiết: [docs/project-flow.md](docs/project-flow.md)

---

## 1. Cần cài sẵn

| Công cụ | Ghi chú |
|---|---|
| Visual Studio 2022 | Có .NET 10 SDK |
| SQL Server | Cài trực tiếp trên máy (SQL Server, Express hoặc LocalDB). Không cần Docker |
| Node.js 18 trở lên | Để chạy giao diện React |
| Gemini API key | Lấy tại https://aistudio.google.com/apikey |

---

## 2. Cách chạy

### Bước 1: Mở dự án
Nhấp đúp file `homework.sln` để mở bằng Visual Studio 2022.

### Bước 2: Tạo file cấu hình
Trong thư mục `homework/homework/`:

1. Copy file `appsettings.Local.example.json` thành `appsettings.Local.json`.
2. Mở `appsettings.Local.json` và sửa 2 chỗ: **chuỗi kết nối** và **API key**.

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=.;Database=QcInspectionDb;Trusted_Connection=True;TrustServerCertificate=True"
  },
  "Gemini": {
    "ApiKey": "YOUR_GEMINI_API_KEY",
    "Model": "gemini-2.5-flash"
  }
}
```

Chọn chuỗi kết nối phù hợp với SQL Server của bạn:

| Loại SQL Server | DefaultConnection |
|---|---|
| Mặc định, đăng nhập bằng Windows | `Server=.;Database=QcInspectionDb;Trusted_Connection=True;TrustServerCertificate=True` |
| SQL Server Express | `Server=localhost\\SQLEXPRESS;Database=QcInspectionDb;Trusted_Connection=True;TrustServerCertificate=True` |
| Tài khoản `sa` | `Server=localhost;Database=QcInspectionDb;User Id=sa;Password=YOUR_PASSWORD;TrustServerCertificate=True` |

> File `appsettings.Local.json` được ưu tiên hơn `appsettings.json` và đã nằm trong `.gitignore`,
> nên key và mật khẩu của bạn không bị đẩy lên Git.

### Bước 3: Tạo database
1. Vào menu **Tools** → **NuGet Package Manager** → **Package Manager Console**.
2. Ở ô **Default project** chọn `homework`.
3. Gõ lệnh:

```powershell
Update-Database
```

Lệnh này tạo database `QcInspectionDb`, các bảng và dữ liệu mẫu: 3 sản phẩm, 8 nhân viên, 5 loại lỗi.

### Bước 4: Chạy API
Chọn profile `http` rồi bấm **F5**.

- API chạy tại: http://localhost:5195
- Swagger (thử API): http://localhost:5195/swagger

### Bước 5: Chạy giao diện
Mở thêm một cửa sổ terminal (API vẫn phải đang chạy):

```bash
cd homeworkclient
npm install
npm run dev
```

Mở trình duyệt tại http://localhost:5173

### Chạy bằng dòng lệnh (không dùng Visual Studio)
Cài công cụ một lần: `dotnet tool install --global dotnet-ef`

```bash
cd homework
dotnet ef database update
dotnet run --launch-profile http
```

---

## 3. Cách dùng

1. **Dashboard**: xem số hàng đạt / hỏng, tỷ lệ Pass và các phiếu đang chờ xác nhận.
2. **Upload Form**: gõ tên sản phẩm, chọn nhân viên kiểm tra, chọn ảnh rồi bấm Phân tích.
3. **Preview Form**: xem kết quả AI nhận diện. Nếu đúng thì bấm **Xác nhận**.
4. **History**: tìm kiếm, lọc và xem lại các phiếu đã tạo.

---

## 4. Quy tắc cần biết

- Mỗi ảnh là **1 sản phẩm**, tạo ra **1 phiếu**.
- Ảnh chấp nhận: `.jpg`, `.png`, `.webp`, tối đa 10 MB.
- AI chỉ chọn **loại lỗi**. Kết quả **Pass / Fail do hệ thống tự tính** theo bảng dưới:

| Loại lỗi | Mức độ | Kết quả |
|---|---|---|
| Không lỗi | None | Pass |
| Trầy xước bề mặt | Minor | Pass |
| Vết bẩn / ố màu | Minor | Pass |
| Móp méo biến dạng | Major | Fail |
| Nứt vỡ | Major | Fail |

- Khi bấm **Xác nhận**: kho cộng thêm 1 vào số đạt (Pass) hoặc số hỏng (Fail).
- Mỗi phiếu chỉ xác nhận được **1 lần**. Khi số đã kiểm bằng tổng kho thì không xác nhận thêm được.
- Gõ tên sản phẩm **chưa có** thì hệ thống tự tạo mới (kho 100, danh mục "Chưa phân loại").

---

## 5. Gặp lỗi?

| Lỗi | Cách xử lý |
|---|---|
| `Login failed for user ...` | Sai tài khoản hoặc mật khẩu trong `DefaultConnection`. Kiểm tra lại ở `appsettings.Local.json` |
| AI báo chưa cấu hình `Gemini:ApiKey` | Chưa tạo `appsettings.Local.json`, hoặc `ApiKey` còn là `YOUR_GEMINI_API_KEY`. Sửa xong thì chạy lại API |
| Giao diện không kết nối được API | Kiểm tra API đang chạy ở cổng 5195. Nếu đổi cổng: copy `homeworkclient/.env.example` thành `.env` rồi sửa `VITE_API_BASE_URL` |
| `Update-Database` không chạy | Kiểm tra **Default project** đang chọn là `homework` |

---

## 6. Test nhanh bằng file HTTP

Có thể thử API mà không cần mở giao diện, bằng file `homework/homework.http` trong Visual Studio.

---

## 7. Cấu trúc thư mục

```
homework/
├── homework/            API (ASP.NET Core)
│   ├── Controllers/     nhận request, trả response
│   ├── Services/        nghiệp vụ (gọi Gemini, lưu ảnh, xác nhận phiếu)
│   ├── Entities/        các bảng trong database
│   ├── Dtos/            dữ liệu vào / ra của API
│   ├── Data/            AppDbContext
│   └── Migrations/      lịch sử tạo database
├── homeworkclient/      Giao diện (React)
└── docs/                Tài liệu (project-flow.md)
```
