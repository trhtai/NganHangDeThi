using NganHangDeThi.Common.Enum;
using NganHangDeThi.Common.Interfaces;
using System.ComponentModel.DataAnnotations.Schema;

namespace NganHangDeThi.Data.Entity;

public class CauHoi : IEntity<int>
{
    public int Id { get; set; }
    public string NoiDung { get; set; } = string.Empty;
    public MucDoCauHoi MucDo { get; set; } = MucDoCauHoi.NhanBiet;
    public LoaiCauHoi Loai { get; set; } = LoaiCauHoi.TracNghiemMotDapAn;
    public bool DaRaDe { get; set; } = false;
    public string? HinhAnh { get; set; }

    public int ChuongId { get; set; }
    public Chuong? Chuong { get; set; }

    // --- BỔ SUNG MỚI ---
    public int? ParentId { get; set; } // Null nếu là câu đơn hoặc câu cha
    public CauHoi? Parent { get; set; } // Link đến câu cha
    public ICollection<CauHoi> DsCauHoiCon { get; set; } = []; // Danh sách câu con
    // -------------------

    public ICollection<CauTraLoi> DsCauTraLoi { get; set; } = []; 
    
    
    [NotMapped]
    public bool IsSelected { get; set; } = false;
}
