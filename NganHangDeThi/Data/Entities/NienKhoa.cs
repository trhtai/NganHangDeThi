using NganHangDeThi.Data.Entities.Interfaces;

namespace NganHangDeThi.Data.Entities;

public class NienKhoa : IEntity<int>, IAuditable
{
    public int Id { get; set; }
    public string TenNienKhoa { get; set; } = null!;
    public int NamNhapHoc { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    public ICollection<Lop> DanhSachLop { get; set; } = [];
}