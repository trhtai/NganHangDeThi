using NganHangDeThi.Common.Enum;
using NganHangDeThi.Data.Entity;
using System.Windows;
using System.Windows.Input;

namespace NganHangDeThi;

public partial class ThemLopHocWindow : Window
{
    public LopHoc? LopHocMoi { get; private set; }
    private List<Khoa> _dsKhoa = new();

    public ThemLopHocWindow(List<Khoa> dsKhoa)
    {
        InitializeComponent();

        _dsKhoa = dsKhoa;
        CbbKhoa.ItemsSource = _dsKhoa;
    }

    public ThemLopHocWindow(LopHoc lopHocCanSua, List<Khoa> dsKhoa) : this(dsKhoa)
    {
        Title = "Sửa lớp học";

        TxtMaLop.Text = lopHocCanSua.MaLop;
        DpNgayBatDau.SelectedDate = lopHocCanSua.NgayBatDau.ToDateTime(TimeOnly.MinValue);
        DpNgayKetThuc.SelectedDate = lopHocCanSua.NgayKetThuc.ToDateTime(TimeOnly.MinValue);
        TxtNamHoc.Text = lopHocCanSua.NamHoc;
        TxtGVCN.Text = lopHocCanSua.GVCN;

        CbbKhoa.SelectedValue = lopHocCanSua.KhoaId;
    }

    private void BtnLuu_Click(object sender, RoutedEventArgs e)
    {
        LopHocMoi = new LopHoc
        {
            MaLop = TxtMaLop.Text.Trim(),
            NgayBatDau = DateOnly.FromDateTime(DpNgayBatDau.SelectedDate ?? DateTime.Today),
            NgayKetThuc = DateOnly.FromDateTime(DpNgayKetThuc.SelectedDate ?? DateTime.Today),
            TrangThai = TrangThaiLopHoc.DangHoc,
            NamHoc = TxtNamHoc.Text.Trim(),
            GVCN = TxtGVCN.Text.Trim(),
            CreatedAt = DateTime.Now,
            KhoaId = (int)CbbKhoa.SelectedValue
        };

        DialogResult = true;
        Close();
    }

    private void Button_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }

    private void Border_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
    {
        if (e.ChangedButton == MouseButton.Left)
        {
            DragMove();
        }
    }
}
