using NganHangDeThi.Data.Entities.Interfaces;

namespace NganHangDeThi.Data.Entities;

public class Khoa : IEntity<int>, IAuditable
{
    public int Id { get; set; }
    public string MaKhoa { get; set; } = string.Empty;
    public string TenKhoa { get; set; } = string.Empty;
    public string MoTa { get; set; } = string.Empty;

    // Audit (Theo dõi lịch sử).
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    // Dùng để tối ưu Search, Sort, Filter (không dấu).
    public string TenKhoaUnSign { get; set; } = string.Empty;

    // Navigation properties (Khóa ngoại & Quan hệ các bảng).
    public ICollection<Lop> DanhSachLop { get; set; } = [];
    public ICollection<MonHoc> DanhSachMonHoc { get; set; } = [];
}