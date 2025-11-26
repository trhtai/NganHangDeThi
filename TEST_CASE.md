# Test cases
1. (PASSED) Kiểm tra loại câu hỏi trắc nghiệm 1 đáp án (kèm câu chùm) có hình ảnh
2. (PASSED) Kiểm tra loại câu hỏi trắc nghiệm nhiều đáp án (kèm câu chùm) có hình ảnh.	
3. (PASSED) Kiểm tra loại câu hỏi tự luận (kèm câu chùm) có hình ảnh).
4. (PASSED) Kiểm tra loại câu hỏi điền khuyết có hình ảnh.
5. Kiểm thử 1 đề thi tiếng anh hoàn chỉnh.
	+ Đảm bảo thứ tự câu hỏi
	+ Đảm bảo định dạng câu hỏi như in đậm, gạch chân,...
	+ Kiểm tra đáp án có trộn không
6. Xuất đề thi gồm tất cả các dạng câu hỏi.
7. Kiểm tra file đáp án.

# Bugs
1. (FIXED) Format cách hiển thị đáp án trong ThemCauHoiTuFileWindow
2. Chức năng sửa câu hỏi.
3. Chức năng xóa câu hỏi.
4. (FIXED) Xác định độ khó câu chùm: (lấy độ khó cao nhất trong các câu hỏi con)
5. (FIXED) Ngăn thêm câu hỏi trùng lặp
	+ so sánh nội dung câu hỏi
	+ có 1 vấn đề đó là nếu có hình ảnh thì mỗi lần import sẽ tạo ra 1 chuỗi guid khác nhau 
dẫn đến không thể phát hiện trùng lặp được
6. (FIXED) Việc xác định số lượng câu hỏi khi sinh đề thi:
	+ Với câu chùm: sẽ dựa vào số lượng câu hỏi con để tính chứ không xem 1 câu chùm là 1 đơn vị câu hỏi nữa.
	+ Với câu điền khuyết: sẽ thống kê số lượng câu con (hay số lượng chỗ trống) để tính số lượng câu hỏi
từ đó đề xuất cho user nhập số lượng câu điền khuyết dựa vào bội số của số lượng các câu con trong câu điền khuyết.
7. Cách hiển thị câu hỏi con, đáp án khi thêm đề từ file
8. Cách hiển thị danh sách câu hỏi trong NganHangCauHoiConotrol