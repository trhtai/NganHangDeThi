using NganHangDeThi.Data.Entity;
using System.Windows;
using System.Windows.Input;

namespace NganHangDeThi;

public partial class ThemHoacSuaKhoaWindow : Window
{
    public Khoa? KhoaResult { get; private set; }

    public ThemHoacSuaKhoaWindow()
    {
        InitializeComponent();
    }

    // Constructor dùng cho việc Sửa
    public ThemHoacSuaKhoaWindow(Khoa khoaCanSua) : this()
    {
        TxtTitle.Text = "CẬP NHẬT KHOA";
        TxtMaKhoa.Text = khoaCanSua.MaKhoa;
        TxtTenKhoa.Text = khoaCanSua.TenKhoa;
    }

    private void BtnLuu_Click(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(TxtMaKhoa.Text) || string.IsNullOrWhiteSpace(TxtTenKhoa.Text))
        {
            MessageBox.Show("Vui lòng nhập đầy đủ Mã khoa và Tên khoa.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        KhoaResult = new Khoa
        {
            MaKhoa = TxtMaKhoa.Text.Trim(),
            TenKhoa = TxtTenKhoa.Text.Trim()
        };

        DialogResult = true;
        Close();
    }

    private void BtnHuy_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
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