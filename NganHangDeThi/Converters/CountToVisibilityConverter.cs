using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace NganHangDeThi.Converters;

public sealed class CountToVisibilityConverter : IValueConverter
{
    /// <summary>ConverterParameter="Invert" => hiện khi count == 0 (dùng cho empty-state).</summary>
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        var count = value is int i ? i : 0;
        var invert = string.Equals(parameter as string, "Invert", StringComparison.OrdinalIgnoreCase);
        var visible = invert ? count == 0 : count > 0;
        return visible ? Visibility.Visible : Visibility.Collapsed;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}
