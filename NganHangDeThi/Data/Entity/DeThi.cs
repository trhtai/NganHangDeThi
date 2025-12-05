using NganHangDeThi.Common.Interfaces;

namespace NganHangDeThi.Data.Entity;

public class DeThi : IEntity<int>
{
    public int Id { get; set; }
    public int MaDe { get; set; }
    public string KyThi { get; set; } = string.Empty;
    public string TieuDe { get; set; } = string.Empty;
    public int ThoiGianLamBai { get; set; }
    public string GhiChu { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public bool DaThi { get; set; } = false;
    public Guid BatchId { get; set; } // Dùng để gom nhóm đề thi trong cùng một lần tạo

    public int MonHocId { get; set; }
    public MonHoc? MonHoc { get; set; }

    public int MaTranId { get; set; }
    public MaTran? MaTran { get; set; }

    public int LopHocId { get; set; }
    public LopHoc? LopHoc { get; set; }

    public ICollection<ChiTietDeThi> DsChiTietDeThi { get; set; } = []; 
}
