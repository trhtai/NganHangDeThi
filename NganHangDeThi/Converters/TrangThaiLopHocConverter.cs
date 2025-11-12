using NganHangDeThi.Common.Enum;
using System.Globalization;
using System.Windows.Data;

namespace NganHangDeThi.Converters;

public class TrangThaiLopHocConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is TrangThaiLopHoc trangThai)
        {
            return trangThai switch
            {
                TrangThaiLopHoc.DangHoc => "Đang học",
                TrangThaiLopHoc.DaKetThuc => "Đã kết thúc",
                TrangThaiLopHoc.DaHuy => "Đã hủy",
                _ => "Không xác định"
            };
        }

        return "Không xác định";
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) 
    {
        throw new NotImplementedException();
    }
}
