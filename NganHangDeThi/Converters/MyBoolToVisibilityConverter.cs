using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace NganHangDeThi.Converters;

public sealed class MyBoolToVisibilityConverter : IValueConverter
{
    // HÀM DỊCH XUÔI (Từ ViewModel -> Giao diện XAML)
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is bool isVisible)
        {
            return isVisible ? Visibility.Visible : Visibility.Collapsed;
        }

        return Visibility.Collapsed;
    }

    // HÀM DỊCH NGƯỢC (Từ Giao diện XAML -> ViewModel)
    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is Visibility visibility)
        {
            return visibility == Visibility.Visible;
        }

        return false;
    }
}
