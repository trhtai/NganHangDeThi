using NganHangDeThi.Data.Entities.Interfaces;

namespace NganHangDeThi.Data.Entities;

public class Khoa : IEntity<int>, IAuditable
{
    public int Id { get; set; }
    public string MaKhoa { get; set; } = string.Empty;
    public string TenKhoa { get; set; } = string.Empty;
    public string MoTa { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    public ICollection<Lop> DanhSachLop { get; set; } = [];
    public ICollection<MonHoc> DanhSachMonHoc { get; set; } = [];
}