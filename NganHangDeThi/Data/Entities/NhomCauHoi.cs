using NganHangDeThi.Data.Entities.Enums;
using NganHangDeThi.Data.Entities.Interfaces;

namespace NganHangDeThi.Data.Entities;

/// <summary>
/// Nhóm câu hỏi dùng chung 1 đoạn dữ liệu (case study / đoạn văn điền khuyết).
/// Theo tài liệu gốc: nhóm và toàn bộ câu hỏi con trong nhóm KHÔNG có mức độ.
/// </summary>
public class NhomCauHoi : IEntity<int>, IAuditable
{
    public int Id { get; set; }

    public LoaiNhomCauHoi LoaiNhom { get; set; }

    /// <summary>
    /// Đoạn dữ liệu dùng chung (rich text/HTML): câu dẫn "Dùng thông tin trả lời các câu {&lt;1&gt;} đến {&lt;n&gt;}"
    /// + nội dung đoạn văn/case study đi kèm.
    /// </summary>
    public string NoiDungDuLieuChung { get; set; } = string.Empty;

    // Audit (Theo dõi lịch sử).
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    // Navigation properties (Khóa ngoại & Quan hệ các bảng).
    public int ChuongId { get; set; }
    public Chuong Chuong { get; set; } = null!;

    public int? FileImportId { get; set; }
    public FileImport? FileImport { get; set; }

    public ICollection<CauHoi> DanhSachCauHoiCon { get; set; } = [];
    public ICollection<HinhAnhCauHoi> DanhSachHinhAnh { get; set; } = [];
}