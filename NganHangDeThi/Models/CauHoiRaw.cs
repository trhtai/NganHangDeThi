using NganHangDeThi.Common.Enum;

namespace NganHangDeThi.Models;

//public record CauHoiRaw(string NoiDung, MucDoCauHoi MucDo, LoaiCauHoi Loai, List<CauTraLoiRaw> DapAn, string? HinhAnh = null);

public class CauHoiRaw
{
    public string NoiDung { get; set; } = string.Empty;
    public MucDoCauHoi MucDo { get; set; }
    public LoaiCauHoi Loai { get; set; }
    public List<CauTraLoiRaw> DapAn { get; set; } = new();
    public string? HinhAnh { get; set; }

    // Hỗ trợ câu chùm
    public List<CauHoiRaw> CauHoiCon { get; set; } = new();
    public bool LaCauChum => CauHoiCon.Any();
}
