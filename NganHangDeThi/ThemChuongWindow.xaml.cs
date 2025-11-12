using NganHangDeThi.Data.Entity;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;

namespace NganHangDeThi;

public partial class ThemChuongWindow : Window
{
    public Chuong? ChuongMoi { get; set; }
    private readonly int _monHocId;

    public ThemChuongWindow(int monHocid)
    {
        InitializeComponent();
        _monHocId = monHocid;
    }

    public ThemChuongWindow(int monHocId, Chuong chuongCanSua) : this(monHocId)
    {
        Title = "SỬA CHƯƠNG";

        TxtViTriChuong.Text = chuongCanSua.ViTri.ToString();
        TxtTenChuong.Text = chuongCanSua.TenChuong;

    }

    private void BtnLuu_Click(object sender, RoutedEventArgs e)
    {
        if (int.TryParse(TxtViTriChuong.Text, out int viTriChuongMoi))
        {
            ChuongMoi = new Chuong
            {
                ViTri = viTriChuongMoi,
                TenChuong = TxtTenChuong.Text,
                MonHocId = _monHocId
            };
            DialogResult = true;
            Close();
        } else
        {
            MessageBox.Show("Vị trí chương là số nguyên");
        }
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
