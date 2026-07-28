using NganHangDeThi.Data.Entities.Interfaces;

namespace NganHangDeThi.Data.Entities;

public class Lop : IEntity<int>, IAuditable
{
    public int Id { get; set; }
    public string MaLop { get; set; } = null!;

    // Audit (Theo dõi lịch sử).
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    // Navigation properties (Khóa ngoại & Quan hệ các bảng).
    public int KhoaId { get; set; }
    public Khoa Khoa { get; set; } = null!;

    public ICollection<ChuongTrinhHoc> DanhSachMonHoc { get; set; } = [];
}
