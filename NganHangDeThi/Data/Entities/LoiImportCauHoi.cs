using NganHangDeThi.Data.Entities.Interfaces;

namespace NganHangDeThi.Data.Entities;

/// <summary>
/// 1 lỗi phát hiện khi parse 1 câu hỏi cụ thể trong file import (VD: ảnh sai Wrap Text, thiếu
/// [&lt;br&gt;], bảng đáp án tự luận không đúng cấu trúc 2 cột...). Giữ lại đoạn văn bản gốc để
/// giảng viên/phòng khảo thí đối chiếu sửa nhanh.
/// </summary>
public class LoiImportCauHoi : IEntity<int>
{
    public int Id { get; set; }

    /// <summary>Thứ tự câu hỏi thứ mấy trong file gốc (để người dùng dễ tìm lại trong Word).</summary>
    public int ViTriCauTrongFile { get; set; }

    public string NoiDungLoi { get; set; } = string.Empty;

    public string DoanVanBanGoc { get; set; } = string.Empty;

    // Navigation properties (Khóa ngoại & Quan hệ các bảng).
    public int FileImportId { get; set; }
    public FileImport FileImport { get; set; } = null!;
}