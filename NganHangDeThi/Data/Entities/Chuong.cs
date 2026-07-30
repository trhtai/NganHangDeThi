using NganHangDeThi.Data.Entities.Interfaces;

namespace NganHangDeThi.Data.Entities;

public class Chuong : IEntity<int>, IAuditable
{
    public int Id { get; set; }
    public string TenChuong { get; set; } = null!;
    public int ThuTu { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    public string TenChuongUnsign { get; set; } = string.Empty;

    public int MonHocId { get; set; }
    public MonHoc MonHoc { get; set; } = null!;
}
