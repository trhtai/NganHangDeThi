using NganHangDeThi.Services.Interfaces;
using System.Windows;

namespace NganHangDeThi.Services;

public class HandyConfirmService : IConfirmService
{
    public bool Confirm(string message, string title = "Xác nhận")
    {
        var result = HandyControl.Controls.MessageBox.Show(
            message, 
            title, 
            MessageBoxButton.YesNo, 
            MessageBoxImage.Warning);

        return result == MessageBoxResult.Yes;
    }
}
