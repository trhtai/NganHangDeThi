using System.IO;

namespace NganHangDeThi.Data;

public class DbPathProvider
{
    // %AppData%/NganHangDeThi/nganhangdethi.db
    public static string DatabasePath
    {
        get
        {
            var folder = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "NganHangDeThi");

            Directory.CreateDirectory(folder);

            return Path.Combine(folder, "nganhangdethi.db");
        }
    }

    public static string ConnectionString => $"Data Source={DatabasePath}";
}
