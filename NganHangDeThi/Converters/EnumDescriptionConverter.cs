using NganHangDeThi.Extensions;
using System.Globalization;
using System.Windows.Data;

namespace NganHangDeThi.Converters;

public class EnumDescriptionConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return value is Enum e ? e.GetDescription() : value?.ToString() ?? "";
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

