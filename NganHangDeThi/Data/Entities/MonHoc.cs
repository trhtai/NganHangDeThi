using NganHangDeThi.Data.Entities.Interfaces;

namespace NganHangDeThi.Data.Entities;

public class MonHoc : IEntity<int>, IAuditable
{
    public int Id { get; set; }
    public string TenMon { get; set; } = string.Empty;
    public string TenMonUnSign { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    public int KhoaId { get; set; }
    public Khoa Khoa { get; set; } = null!;

    public ICollection<Chuong> DanhSachChuong { get; set; } = [];
}