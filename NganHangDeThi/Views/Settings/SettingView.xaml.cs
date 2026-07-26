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
}
