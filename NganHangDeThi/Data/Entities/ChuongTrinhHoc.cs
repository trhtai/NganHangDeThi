using NganHangDeThi.Data.Entities.Interfaces;

namespace NganHangDeThi.Data.Entities;

/// <summary>
/// Bảng trung gian giữa lớp học và môn học.
/// </summary>
public class ChuongTrinhHoc : IEntity<int>
{
    public int Id { get; set; }
    public int NamHoc { get; set; }

    public int LopId { get; set; }
    public Lop Lop { get; set; } = null!;

    public int MonHocId { get; set; }
    public MonHoc MonHoc { get; set; } = null!;
}
