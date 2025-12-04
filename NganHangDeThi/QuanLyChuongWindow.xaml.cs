using Microsoft.EntityFrameworkCore;
using NganHangDeThi.Data.DataContext;
using NganHangDeThi.Data.Entity;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace NganHangDeThi;

public partial class QuanLyChuongWindow : Window, INotifyPropertyChanged
{
    private readonly AppDbContext _db;
    public ObservableCollection<Chuong> DsChuong { get; set; } = [];

    public MonHoc? MonHoc { get; set; }

    public QuanLyChuongWindow(AppDbContext dbContext, int monHocId)
    {
        InitializeComponent();
        DataContext = this;
        _db = dbContext;
        LoadMonHocKemDschuong(monHocId);
    }

    private void LoadMonHocKemDschuong(int monHocId)
    {
        MonHoc = _db.MonHoc.Include(m => m.DsChuong).Where(m => m.Id == monHocId).FirstOrDefault();
        if (MonHoc == null)
        {
            MessageBox.Show("Tải môn học bị lỗi!", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            Close();
        }
        DsChuong = new ObservableCollection<Chuong>(MonHoc!.DsChuong);
        OnPropertyChanged(nameof(DsChuong));
    }

    private void BtnTaiLai_Click(object sender, RoutedEventArgs e)
    {
        LoadMonHocKemDschuong(MonHoc!.Id);
    }

    private void BtnThemChuong_Click(object sender, RoutedEventArgs e)
    {
        var themChuong = new ThemChuongWindow(_db, MonHoc!.Id)
        {
            Owner = Window.GetWindow(this)
        };

        var result = themChuong.ShowDialog();
        if (result == true && themChuong.ChuongMoi != null)
        {
            _db.Chuong.Add(themChuong.ChuongMoi);
            _db.SaveChanges();
            LoadMonHocKemDschuong(MonHoc!.Id);
        }
    }

    private void BtnSuaChuong_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button btn && btn.Tag is Chuong chuong)
        {
            var suaWindow = new ThemChuongWindow(_db, MonHoc!.Id, chuong)
            {
                Owner = Window.GetWindow(this)
            };

            var result = suaWindow.ShowDialog();
            if (result == true && suaWindow.ChuongMoi != null)
            {
                var monHocCanSua = _db.Chuong.FirstOrDefault(x => x.Id == chuong.Id);
                if (monHocCanSua == null)
                {
                    MessageBox.Show("Chương không tồn tại hoặc đã bị xóa.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }
                monHocCanSua.ViTri = suaWindow.ChuongMoi.ViTri;
                monHocCanSua.TenChuong = suaWindow.ChuongMoi.TenChuong;

                _db.SaveChanges();

                LoadMonHocKemDschuong(MonHoc!.Id);
            }
        }
    }

    private void BtnXoaChuonog_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button btn && btn.Tag is Chuong chuong)
        {
            var confirm = MessageBox.Show($"Bạn có chắc muốn xóa chương \"{chuong.TenChuong}\"?", "Xác nhận xóa", MessageBoxButton.YesNo, MessageBoxImage.Warning);

            if (confirm == MessageBoxResult.Yes)
            {
                var chuongCanXoa = _db.Chuong.FirstOrDefault(x => x.Id == chuong.Id);
                if (chuongCanXoa == null)
                {
                    MessageBox.Show("Chương không tồn tại hoặc đã bị xóa.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                _db.Chuong.Remove(chuongCanXoa);
                _db.SaveChanges();

                LoadMonHocKemDschuong(MonHoc!.Id);
            }
        }
    }

    private void Border_MouseDown(object sender, MouseButtonEventArgs e)
    {
        if (e.ChangedButton == MouseButton.Left)
        {
            DragMove();
        }
    }

    private void BtnDongForm_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }

    // Triển khai INotifyPropertyChanged.
    public event PropertyChangedEventHandler? PropertyChanged;
    protected void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
