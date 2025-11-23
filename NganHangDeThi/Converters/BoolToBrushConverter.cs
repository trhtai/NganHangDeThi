using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace NganHangDeThi.Converters;

public class BoolToBrushConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        // Nếu là True (Đáp án đúng) -> Trả về màu Đỏ, ngược lại màu Đen
        return (value is true) ? Brushes.Red : Brushes.Black;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
