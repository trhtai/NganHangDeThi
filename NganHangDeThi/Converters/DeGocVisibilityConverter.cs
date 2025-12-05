using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace NganHangDeThi.Converters;

public class DeGocVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is string ghiChu && !string.IsNullOrEmpty(ghiChu))
        {
            // Kiểm tra xem trong ghi chú có cụm từ đánh dấu đề gốc không
            if (ghiChu.Contains("(Đề gốc)", StringComparison.OrdinalIgnoreCase))
            {
                return Visibility.Visible;
            }
        }
        return Visibility.Collapsed;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}