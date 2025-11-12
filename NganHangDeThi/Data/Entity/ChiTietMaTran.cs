using NganHangDeThi.Common.Enum;
using NganHangDeThi.Common.Interfaces;

namespace NganHangDeThi.Data.Entity;

public class ChiTietMaTran : IEntity<int>
{
    public int Id { get; set; }
    public int SoCau { get; set; }
    public MucDoCauHoi MucDoCauHoi { get; set; }
    public LoaiCauHoi LoaiCauHoi { get; set; }

    public int ChuongId { get; set; }
    public Chuong Chuong { get; set; } = default!;

    public int MaTranId { get; set; }
    public MaTran MaTran { get; set; } = default!;
}
