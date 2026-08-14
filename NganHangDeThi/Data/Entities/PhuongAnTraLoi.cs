using NganHangDeThi.Data.Entities.Interfaces;

namespace NganHangDeThi.Data.Entities;

/// <summary>
/// Phương án trả lời của câu hỏi trắc nghiệm. Số lượng phương án linh hoạt (2-8), không cố định 4.
/// </summary>
public class PhuongAnTraLoi : IEntity<int>
{
    public int Id { get; set; }

    /// <summary>Ký tự nhãn A, B, C, D, ...</summary>
    public char KyTuNhan { get; set; }

    /// <summary>Nội dung phương án, rich text/HTML.</summary>
    public string NoiDung { get; set; } = string.Empty;

    /// <summary>true nếu ký tự nhãn có underline trong file gốc (đáp án đúng).</summary>
    public bool LaDapAnDung { get; set; }

    /// <summary>true nếu ký tự nhãn có italic trong file gốc (cố định vị trí khi trộn đề).</summary>
    public bool KhongHoanVi { get; set; }

    public int ThuTu { get; set; }

    // Navigation properties (Khóa ngoại & Quan hệ các bảng).
    public int CauHoiId { get; set; }
    public CauHoi CauHoi { get; set; } = null!;

    public ICollection<HinhAnhCauHoi> DanhSachHinhAnh { get; set; } = [];
}