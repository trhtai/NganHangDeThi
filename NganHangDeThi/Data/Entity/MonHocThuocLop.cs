using NganHangDeThi.Common.Interfaces;

namespace NganHangDeThi.Data.Entity;

public class MonHocThuocLop : IEntity<int>
{
    public int Id { get; set; }
    
    public int MonHocId { get; set; }
    public MonHoc MonHoc { get; set; } = default!;

    public int LopHocId { get; set; }
    public LopHoc LopHoc { get; set; } = default!;
}
