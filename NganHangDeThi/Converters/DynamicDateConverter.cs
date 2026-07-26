using NganHangDeThi.Helpers;
using System.Globalization;
using System.Windows.Data;

namespace NganHangDeThi.Converters;

public class DynamicDateConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is DateTime date)
        {
            string format = AppGlobalState.CurrentDateFormat ?? "dd/MM/yyyy HH:mm";
            return date.ToString(format);
        }

        return value;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException("Chỉ dùng để hiển thị (OneWay).");
    }
}
