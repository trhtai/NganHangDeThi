using NganHangDeThi.Common.Interfaces;

namespace NganHangDeThi.Data.Entity;

public class Chuong : IEntity<int>
{
    public int Id { get; set; }
    public int ViTri { get; set; }
    public string TenChuong { get; set; } = string.Empty;

    public int MonHocId { get; set; }
    public MonHoc MonHoc { get; set; } = default!;

    public ICollection<CauHoi> DsCauHoi { get; set; } = [];
}
