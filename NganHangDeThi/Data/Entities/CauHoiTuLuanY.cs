using NganHangDeThi.Data.Entities.Interfaces;

namespace NganHangDeThi.Data.Entities;

/// <summary>
/// 1 dòng trong bảng đáp án tự luận (Nội dung đáp án / Thang điểm). Tên "Ý n" không bắt buộc đúng
/// chữ — giảng viên có thể đặt tên tự do (VD "Câu a"), chỉ bắt buộc cấu trúc bảng 2 cột.
/// Cho phép chỉ có 1 dòng (không chia nhỏ ý) vẫn hợp lệ.
/// </summary>
public class CauHoiTuLuanY : IEntity<int>
{
    public int Id { get; set; }

    public string TenY { get; set; } = string.Empty; // "Ý 1", "Câu a"...

    /// <summary>Nội dung đáp án, rich text/HTML để giữ công thức (subscript/superscript), bold/italic.</summary>
    public string NoiDung { get; set; } = string.Empty;

    public decimal ThangDiem { get; set; }

    public int ThuTu { get; set; }

    // Navigation properties (Khóa ngoại & Quan hệ các bảng).
    public int CauHoiId { get; set; }
    public CauHoi CauHoi { get; set; } = null!;
}