using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Microsoft.Extensions.DependencyInjection;
using NganHangDeThi.MyUserControl;

namespace NganHangDeThi;

public partial class MainWindow : Window
{
    private readonly IServiceProvider _serviceProvider;

    public MainWindow(IServiceProvider serviceProvider)
    {
        InitializeComponent();
        FontFamily = new FontFamily("Tahoma");

        _serviceProvider = serviceProvider;

        MainContent.Content = _serviceProvider.GetRequiredService<RaDeControl>();
    }

    #region Events.
    private void MenuButtonClick(object sender, RoutedEventArgs e)
    {
        // Reset trạng thái tất cả button trong MenuPanel.
        foreach (var child in MenuPanel.Children)
        {
            if (child is Button btn)
            {
                btn.Tag = null;
            }
        }

        // Đánh dấu button được chọn là Active.
        var clickedButton = sender as Button;
        if (clickedButton != null)
        {
            clickedButton.Tag = "Active";
        }

        // Thay đổi nội dung chính.
        if (clickedButton == BtnRaDeThi)
        {
            MainContent.Content = _serviceProvider.GetRequiredService<RaDeControl>();
        }
        else if (clickedButton == BtnNganHangCauHoi)
        {
            MainContent.Content = _serviceProvider.GetRequiredService<NganHangCauHoiControl>();
        }
        else if (clickedButton == BtnQuanTriHeThong)
        {
            MainContent.Content = _serviceProvider.GetRequiredService<QuanTriHeThongControl>();
        }
    }

    // Di chuyển cửa sổ khi nhấn chuột trái vào MainBorder.
    private void MainBorder_MouseDown(object sender, MouseButtonEventArgs e)
    {
        if (e.ChangedButton == MouseButton.Left)
        {
            DragMove();
        }
    }

    // Phóng to hoặc thu nhỏ cửa sổ khi nhấp đúp vào MainBorder.
    private bool _isMaximized = false;
    private void MainBorder_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (e.ClickCount == 2)
        {
            if (_isMaximized)
            {
                WindowState = WindowState.Normal;
                Width = 1200;
                Height = 700;
                _isMaximized = false;
            }
            else
            {
                WindowState = WindowState.Maximized;
                _isMaximized = true;
            }
        }
    }
    private void BtnCloseMainWindow_Click(object sender, RoutedEventArgs e)
    {
        Application.Current.Shutdown();
    }

    #endregion
}