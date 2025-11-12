using NganHangDeThi.Data.Entity;

namespace NganHangDeThi.Models;

public class DeThiExportData
{
    public DeThi DeThi { get; set; } = new DeThi();
    public List<(CauHoi CauHoi, List<CauTraLoi> DapAnDaTron)> CauHoiVaDapAn { get; set; } = [];
}
