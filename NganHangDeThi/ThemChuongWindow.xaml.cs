using NganHangDeThi.Data.DataContext;
using NganHangDeThi.Data.Entity;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;

namespace NganHangDeThi;

public partial class ThemChuongWindow : Window
{
    public Chuong? ChuongMoi { get; set; }
    private readonly int _monHocId; 
    private readonly AppDbContext _db;
    private readonly int _currentId = 0; // Biến để lưu Id nếu đang ở chế độ Sửa

    public ThemChuongWindow(AppDbContext db, int monHocid)
    {
        InitializeComponent();
        _db = db;
        _monHocId = monHocid;
    }

    public ThemChuongWindow(AppDbContext db, int monHocId, Chuong chuongCanSua) : this(db, monHocId)
    {
        Title = "SỬA CHƯƠNG";

        TxtViTriChuong.Text = chuongCanSua.ViTri.ToString();
        TxtTenChuong.Text = chuongCanSua.TenChuong;
        _currentId = chuongCanSua.Id; // Lưu lại ID đang sửa

    }

    private void BtnLuu_Click(object sender, RoutedEventArgs e)
    {
        if (int.TryParse(TxtViTriChuong.Text, out int viTriChuongMoi))
        {
            // --- LOGIC KIỂM TRA TRÙNG LẶP ---

            // 1. Kiểm tra trùng Tên chương trong cùng môn học
            // (Loại trừ chính nó nếu đang sửa: c.Id != _currentId)
            bool isDuplicateName = _db.Chuong.Any(c =>
                c.MonHocId == _monHocId &&
                c.Id != _currentId &&
                c.TenChuong.ToLower() == TxtTenChuong.Text.ToLower());

            if (isDuplicateName)
            {
                MessageBox.Show($"Chương \"{TxtTenChuong.Text}\" đã tồn tại trong môn học này!", "Trùng dữ liệu", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // 2. Kiểm tra trùng Vị trí chương (nếu bạn muốn bắt buộc vị trí duy nhất)
            bool isDuplicatePos = _db.Chuong.Any(c =>
                c.MonHocId == _monHocId &&
                c.Id != _currentId &&
                c.ViTri == viTriChuongMoi);

            if (isDuplicatePos)
            {
                MessageBox.Show($"Vị trí số {viTriChuongMoi} đã được sử dụng bởi chương khác!", "Trùng dữ liệu", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            // --------------------------------

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
