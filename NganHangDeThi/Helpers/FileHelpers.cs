using System.IO;

namespace NganHangDeThi.Helpers;

public static class FileHelpers
{
    public static bool IsFileLocked(string filePath)
    {
        try
        {
            using var stream = File.Open(filePath, FileMode.Open, FileAccess.Read, FileShare.None);
            // Nếu mở được với FileShare.None → không bị khóa
            return false;
        }
        catch (IOException)
        {
            // Nếu bị IOException → file đang bị sử dụng bởi app khác (Word chẳng hạn)
            return true;
        }
    }
}
