using NganHangDeThi.Common.Interfaces;

namespace NganHangDeThi.Data.Entity;

public class ChiTietDeThi : IEntity<int>
{
    public int Id { get; set; }

    public int CauHoiId { get; set; }
    public CauHoi CauHoi { get; set; } = default!;

    public int DeThiId { get; set; }
    public DeThi DeThi { get; set; } = default!;
    public ICollection<ChiTietCauTraLoiTrongDeThi> DsDapAnTrongDe { get; set; } = [];
}
