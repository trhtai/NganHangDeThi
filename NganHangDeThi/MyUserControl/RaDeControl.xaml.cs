using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using NganHangDeThi.Common.Configs;
using NganHangDeThi.Data.DataContext;
using NganHangDeThi.Data.Entity;
using NganHangDeThi.Models;
using NganHangDeThi.Services;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace NganHangDeThi.MyUserControl;

public partial class RaDeControl : UserControl, INotifyPropertyChanged
{
    private readonly AppDbContext _dbContext; 
    private ObservableCollection<DeThi> _dsDeThi = [];
    private readonly string _baseImageFolder;
    public ObservableCollection<MaTran> MaTranView { get; set; } = [];

    private bool _daTaiMaTran = false;
    private bool _daTaiDeThi = false;

    private ICollectionView _deThiView;
    public ICollectionView DeThiView => _deThiView;

    private string _tuKhoaTimKiemDeThi = string.Empty;
    public string TuKhoaTimKiemDeThi
    {
        get => _tuKhoaTimKiemDeThi;
        set
        {
            if (_tuKhoaTimKiemDeThi != value)
            {
                _tuKhoaTimKiemDeThi = value;
                OnPropertyChanged(nameof(TuKhoaTimKiemDeThi));
                DeThiView.Refresh();
            }
        }
    }
    public RaDeControl(AppDbContext dbContext, IOptions<ImageStorageOptions> options)
    {
        InitializeComponent();

        _dbContext = dbContext;
        DataContext = this;
        _baseImageFolder = options.Value.FolderPath;

        _deThiView = CollectionViewSource.GetDefaultView(_dsDeThi);
        OnPropertyChanged(nameof(DeThiView));

        LoadDeThi();
        LoadMaTran();
    }

    private void LoadMaTran()
    {
        MaTranView.Clear();
        var list = _dbContext.MaTran.ToList();
        foreach (var item in list)
        {
            MaTranView.Add(item);
        }
    }

    private void LoadDeThi()
    {
        var dsDeThi = LayDsDeThi() ?? [];
        _dsDeThi = new ObservableCollection<DeThi>(dsDeThi);
        _deThiView = CollectionViewSource.GetDefaultView(_dsDeThi);
        _deThiView.Filter = FilterDeThi;
        OnPropertyChanged(nameof(DeThiView));
    }

    private bool FilterDeThi(object obj)
    {
        if (obj is not DeThi lh) return false;
        if (string.IsNullOrWhiteSpace(TuKhoaTimKiemDeThi)) return true;

        var keyword = TuKhoaTimKiemDeThi.ToLowerInvariant();

        return (lh.TieuDe?.Contains(keyword, StringComparison.OrdinalIgnoreCase) ?? false);
    }

    private List<DeThi> LayDsDeThi()
    {
        return [.. _dbContext.DeThi.Include(x => x.MonHoc).Include(x => x.LopHoc)];
    }

    private void BtnTaiLaiMaTran_Click(object sender, System.Windows.RoutedEventArgs e)
    {
        LoadMaTran();
    }

    private void BtnThemMaTran_Click(object sender, System.Windows.RoutedEventArgs e)
    {
        var window = new ThemHoacSuaMaTranWindow(_dbContext);
        if (window.ShowDialog() == true)
        {
            var newMaTran = window.MaTran;
            _dbContext.MaTran.Add(newMaTran);
            _dbContext.SaveChanges();
            LoadMaTran();
        }
    }

    private void BtnSuaMaTran_Click(object sender, System.Windows.RoutedEventArgs e)
    {
        if ((sender as Button)?.Tag is MaTran selected)
        {
            var window = new ThemHoacSuaMaTranWindow(_dbContext, selected);
            if (window.ShowDialog() == true)
            {
                var edited = window.MaTran;

                selected.Name = edited.Name;
                selected.ThoiGianCapNhatGanNhat = DateTime.Now;

                // Xóa các chi tiết cũ
                var oldDetails = _dbContext.ChiTietMaTran.Where(c => c.MaTranId == selected.Id);
                _dbContext.ChiTietMaTran.RemoveRange(oldDetails);

                // Thêm mới chi tiết
                foreach (var ct in window.DsChiTietMaTran)
                {
                    ct.MaTranId = selected.Id;
                    _dbContext.ChiTietMaTran.Add(ct);
                }

                _dbContext.MaTran.Update(selected);
                _dbContext.SaveChanges();
                LoadMaTran();
            }
        }
    }

    private void BtnXoaMaTran_Click(object sender, System.Windows.RoutedEventArgs e)
    {
        var button = sender as Button;
        if (button?.Tag is MaTran selected)
        {
            var result = MessageBox.Show("Bạn có chắc chắn muốn xóa ma trận này không?", "Xác nhận", MessageBoxButton.YesNo, MessageBoxImage.Warning);
            if (result == MessageBoxResult.Yes)
            {
                _dbContext.MaTran.Remove(selected);
                _dbContext.SaveChanges();
                LoadMaTran();
            }
        }
    }

