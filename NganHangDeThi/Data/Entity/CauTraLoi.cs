using NganHangDeThi.Common.Interfaces;

namespace NganHangDeThi.Data.Entity;

public class CauTraLoi : IEntity<int>
{
    public int Id { get; set; }
    public string NoiDung { get; set; } = string.Empty;
    public bool LaDapAnDung { get; set; } = false;
    public byte ViTriGoc { get; set; }
    public bool DaoViTri { get; set; } = false;
    public string? HinhAnh { get; set; }

    public int CauHoiId { get; set; }
    public CauHoi? CauHoi { get; set; }
}
