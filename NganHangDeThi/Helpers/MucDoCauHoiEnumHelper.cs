using NganHangDeThi.Common.Enum;
using NganHangDeThi.Extensions;

namespace NganHangDeThi.Helpers;

public static class MucDoCauHoiEnumHelper
{
    public static Dictionary<MucDoCauHoi, string> DanhSach => Enum.GetValues(typeof(MucDoCauHoi))
        .Cast<MucDoCauHoi>()
        .ToDictionary(x => x, x => x.GetDescription() ?? x.ToString());
}

