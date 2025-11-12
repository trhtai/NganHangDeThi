using NganHangDeThi.Data.DataContext;
using NganHangDeThi.Data.Entity;
using NganHangDeThi.Repository.UnitOfWorks;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace NganHangDeThi.MyUserControl;

public partial class QuanTriHeThongControl : UserControl, INotifyPropertyChanged
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly AppDbContext _appDbContext;

    private bool _daTaiLopHoc = false;
    private bool _daTaiMonHoc = false;
    private ObservableCollection<LopHoc> _dsLopHoc = [];
    private ObservableCollection<MonHoc> _dsMonHoc = [];
    private ICollectionView _lopHocView;
    private ICollectionView _monHocView;
    public ICollectionView LopHocView => _lopHocView;
    public ICollectionView MonHocView => _monHocView;

    private string _tuKhoaTimKiemLopHoc = string.Empty;
    private string _tuKhoaTimKiemMonHoc = string.Empty;

    public string TuKhoaTimKiemLopHoc
    {
        get => _tuKhoaTimKiemLopHoc;
        set
        {
            if (_tuKhoaTimKiemLopHoc != value)
            {
                _tuKhoaTimKiemLopHoc = value;
                OnPropertyChanged(nameof(TuKhoaTimKiemLopHoc));
                LopHocView.Refresh();
            }
        }
    }

    public string TuKhoaTimKiemMonHoc
    {
        get => _tuKhoaTimKiemMonHoc;
        set
        {
            if (_tuKhoaTimKiemMonHoc != value)
            {
                _tuKhoaTimKiemMonHoc = value;
                OnPropertyChanged(nameof(TuKhoaTimKiemMonHoc));
                MonHocView.Refresh();
            }
        }
    }

    public QuanTriHeThongControl(IUnitOfWork unitOfWork, AppDbContext dbContext)
    {
        InitializeComponent();
        DataContext = this;

        _lopHocView = CollectionViewSource.GetDefaultView(_dsLopHoc);
        OnPropertyChanged(nameof(LopHocView));

        _monHocView = CollectionViewSource.GetDefaultView(_dsMonHoc);
        OnPropertyChanged(nameof(MonHocView));

        _unitOfWork = unitOfWork;
        _appDbContext = dbContext;
    }

    #region Events.

    private void TabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (TabLopHoc.IsSelected && !_daTaiLopHoc)
        {
            NapDsLopHoc();
            _daTaiLopHoc = true;
        }
        else if (TabMonHoc.IsSelected && !_daTaiMonHoc)
        {
            NapDsMonHoc();
            _daTaiMonHoc = true;
        }
    }

    private void BtnTaiLaiLopHoc_Click(object sender, RoutedEventArgs e)
    {
        NapDsLopHoc();
    }

    private void BtnTaiLaiMonHoc_Click(object sender, RoutedEventArgs e)
    {
        NapDsMonHoc();
    }

    private void BtnThemLopHoc_Click(object sender, RoutedEventArgs e)
    {
        var themLopHocWindow = new ThemLopHocWindow
        {
            Owner = Window.GetWindow(this)
        };

        var result = themLopHocWindow.ShowDialog();
        if (result == true && themLopHocWindow.LopHocMoi != null)
        {
            _unitOfWork.LopHocRepo.Add(themLopHocWindow.LopHocMoi);
            _unitOfWork.Complete();

            NapDsLopHoc();
        }
    }

    private void BtnSuaLopHoc_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button btn && btn.Tag is LopHoc lopHoc)
        {
            var suaWindow = new ThemLopHocWindow(lopHoc)
            {
                Owner = Window.GetWindow(this)
            };

            var result = suaWindow.ShowDialog();
            if (result == true && suaWindow.LopHocMoi != null)
            {
                var lopHocCanSua = _unitOfWork.LopHocRepo.GetById(lopHoc.Id, false);
                if (lopHocCanSua == null)
                {
                    MessageBox.Show("Lớp học không tồn tại hoặc đã bị xóa.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }
                lopHocCanSua.MaLop = suaWindow.LopHocMoi.MaLop;
                lopHocCanSua.NgayBatDau = suaWindow.LopHocMoi.NgayBatDau;
                lopHocCanSua.NgayKetThuc = suaWindow.LopHocMoi.NgayKetThuc;
                lopHocCanSua.TrangThai = suaWindow.LopHocMoi.TrangThai;
                lopHocCanSua.NamHoc = suaWindow.LopHocMoi.NamHoc;
                lopHocCanSua.GVCN = suaWindow.LopHocMoi.GVCN;

                _unitOfWork.LopHocRepo.Update(lopHocCanSua);
                _unitOfWork.Complete();

                NapDsLopHoc();
            }
        }
    }

    private void BtnXoaLopHoc_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button btn && btn.Tag is LopHoc lopHoc)
        {
            var confirm = MessageBox.Show($"Bạn có chắc muốn xóa lớp \"{lopHoc.MaLop}\"?", "Xác nhận xóa", MessageBoxButton.YesNo, MessageBoxImage.Warning);
            
            if (confirm == MessageBoxResult.Yes)
            {
                var lopHocCanXoa = _unitOfWork.LopHocRepo.GetById(lopHoc.Id, false);
                if (lopHocCanXoa == null)
                {
                    MessageBox.Show("Lớp học không tồn tại hoặc đã bị xóa.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                _unitOfWork.LopHocRepo.Delete(lopHocCanXoa);
                _unitOfWork.Complete();

                NapDsLopHoc();
            }
        }
    }

    private void BtnQuanLyMonHoc_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button btn && btn.Tag is LopHoc lopHoc)
        {
            var quanLyMonHocThuocLopWindow = new QuanLyMonHocThuocLopWindow(_appDbContext, lopHoc.Id)
            {
                Owner = Window.GetWindow(this)
            };

            quanLyMonHocThuocLopWindow.ShowDialog();
        }
    }

    private void BtnThemMonHoc_Click(object sender, RoutedEventArgs e)
    {
        var themMonHocWindow = new ThemMonHocWindow
        {
            Owner = Window.GetWindow(this)
        };

        var result = themMonHocWindow.ShowDialog();
        if (result == true && themMonHocWindow.MonHocMoi != null)
        {
            _unitOfWork.MonHocRepo.Add(themMonHocWindow.MonHocMoi);
            _unitOfWork.Complete();

            NapDsMonHoc();
        }
    }

    private void BtnSuaMonHoc_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button btn && btn.Tag is MonHoc monHoc)
        {
            var suaWindow = new ThemMonHocWindow(monHoc)
            {
                Owner = Window.GetWindow(this)
            };

            var result = suaWindow.ShowDialog();
            if (result == true && suaWindow.MonHocMoi != null)
            {
                var monHocCanSua = _unitOfWork.MonHocRepo.GetById(monHoc.Id, false);
                if (monHocCanSua == null)
                {
                    MessageBox.Show("Môn học không tồn tại hoặc đã bị xóa.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }
                monHocCanSua.TenMon = suaWindow.MonHocMoi.TenMon;

                _unitOfWork.MonHocRepo.Update(monHocCanSua);
                _unitOfWork.Complete();

                NapDsMonHoc();
            }
        }
    }

    private void BtnXoaMonHoc_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button btn && btn.Tag is MonHoc monHoc)
        {
            var confirm = MessageBox.Show($"Bạn có chắc muốn xóa môn \"{monHoc.TenMon}\"?", "Xác nhận xóa", MessageBoxButton.YesNo, MessageBoxImage.Warning);

            if (confirm == MessageBoxResult.Yes)
            {
                var monHocCanXoa = _unitOfWork.MonHocRepo.GetById(monHoc.Id, false);
                if (monHocCanXoa == null)
                {
                    MessageBox.Show("Môn học không tồn tại hoặc đã bị xóa.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                _unitOfWork.MonHocRepo.Delete(monHocCanXoa);
                _unitOfWork.Complete();

                NapDsMonHoc();
            }
        }
    }

    private void BtnQuanLyChuong_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button btn && btn.Tag is MonHoc monHoc)
        {
            var quanLyChuongWindow = new QuanLyChuongWindow(_appDbContext, monHoc.Id)
            {
                Owner = Window.GetWindow(this)
            };

            quanLyChuongWindow.ShowDialog();
        }
    }

    #endregion

    #region Methods.

    private void NapDsLopHoc()
    {
        var dsLopHoc = LayDsLopHoc() ?? [];
        _dsLopHoc = new ObservableCollection<LopHoc>(dsLopHoc);
        _lopHocView = CollectionViewSource.GetDefaultView(_dsLopHoc);
        _lopHocView.Filter = FilterLopHoc;
        OnPropertyChanged(nameof(LopHocView));
    }

    private void NapDsMonHoc()
    {
        var dsMonHoc = LayDsMonHoc() ?? [];
        _dsMonHoc = new ObservableCollection<MonHoc>(dsMonHoc);
        _monHocView = CollectionViewSource.GetDefaultView(dsMonHoc);
        _monHocView.Filter = FilterMonHoc;
        OnPropertyChanged(nameof(MonHocView));
    }

    private bool FilterLopHoc(object obj)
    {
        if (obj is not LopHoc lh) return false;
        if (string.IsNullOrWhiteSpace(TuKhoaTimKiemLopHoc)) return true;

        var keyword = TuKhoaTimKiemLopHoc.ToLowerInvariant();

        return (lh.MaLop?.Contains(keyword, StringComparison.OrdinalIgnoreCase) ?? false)
            || (lh.GVCN?.Contains(keyword, StringComparison.OrdinalIgnoreCase) ?? false);
    }

    private bool FilterMonHoc(object obj)
    {
        if (obj is not MonHoc mh) return false;
        if (string.IsNullOrWhiteSpace(TuKhoaTimKiemMonHoc)) return true;

        var keyword = TuKhoaTimKiemMonHoc.ToLowerInvariant();

        return mh.TenMon?.Contains(keyword, StringComparison.OrdinalIgnoreCase) ?? false;
    }

    private List<LopHoc> LayDsLopHoc()
    {
        return _unitOfWork.LopHocRepo.GetAll();
    }

    private List<MonHoc> LayDsMonHoc()
    {
        return _unitOfWork.MonHocRepo.GetAll();
    }

    #endregion

    // Triển khai INotifyPropertyChanged.
    public event PropertyChangedEventHandler? PropertyChanged;
    protected void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
