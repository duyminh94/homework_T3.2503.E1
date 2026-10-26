# Thư mục chứa ảnh mẫu kiểm thử (Test Samples - R6)

Bỏ 4 file ảnh kiểm thử vào thư mục này để thực hiện test và quay video demo theo yêu cầu đề bài:

1. **`good.jpg`**
   - Loại ảnh: Sản phẩm đạt tiêu chuẩn, nguyên vẹn, không có khuyết tật.
   - Kết quả kỳ vọng: Danh mục `Không lỗi` (Severity: `None`), trạng thái **Pass**.

2. **`minor.jpg`**
   - Loại ảnh: Sản phẩm có lỗi nhỏ (trầy xước nhẹ bề mặt, vết ố bẩn nhỏ).
   - Kết quả kỳ vọng: Danh mục `Trầy xước bề mặt` / `Vết bẩn` (Severity: `Minor`), trạng thái **Pass**.

3. **`major.jpg`**
   - Loại ảnh: Sản phẩm có lỗi nặng (nứt vỡ, móp méo, biến dạng).
   - Kết quả kỳ vọng: Danh mục `Nứt vỡ` / `Móp méo biến dạng` (Severity: `Major`), trạng thái **Fail**.

4. **`blurry.jpg`**
   - Loại ảnh: Ảnh chụp bị rung tay, quá mờ, quá tối hoặc không thấy sản phẩm.
   - Kết quả kỳ vọng: API trả mã lỗi **HTTP 422** kèm lý do bằng tiếng Việt, không tạo phiếu kiểm định.
