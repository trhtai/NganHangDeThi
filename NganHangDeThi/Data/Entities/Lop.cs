using NganHangDeThi.Data.Entities.Interfaces;

namespace NganHangDeThi.Data.Entities;

public class Lop : IEntity<int>, IAuditable
{
    public int Id { get; set; }
    public string MaLop { get; set; } = null!;
    public string TenLop { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    public int KhoaId { get; set; }
    public Khoa Khoa { get; set; } = null!;

    public int NienKhoaId { get; set; }
    public NienKhoa NienKhoa { get; set; } = null!;

    public ICollection<ChuongTrinhHoc> DanhSachMonHoc { get; set; } = [];
}
