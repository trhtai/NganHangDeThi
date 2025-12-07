using NganHangDeThi.Data.Entity;
using System.Windows;
using System.Windows.Input;

namespace NganHangDeThi;

public partial class ThemMonHocWindow : Window
{
    public MonHoc? MonHocMoi { get; private set; }
    private List<Khoa> _dsKhoa = new();

    public ThemMonHocWindow(List<Khoa> dsKhoa)
    {
        InitializeComponent();
        _dsKhoa = dsKhoa;
        CbbKhoa.ItemsSource = _dsKhoa;
    }

    public ThemMonHocWindow(MonHoc monHocCanSua, List<Khoa> dsKhoa) : this(dsKhoa)
    {
        Title = "Sửa môn học";
        TxtTenMon.Text = monHocCanSua.TenMon;
        CbbKhoa.SelectedValue = monHocCanSua.KhoaId;
    }

    private void BtnLuu_Click(object sender, RoutedEventArgs e)
    {
        MonHocMoi = new MonHoc
        {
            TenMon = TxtTenMon.Text.Trim(),
            KhoaId = (int)CbbKhoa.SelectedValue
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
