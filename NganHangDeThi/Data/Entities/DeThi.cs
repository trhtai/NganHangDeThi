using NganHangDeThi.Data.Entities.Enums;
using NganHangDeThi.Data.Entities.Interfaces;

namespace NganHangDeThi.Data.Entities;

/// <summary>
/// 1 đề thi cụ thể được sinh ra từ 1 ma trận đề thi (1 ma trận có thể sinh nhiều mã đề khác nhau
/// để trộn đề). Tách khỏi CauHoi để vòng đời ngân hàng câu hỏi và đề thi độc lập nhau — sửa/xoá
/// câu hỏi trong ngân hàng sau này không ảnh hưởng đề thi đã sinh trước đó.
/// </summary>
public class DeThi : IEntity<int>, IAuditable
{
    public int Id { get; set; }

    /// <summary>"Đề 001", "Mã đề 132"...</summary>
    public string MaDe { get; set; } = string.Empty;

    public TrangThaiDeThi TrangThai { get; set; }

    // Audit (Theo dõi lịch sử).
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    // Navigation properties (Khóa ngoại & Quan hệ các bảng).
    public int MaTranDeThiId { get; set; }
    public MaTranDeThi MaTranDeThi { get; set; } = null!;

    public int? LopId { get; set; }
    public Lop? Lop { get; set; }

    public int? HocKyId { get; set; }
    public HocKy? HocKy { get; set; }

    public ICollection<DeThiCauHoi> DanhSachCauHoi { get; set; } = [];
}