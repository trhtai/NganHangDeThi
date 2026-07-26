using NganHangDeThi.Data.Entities.Interfaces;

namespace NganHangDeThi.Data.Entities;

public class MonHoc : IEntity<int>, IAuditable
{
    public int Id { get; set; }
    public string TenMon { get; set; } = string.Empty;

    // Audit (Theo dõi lịch sử).
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    // Dùng để tối ưu Search, Sort, Filter (không dấu).
    public string TenMonUnSign { get; set; } = string.Empty;

    // Navigation properties (Khóa ngoại & Quan hệ các bảng).
    public int KhoaId { get; set; }
    public Khoa Khoa { get; set; } = null!;
    public ICollection<Chuong> DanhSachChuong { get; set; } = [];
}