using System.Globalization;
using System.Net;
using System.Text.RegularExpressions;
using System.Windows.Data;

namespace NganHangDeThi.Converters;

public class HtmlDisplayConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is string html)
        {
            // 1. Giải mã HTML Entities (&#243; -> ó)
            string decoded = WebUtility.HtmlDecode(html);

            // 2. Thay thế <br/> bằng xuống dòng để hiển thị đẹp trong TextBlock
            decoded = decoded.Replace("<br/>", "\n").Replace("<br>", "\n");

            // 3. (Tùy chọn) Loại bỏ các tag HTML còn lại (<b>, <sub>...) để grid nhìn sạch sẽ hơn
            // Nếu bạn muốn giữ lại text thuần túy để xem nhanh:
            decoded = Regex.Replace(decoded, "<.*?>", string.Empty);

            return decoded;
        }
        return string.Empty;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
