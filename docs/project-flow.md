# Homework — AI Product Defect Inspection & QC Assistant: dự án chạy như thế nào

Tài liệu mô tả các phần ghép với nhau ra sao, một lần kiểm tra ảnh chạy qua những bước nào, và muốn sửa một thứ thì mở file nào. Cách chạy dự án nằm trong `README.md`.

## 1. Dự án làm gì

Nhân viên QC gõ tên sản phẩm, chọn tên mình, tải một ảnh sản phẩm lên. Gemini Vision nhìn ảnh, chọn một loại lỗi trong danh mục có sẵn, viết mô tả và chấm độ tin cậy. Hệ thống lưu một **phiếu nháp**. QC xem lại kết quả rồi bấm **Xác nhận**: lúc này phiếu mới thành chính thức và tồn kho của sản phẩm được cộng 1 (đạt hoặc không đạt).

Một ảnh tương ứng một đơn vị sản phẩm. Kho chỉ thay đổi ở bước Xác nhận, không đổi ở bước phân tích.

## 2. Bản đồ tổng thể

```
  Trình duyệt (React + Vite, http://localhost:5173)
        |
        |  fetch("http://localhost:5195/api/...")   gọi thẳng API, API bật CORS cho mọi origin localhost
        v
  ASP.NET Core Web API (http://localhost:5195)
        |                       |                          |
        | EF Core               | đọc/ghi file             | HTTPS, kèm Gemini:ApiKey
        v                       v                          v
  SQL Server              wwwroot/uploads/           Gemini API (Google)
  (QcInspectionDb)        (ảnh đã tải lên,           model gemini-2.5-flash
                           phục vụ lại ở /uploads/...)
```

Trong API, một request đi qua 3 tầng:

```
  Controller  ------->  Service  ------->  AppDbContext (database)
  nhận request,         nghiệp vụ,          hoặc GeminiVisionService (AI)
  trả mã HTTP           kiểm dữ liệu        hoặc ImageStorageService (file ảnh)
```

Giao diện có 3 màn (thanh điều hướng trong `App.jsx`):

| Màn | Việc làm | API gọi |
|---|---|---|
| Bảng điều khiển | Tổng sản phẩm, tồn kho đạt/không đạt, số phiếu chờ xác nhận, thống kê loại lỗi, tồn kho từng sản phẩm, phiếu gần đây | `GET /api/dashboard`, `GET /api/products`, `GET /api/reports` |
| Tải ảnh kiểm tra | Gõ tên sản phẩm, chọn nhân viên, chọn ảnh, xem kết quả AI, xác nhận | `GET /api/products`, `GET /api/inspectors`, `POST /api/analyze`, `POST /api/confirm-report` |
| Lịch sử kiểm tra | Lọc, phân trang, xác nhận phiếu còn nháp | `GET /api/reports`, `GET /api/products`, `POST /api/confirm-report` |

## 3. Luồng chính: kiểm tra một ảnh

Khớp với sơ đồ 5 bước trong đề `Homework.pdf`.

```
  [1] QC trên React            [2] API                  [3] Gemini Vision
  --------------------         ----------------         ------------------
  gõ tên sản phẩm
  chọn nhân viên
  chọn ảnh
  bấm "Phân tích"  ------->    kiểm tên + nhân viên
       POST /api/analyze       kiểm ảnh, lưu file  ---> nhận ảnh (Base64) + prompt
                               chờ kết quả         <--- trả JSON: loại lỗi, mô tả,
                               server tính Pass/Fail         độ tin cậy
                               lưu PHIẾU NHÁP (chưa xác nhận)
  <------- 200 + kết quả
  [4] Màn xem lại
  đọc kết quả AI
  bấm "Xác nhận"   ------->    [5] Database transaction
       POST /api/confirm-report    đánh dấu phiếu đã xác nhận
                                   cộng 1 vào kho đạt / không đạt
                                   commit (lỗi bất kỳ thì hủy hết)
  <------- 200 + tồn kho mới
  Hiện toast. Bảng điều khiển tự tải lại số liệu mỗi lần mở.
```

## 4. Chi tiết `POST /api/analyze`

Body dạng `multipart/form-data`: `productName`, `inspectorId`, `image`. Code ở `Controllers/InspectionController.cs`.

