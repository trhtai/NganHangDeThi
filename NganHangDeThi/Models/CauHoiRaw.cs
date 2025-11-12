using NganHangDeThi.Common.Enum;

namespace NganHangDeThi.Models;

public record CauHoiRaw(string NoiDung, MucDoCauHoi MucDo, LoaiCauHoi Loai, List<CauTraLoiRaw> DapAn, string? HinhAnh = null);
