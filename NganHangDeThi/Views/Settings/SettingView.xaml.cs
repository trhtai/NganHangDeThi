using HandyControl.Interactivity;
using NganHangDeThi.ViewModels.Settings;
using System.Windows.Controls;

namespace NganHangDeThi.Views.Settings;

public partial class SettingView : UserControl
{
    public SettingView(SettingViewModel vm)
    {
        InitializeComponent();

        DataContext = vm;
    }

    private void CloseButton_Click(object sender, System.Windows.RoutedEventArgs e)
    {
        // Bắn tín hiệu đóng Dialog lên cho HandyControl xử lý
        ControlCommands.Close.Execute(null, this);
    }
}
