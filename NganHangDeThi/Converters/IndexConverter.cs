using System.Globalization;
using System.Windows.Controls;
using System.Windows.Data;

namespace NganHangDeThi.Converters
{
    public class IndexConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var row = value as DataGridRow;
            if (row != null)
            {
                return (row.GetIndex() + 1).ToString(); // bắt đầu từ 1
            }
            return "0";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
