using NganHangDeThi.Services.Interfaces;
using System.Runtime.InteropServices;

namespace NganHangDeThi.Services;

public class DateTimeService : IDateTimeService
{
    public DateTime GetVietnamTime()
    {
        // 1. Dùng UTC làm chuẩn gốc (luôn đúng trên mọi server)
        DateTime utcNow = DateTime.UtcNow;

        // 2. Xác định tên múi giờ dựa theo Hệ điều hành
        string timeZoneId = RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
            ? "SE Asia Standard Time" // Tên múi giờ VN trên Windows
            : "Asia/Ho_Chi_Minh";     // Tên múi giờ VN trên Linux (Ubuntu)

        // 3. Lấy thông tin múi giờ và chuyển đổi
        var vnTimeZone = TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);
        return TimeZoneInfo.ConvertTimeFromUtc(utcNow, vnTimeZone);
    }
}
