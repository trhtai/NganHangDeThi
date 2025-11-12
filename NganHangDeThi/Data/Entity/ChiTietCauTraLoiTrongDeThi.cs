using NganHangDeThi.Common.Interfaces;

namespace NganHangDeThi.Data.Entity;

public class ChiTietCauTraLoiTrongDeThi : IEntity<int>
{
    public int Id { get; set; }

    public string NoiDung { get; set; } = string.Empty;
    public bool LaDapAnDung { get; set; }
    public byte ViTri { get; set; }
    public string? HinhAnh { get; set; }

    public int ChiTietDeThiId { get; set; }
    public ChiTietDeThi ChiTietDeThi { get; set; } = default!;
}