    private void TabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (TabMaTran.IsSelected && !_daTaiMaTran)
        {
            LoadMaTran();
            _daTaiMaTran = true;
        }
        else if (TabDeThi.IsSelected && !_daTaiDeThi)
        {
            LoadDeThi();
            _daTaiDeThi = true;
        }
    }

    private void BtnXoaDeThi_Click(object sender, RoutedEventArgs e)
    {
        if ((sender as Button)?.Tag is not DeThi selectedDeThi) return;

        var result = MessageBox.Show(
            $"Bạn có chắc chắn muốn xóa đề thi \"{selectedDeThi.TieuDe}\"?",
            "Xác nhận xóa",
            MessageBoxButton.YesNo,
            MessageBoxImage.Warning
        );

        if (result != MessageBoxResult.Yes) return;

        try
        {
            // Xóa chi tiết đề thi
            var chiTietDeThi = _dbContext.ChiTietDeThi
                .Where(ct => ct.DeThiId == selectedDeThi.Id)
                .ToList();
            _dbContext.ChiTietDeThi.RemoveRange(chiTietDeThi);

            // Xóa đề thi
            _dbContext.DeThi.Remove(selectedDeThi);
            _dbContext.SaveChanges();

            // Cập nhật UI
            _dsDeThi.Remove(selectedDeThi);
            _deThiView.Refresh();
        }
        catch (Exception ex)
        {
            MessageBox.Show("Xảy ra lỗi khi xóa đề thi: " + ex.Message, "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void BtnTaoDeThi_Click(object sender, RoutedEventArgs e)
    {
        var window = new ThemDeThiWindow(_dbContext);
        // Window này giờ đã tự xử lý việc lưu Database bên trong nó
        if (window.ShowDialog() == true)
        {
            // Chỉ cần load lại danh sách để hiện đề thi mới lên lưới
            LoadDeThi();
            MessageBox.Show("Tạo đề thi thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }

    private void BtnXuatDeThi_Click(object sender, RoutedEventArgs e)
    {
        if ((sender as Button)?.Tag is not DeThi selectedDeThi) return;

        // 1. Load dữ liệu đầy đủ (Bao gồm cả câu hỏi cha - Parent)
        var deThiDayDu = _dbContext.DeThi
            .Include(x => x.MonHoc)
            .Include(x => x.LopHoc)
            .Include(d => d.DsChiTietDeThi)
                .ThenInclude(ct => ct.CauHoi)
                    .ThenInclude(ch => ch.Parent) // <--- QUAN TRỌNG: Lấy câu chùm/đoạn văn
            .Include(d => d.DsChiTietDeThi)
                .ThenInclude(ct => ct.DsDapAnTrongDe)
            .FirstOrDefault(d => d.Id == selectedDeThi.Id);

        if (deThiDayDu == null)
        {
            MessageBox.Show("Không tìm thấy dữ liệu đề thi.", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            return;
        }

        // 2. Chuẩn bị dữ liệu Export
        var exportData = new DeThiExportData { DeThi = deThiDayDu };

        foreach (var chiTiet in deThiDayDu.DsChiTietDeThi.OrderBy(ct => ct.Id))
        {
            var ch = chiTiet.CauHoi;
            // Map ngược lại từ ChiTietCauTraLoiTrongDeThi sang CauTraLoi để dùng chung Service
            var dapAnDaTron = chiTiet.DsDapAnTrongDe
                .OrderBy(d => d.ViTri)
                .Select(d => new CauTraLoi
                {
                    NoiDung = d.NoiDung,
                    LaDapAnDung = d.LaDapAnDung,
                    ViTriGoc = d.ViTri,
                    HinhAnh = d.HinhAnh,
                    DaoViTri = false
                }).ToList();

            exportData.CauHoiVaDapAn.Add((ch, dapAnDaTron));
        }

        // 3. Thực hiện lưu file
        var saveFileDialog = new Microsoft.Win32.SaveFileDialog
        {
            Title = $"Lưu đề thi: {exportData.DeThi.TieuDe}_{exportData.DeThi.MaDe}",
            FileName = $"{exportData.DeThi.TieuDe}_{exportData.DeThi.MaDe}.docx",
            Filter = "Word Document (*.docx)|*.docx"
        };

        if (saveFileDialog.ShowDialog() == true)
        {
            try
            {
                // Gọi Service Export MỚI (đã cập nhật dùng HtmlToWordHelper)
                ExportDeThiToWordService.Export(exportData, saveFileDialog.FileName, _baseImageFolder);

                var dapAnPath = System.IO.Path.Combine(
                    System.IO.Path.GetDirectoryName(saveFileDialog.FileName)!,
                    System.IO.Path.GetFileNameWithoutExtension(saveFileDialog.FileName) + "_DapAn.docx");

                ExportDapAnService.Export(deThiDayDu, dapAnPath, _baseImageFolder);

                MessageBox.Show("Xuất đề thi và đáp án thành công!", "Thành công", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Xuất đề thi thất bại: " + ex.Message, "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }


    // Triển khai INotifyPropertyChanged.
    public event PropertyChangedEventHandler? PropertyChanged;
    protected void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    private void BtnTaiLaiDeThi_Click(object sender, RoutedEventArgs e)
    {
        LoadDeThi();
    }
}
