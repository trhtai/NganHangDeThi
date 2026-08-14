namespace NganHangDeThi.Data.Entities.Enums;

/// <summary>
/// Phân biệt 2 kiểu trình bày của câu hỏi nhóm (dùng chung dữ liệu):
/// - DuLieuChung: đoạn dữ liệu/case study đứng trước, các câu hỏi con liệt kê sau (VD: bảng báo cáo tài chính).
/// - DienKhuyet: đoạn văn có các ô trống (<n>) chèn ngay giữa câu (cloze test).
/// Bản chất dữ liệu con (câu hỏi + đáp án) giống nhau, chỉ khác cách trình bày khối văn bản chung
/// khi hiển thị/export lại.
/// </summary>
public enum LoaiNhomCauHoi
{
    DuLieuChung = 1,
    DienKhuyet = 2
}