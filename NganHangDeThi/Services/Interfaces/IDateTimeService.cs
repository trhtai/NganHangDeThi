namespace NganHangDeThi.Services.Interfaces;

public interface IDateTimeService
{
    /// <summary>
    /// Luôn luôn trả về giờ Việt Nam (UTC+7) bất chấp VPS đang ở múi giờ nào,
    /// bất chấp đang chạy ở nền tảng nào Windows/Linux
    /// </summary>
    DateTime GetVietnamTime();
}
