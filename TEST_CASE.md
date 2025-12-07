# Test cases
1. (PASSED) Kiểm tra loại câu hỏi trắc nghiệm 1 đáp án (kèm câu chùm) có hình ảnh
2. (PASSED) Kiểm tra loại câu hỏi trắc nghiệm nhiều đáp án (kèm câu chùm) có hình ảnh.	
3. (PASSED) Kiểm tra loại câu hỏi tự luận (kèm câu chùm) có hình ảnh).
4. (PASSED) Kiểm tra loại câu hỏi điền khuyết có hình ảnh.
5. (PASSED) Kiểm thử 1 đề thi tiếng anh hoàn chỉnh.
	+ Đảm bảo thứ tự câu hỏi
	+ (BUG) Đảm bảo định dạng câu hỏi như in đậm, gạch chân,...(bị mất in đậm)
	+ Kiểm tra đáp án có trộn không
6. (PASSED) Xuất đề thi gồm tất cả các dạng câu hỏi.
7. Kiểm tra file đáp án - điều chỉnh theo dạng bảng, không cần hiển thị toàn bộ câu hỏi.

# Bugs
1. (FIXED) Format cách hiển thị đáp án trong ThemCauHoiTuFileWindow
2. (FIXED) Chức năng sửa câu hỏi.
3. (FIXED) Chức năng xóa câu hỏi.
4. (FIXED) Xác định độ khó câu chùm: (lấy độ khó cao nhất trong các câu hỏi con)
5. (FIXED) Ngăn thêm câu hỏi trùng lặp
	+ so sánh nội dung câu hỏi
	+ có 1 vấn đề đó là nếu có hình ảnh thì mỗi lần import sẽ tạo ra 1 chuỗi guid khác nhau 
dẫn đến không thể phát hiện trùng lặp được
6. (FIXED) Việc xác định số lượng câu hỏi khi sinh đề thi:
	+ Với câu chùm: sẽ dựa vào số lượng câu hỏi con để tính chứ không xem 1 câu chùm là 1 đơn vị câu hỏi nữa.
	+ Với câu điền khuyết: sẽ thống kê số lượng câu con (hay số lượng chỗ trống) để tính số lượng câu hỏi
từ đó đề xuất cho user nhập số lượng câu điền khuyết dựa vào bội số của số lượng các câu con trong câu điền khuyết.
7. (FIXED) Cách hiển thị câu hỏi con, đáp án khi thêm đề từ file
8. (FIXED) Cách hiển thị danh sách câu hỏi trong NganHangCauHoiConotrol
9. (FIXED) Đổi "Câu 1: ..." thành "Question 1: ..."
	+ Nhưng môn học có tên là "Tiếng anh", "Anh văn", "English",... thì đổi
10. (FIXED) chỉ hiển thị các câu hỏi đơn, giả thuyết điền khuyết và giả khuyết câu chùm.
	Không hiển thị các câu hỏi con
11. (FIXED) Lọc câu hỏi theo môn mặc định
12. (FIXED) Lọc đề thi theo lớp học
13. (FIXED) Loai bỏ việc chọn môn học khi tạo đề thi từ ma trận, vì ma trận đã có môn học rồi
14. (FIXED) Tối ưu lại UX
	+ thêm môn và chương khi thêm ma trận
    + thêm lớp khi tạo đề thi 
    + thêm môn và chương khi thêm câu hỏi từ file
	
15. (FIXED) Xuất đáp án dạng bảng
16. (FIXED Có đề gốc, sau đó sinh các đề con từ đề gốc
	+ lưu lại đề con 
	+ câpj nhật đề gốc -> sinh đề con -> cập nhật đề con (HƠI VÔ LÍ)
	+ Có 2 loại đề: 
		- đề chỉ 1 bộ câu hỏi và trộn thứ tự các mã đề
		- mỗi mã đề có 1 bộ câu hỏi riêng biệt (TẠM THỜI BỎ QUA): user có thể tạo 4 đề riêng lẻ
		- 
17. Thêm câu hỏi thủ công
	+ loại bỏ việc thêm ảnh
	+ style lại
	
18. (FIXED) Làm sao để tìm đề thi 1 cách nhanh chóng
	+ search thông minh hơn (FilterDeThi)
	+ sắp xếp danh sách đề thi theo ngày tạo
19. (FIXED) Xem lại định dạng bold khi xuất đề thi
	+ Khi xuất đềthi thì định dạng bold bị mất, nhưng nếu giữ lai thì bị ghi đè lên đáp án,...
	+ Không thể sửa vì là lỗi trong word, 1 phần do thư viện docx không hỗ trợ tốt

20. (FIXED) Ngăn tạo trùng chương: hiện tại có thể thêm 2 chương 1
21. (FIXED) Style lại nút "thêm câu hỏi thủ công"
22. (FIXED) Chỉnh lại thông báo khi "thêm câu hỏi từ file", khi thêm thành công cần thông báo chính xác số câu hỏi,
cần đếm các câu đơn và các câu hỏi con trong câu chùm, không đếm phần giả thuyết
23. (FIXED) KHi sửa câu hỏi, nội dung show lên chưa được tô đỏ mà hiển thị "<$*> <span style='color:red'> although</span>"


- (DONE) Quy định lại về đề gốc, sẽ có 2 loại đề:
+ Đề hoán vị: tức chỉ có 1 bộ câu hỏi, các mã đề sẽ hoán vị câu hỏi với nhau để tạo thành các mã đề khác nhau và 
mã đề đầu tiên chính là đề gốc.
+ Đề ngẫu nhiên: tức mỗi mã đề sẽ có 1 bộ câu hỏi riêng biệt, mỗi mã đề là đề gốc.

- (DONE) Style lại giao diện thêm câu hỏi thủ công và kiểm thử.

- Fix lỗi item combobox hiển thị sai vị trí.

- (DONE) Tôi ưu hiệu năng các danh sách, việc lướt các danh sách dài bị lag khá nhiều.

- Quản lí các lớp và môn theo khoa, có thể lọc.