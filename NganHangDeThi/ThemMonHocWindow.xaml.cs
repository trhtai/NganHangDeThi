using NganHangDeThi.Data.Entity;
using System.Windows;
using System.Windows.Input;

namespace NganHangDeThi;

public partial class ThemMonHocWindow : Window
{
    public MonHoc? MonHocMoi { get; private set; }

    public ThemMonHocWindow()
    {
        InitializeComponent();
    }

    public ThemMonHocWindow(MonHoc monHocCanSua) : this()
    {
        Title = "Sửa môn học";
        TxtTenMon.Text = monHocCanSua.TenMon;
    }

    private void BtnLuu_Click(object sender, RoutedEventArgs e)
    {
        MonHocMoi = new MonHoc
        {
            TenMon = TxtTenMon.Text.Trim(),
        };

        DialogResult = true;
        Close();
    }

    private void Button_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }

    private void Border_MouseDown(object sender, MouseButtonEventArgs e)
    {
        if (e.ChangedButton == MouseButton.Left)
        {
            DragMove();
        }
    }
}
