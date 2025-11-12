using NganHangDeThi.Common.Enum;
using NganHangDeThi.Extensions;

namespace NganHangDeThi.Helpers;

public static class LoaiCauHoiEnumHelper
{
    public static Dictionary<LoaiCauHoi, string> DanhSach => Enum.GetValues(typeof(LoaiCauHoi))
        .Cast<LoaiCauHoi>()
        .ToDictionary(x => x, x => x.GetDescription() ?? x.ToString());
}