```
  productName trống hoặc dài quá 200 ký tự?  ---------> 400
        | không
        v
  Tìm Product theo tên (bỏ khoảng trắng thừa,
  không phân biệt hoa thường, vẫn phân biệt dấu)
        | chưa có: chuẩn bị sản phẩm mới (danh mục "Chưa phân loại", kho 100),
        |          CHƯA lưu. Chỉ lưu cùng phiếu nháp khi Gemini phân tích xong
        v
  Tìm Inspector theo inspectorId  -------------------> 400 "Không tìm thấy nhân viên kiểm tra"
        | thấy
        v
  ImageStorageService.SaveImageAsync
    - đọc 12 byte đầu để nhận JPG / PNG / WEBP (không tin đuôi file hay ContentType)
    - tối đa 10 MB, file rỗng bị từ chối
    - lưu vào wwwroot/uploads với tên Guid  ----------> 400 nếu ảnh sai
        | lưu xong
        v
  Lấy danh mục lỗi từ DB, đọc ảnh lại thành byte
        v
  GeminiVisionService.AnalyzeImageAsync
    - gửi ảnh (Base64) + prompt tiếng Việt + danh mục lỗi
    - ép Gemini trả JSON theo schema
    - gặp 429 thì chờ 2 giây rồi 4 giây, tối đa thử lại 2 lần
    - ảnh mờ / không thấy sản phẩm  -----------------> 422 kèm lý do tiếng Việt
    - JSON sai cấu trúc, loại lỗi không có trong danh mục,
      mô tả rỗng, độ tin cậy ngoài 0-1 ---------------> 422
    - quá tải sau khi thử lại -----------------------> 429
    - Gemini không gọi được / từ chối ---------------> 502
    - key thiếu hoặc sai định dạng ------------------> 500
        | có kết quả
        v
  Server tính Pass/Fail từ mức độ của loại lỗi (mục 7)
        v
  Lưu InspectionReport với IsUserConfirmed = false (kèm Product mới nếu tên chưa có)
        v
  200 + reportId, productName, loại lỗi, mức độ, mô tả, qualityStatus, confidenceScore
```

Mọi nhánh lỗi sau khi file đã lưu đều xóa file ảnh đó (khối `finally`), nên `uploads/` không bị rác.

## 5. Chi tiết `POST /api/confirm-report`

Body: `{ "reportId": N }`. Code ở `Services/InspectionService.cs`.

```
  Tìm phiếu theo reportId  -----------------------------> 404 nếu không có
        | có
        v
  Mở transaction (IDbContextTransaction)
        v
  UPDATE phiếu SET IsUserConfirmed = 1
         WHERE ReportId = N AND IsUserConfirmed = 0
        |  0 dòng bị đổi (đã xác nhận rồi) -------------> 409, hủy transaction
        v
  UPDATE sản phẩm: Pass thì PassedStock + 1, Fail thì FailedStock + 1
         WHERE PassedStock + FailedStock < TotalStock
        |  0 dòng bị đổi (kho đã kiểm đủ) --------------> 409, hủy transaction,
        |                                                  bước trên cũng bị hủy theo
        v
  Đọc lại sản phẩm, commit
        v
  200 + reportId, qualityStatus, tồn kho mới (total / passed / failed)
```

Hai câu `UPDATE` có điều kiện trong `WHERE` giúp hai lần bấm gần như đồng thời không cộng kho hai lần.

## 6. Một phiếu kiểm đi qua những trạng thái nào

```
  (chưa có)  --- analyze thành công --->  NHÁP                --- confirm thành công --->  ĐÃ XÁC NHẬN
                                          IsUserConfirmed = 0                              IsUserConfirmed = 1
                                          kho chưa đổi                                     kho đã +1
                                                |
                                                +-- QC bỏ đi không xác nhận: phiếu vẫn nằm trong
                                                    Lịch sử với nhãn "Chưa xác nhận", bấm Xác nhận
                                                    ở Lịch sử để hoàn tất sau
```

Trên Bảng điều khiển, `pendingReports` đếm các phiếu nháp, còn thống kê loại lỗi chỉ tính phiếu đã xác nhận.

## 7. Quy tắc nghiệp vụ

