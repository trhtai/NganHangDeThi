using NganHangDeThi.Data.Entities.Interfaces;

namespace NganHangDeThi.Data.Entities;

public class HocKy : IEntity<int>, IAuditable
{
    public int Id { get; set; }
    public string TenHocKy { get; set; } = string.Empty; // "Học kỳ 1", "Học kỳ 2", "Học kỳ Hè"

    // Audit (Theo dõi lịch sử).
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    // Navigation properties (Khóa ngoại & Quan hệ các bảng).
    public int NienKhoaId { get; set; }
    public NienKhoa NienKhoa { get; set; } = null!;
}