| Quy tắc | Chi tiết | Nơi kiểm |
|---|---|---|
| Pass / Fail | Mức `None` và `Minor` là **Pass**, mức `Major` là **Fail**. Server tính, không tin kết quả AI tự ghi | `Services/QualityStatusRule.cs` |
| Ảnh không lỗi | Ảnh đạt thì loại lỗi là "Không lỗi" (mức `None`) | `Data/AppDbContext.cs` (seed) |
| Tồn kho | `PassedStock + FailedStock <= TotalStock`, kiểm ở bước Xác nhận | `Services/InspectionService.cs` |
| Xác nhận một lần | Phiếu đã xác nhận không xác nhận lại được | `Services/InspectionService.cs` |
| Tên sản phẩm | Gõ tên bất kỳ (tối đa 200 ký tự). Tên đã có thì dùng sản phẩm đó, tên chưa có thì tự tạo sản phẩm mới với kho 100 và danh mục "Chưa phân loại", lưu cùng phiếu nháp. Gõ khác dấu hoặc sai chính tả sẽ ra sản phẩm mới | `Controllers/InspectionController.cs` |
| Ảnh hợp lệ | JPG, PNG, WEBP, tối đa 10 MB, nhận diện bằng 12 byte đầu | `Services/ImageStorageService.cs` |

Những thứ chưa làm, có chủ ý: QC chưa sửa được kết quả AI trước khi xác nhận (đang chờ hỏi thầy), không có đăng nhập (chọn nhân viên từ danh sách), không có chức năng sửa hay xóa sản phẩm (sản phẩm chỉ được tạo từ seed hoặc tự tạo khi gõ tên mới).

## 8. Bảng mã HTTP

`POST /api/analyze`

| Mã | Khi nào |
|---|---|
| 200 | Phân tích xong, đã lưu phiếu nháp |
| 400 | Thiếu tên sản phẩm hoặc tên dài quá 200 ký tự, không có nhân viên, ảnh sai loại / quá 10 MB / rỗng |
| 422 | Ảnh mờ hoặc không thấy sản phẩm, hoặc AI trả kết quả không hợp lệ |
| 429 | Gemini quá tải, vẫn lỗi sau 2 lần thử lại |
| 502 | Không gọi được Gemini hoặc Gemini từ chối request |
| 500 | Thiếu hoặc sai `Gemini:ApiKey`, hoặc lỗi hệ thống khác |

`POST /api/confirm-report`

| Mã | Khi nào |
|---|---|
| 200 | Đã xác nhận, kho đã cập nhật |
| 404 | Không có phiếu với `reportId` đó |
| 409 | Phiếu đã xác nhận trước đó, hoặc sản phẩm đã kiểm đủ số lượng |
| 500 | Lỗi hệ thống |

Các `GET` (`products`, `inspectors`, `defect-categories`, `reports`, `reports/{id}`, `dashboard`) chỉ đọc dữ liệu. `reports` có lọc `productId`, `qualityStatus`, `isConfirmed` và phân trang (`pageSize` tối đa 50).

## 9. Muốn sửa gì thì mở file nào

Backend, trong `homework/homework/`:

| Muốn sửa | Mở file |
|---|---|
| Thứ tự kiểm tra, mã lỗi của analyze / confirm | `Controllers/InspectionController.cs` |
| Cách xác nhận phiếu và cộng kho | `Services/InspectionService.cs` |
| Prompt, model, số lần thử lại khi gặp 429 | `Services/GeminiVisionService.cs` |
| Luật nhận ảnh, giới hạn dung lượng, tên file | `Services/ImageStorageService.cs` |
| Luật Pass / Fail | `Services/QualityStatusRule.cs` |
| Danh mục lỗi, sản phẩm, nhân viên mẫu | `Data/AppDbContext.cs` (`HasData`), sau đó tạo migration |
| CORS, đăng ký service | `Program.cs` |
| Key Gemini, connection string | `appsettings.Local.json` (không đưa lên Git) |
| Mẫu request để thử | `homework.http` |

Frontend, trong `homework/homeworkclient/src/`:

| Muốn sửa | Mở file |
|---|---|
| Form tải ảnh, kiểm dữ liệu nhập | `components/UploadForm.jsx` |
| Màn xem lại kết quả AI | `components/PreviewForm.jsx` |
| Lịch sử, lọc, nút xác nhận phiếu nháp | `components/InspectionHistory.jsx` |
| Bảng điều khiển | `components/Dashboard.jsx` |
| Địa chỉ API, cách gọi và xử lý lỗi | `services/api.js` (đổi địa chỉ qua `VITE_API_BASE_URL`) |
| Màu, khoảng cách, kiểu ô nhập | `index.css`, `App.css` |
