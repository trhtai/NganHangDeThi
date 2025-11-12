using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using NganHangDeThi.Common.Configs;
using NganHangDeThi.Common.Enum;
using NganHangDeThi.Data.DataContext;
using NganHangDeThi.Data.Entity;
using NganHangDeThi.Models;
using NganHangDeThi.Services;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace NganHangDeThi.MyUserControl;

public partial class NganHangCauHoiControl : UserControl, INotifyPropertyChanged
{
    private readonly AppDbContext _dbContext;
    private readonly QuestionExtractorService _questionExtractorService;

    private ObservableCollection<CauHoi> _dsCauHoi = [];
    public ObservableCollection<MonHoc> DsMonHoc { get; set; } = [];
    public ObservableCollection<Chuong> DsChuong { get; set; } = [];

    public int TongTatCa => _dsCauHoi.Count;
    public int TongDangHienThi => CauHoiView?.Cast<object>().Count() ?? 0;

    private ICollectionView _cauHoiView;
    public ICollectionView CauHoiView => _cauHoiView;

    private string _tuKhoaTimKiemCauHoi = string.Empty;
    public string TuKhoaTimKiemCauHoi
    {
        get => _tuKhoaTimKiemCauHoi;
        set
        {
            if (_tuKhoaTimKiemCauHoi != value)
            {
                _tuKhoaTimKiemCauHoi = value;
                OnPropertyChanged(nameof(TuKhoaTimKiemCauHoi));
                CauHoiView.Refresh();
                OnPropertyChanged(nameof(TongDangHienThi));
            }
        }
    }

    public NganHangCauHoiControl(
        AppDbContext dbContext,
        QuestionExtractorService questionExtractorService)
    {
        InitializeComponent();
        DataContext = this;

        _cauHoiView = CollectionViewSource.GetDefaultView(_dsCauHoi);
        OnPropertyChanged(nameof(CauHoiView));

        _dbContext = dbContext;
        _questionExtractorService = questionExtractorService;

        NapDsCauHoi();
    }

    private void BtnTaiLaiCauHoi_Click(object sender, RoutedEventArgs e)
    {
        NapDsCauHoi();
    }

    private void BtnThemCauHoiTuFile_Click(object sender, RoutedEventArgs e)
    {
        var w = App.AppHost!.Services.GetRequiredService<ThemCauHoiTuFileWindow>();
        w.Owner = Window.GetWindow(this);

        var result = w.ShowDialog();

        if (result == true)
        {
            NapDsCauHoi();
        }
    }

    private void BtnSuaCauHoi_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button btn && btn.Tag is CauHoi cauHoi)
        {
            // Lấy câu hỏi đầy đủ từ DB
            var full = _dbContext.CauHoi
                .Include(x => x.DsCauTraLoi)
                .FirstOrDefault(x => x.Id == cauHoi.Id);

            if (full == null)
            {
                MessageBox.Show("Không tìm thấy câu hỏi.", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            // Tạo thư mục tạm và file Word
            string folder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "TempDocs");
            Directory.CreateDirectory(folder);
            string filePath = Path.Combine(folder, $"CauHoi-{cauHoi.Id}.docx");

            // Export ra file
            var exporter = new ExportCauHoiToDocxService();
            var imagePath = App.AppHost!.Services.GetRequiredService<IOptions<ImageStorageOptions>>().Value.FolderPath;
            exporter.ExportToDocx(full, filePath, imagePath);

            // Mở file
            Process.Start(new ProcessStartInfo
            {
                FileName = filePath,
                UseShellExecute = true
            });

            // Hỏi người dùng có muốn cập nhật không
            var result = MessageBox.Show(
                "Sau khi chỉnh sửa xong file, bạn có muốn cập nhật lại câu hỏi trong hệ thống?",
                "Cập nhật lại câu hỏi",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    var updated = _questionExtractorService.ExtractQuestionsFromDocx(filePath).FirstOrDefault();
                    if (updated != null)
                    {
                        CapNhatCauHoi(full.Id, updated);
                        _questionExtractorService.CommitImages();

                        MessageBox.Show("Cập nhật thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                        NapDsCauHoi();
                    }
                    else
                    {
                        MessageBox.Show("Không tìm thấy nội dung hợp lệ trong file.", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Warning);
                    }
                }
                catch (Exception ex)
                {
                    _questionExtractorService.CleanupTemporaryImages();
                    MessageBox.Show($"Lỗi khi cập nhật: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
    }

    private void CapNhatCauHoi(int cauHoiId, CauHoiRaw q)
    {
        var cauHoiDb = _dbContext.CauHoi
            .Include(x => x.DsCauTraLoi)
            .First(x => x.Id == cauHoiId);

        var imagePath = App.AppHost!.Services.GetRequiredService<IOptions<ImageStorageOptions>>().Value.FolderPath;

        // Xóa ảnh cũ
        TryDeleteImage(cauHoiDb.HinhAnh, imagePath);
        foreach (var da in cauHoiDb.DsCauTraLoi)
        {
            TryDeleteImage(da.HinhAnh, imagePath);
        }

        // Cập nhật câu hỏi
        cauHoiDb.NoiDung = q.NoiDung;
        cauHoiDb.MucDo = q.MucDo;
        cauHoiDb.Loai = q.Loai;
        cauHoiDb.HinhAnh = q.HinhAnh;
        cauHoiDb.DsCauTraLoi.Clear();

        foreach (var d in q.DapAn)
        {
            cauHoiDb.DsCauTraLoi.Add(new CauTraLoi
            {
                NoiDung = d.NoiDung,
                LaDapAnDung = d.LaDapAnDung,
                ViTriGoc = d.ViTriGoc,
                DaoViTri = d.DaoViTri,
                HinhAnh = d.HinhAnh
            });
        }

        _dbContext.SaveChanges();
    }

    private void TryDeleteImage(string? relativePath, string imageBaseFolder)
    {
        if (string.IsNullOrWhiteSpace(relativePath)) return;

        try
        {
            var fullPath = Path.Combine(imageBaseFolder, relativePath);
            if (File.Exists(fullPath))
                File.Delete(fullPath);
        }
        catch
        {
            // Ghi log nếu cần
        }
    }


    private void BtnXoaCauHoi_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button btn && btn.Tag is CauHoi cauHoi)
        {
            var confirm = MessageBox.Show($"Bạn có chắc muốn xóa câu hỏi \"{cauHoi.NoiDung}\"?", "Xác nhận xóa", MessageBoxButton.YesNo, MessageBoxImage.Warning);

            if (confirm == MessageBoxResult.Yes)
            {
                var cauHoiCanXoa = _dbContext.CauHoi.FirstOrDefault(x => x.Id ==  cauHoi.Id);
                if (cauHoiCanXoa == null)
                {
                    MessageBox.Show("Câu hỏi không tồn tại hoặc đã bị xóa.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }
                _dbContext.CauHoi.Remove(cauHoiCanXoa);
                _dbContext.SaveChanges();
                NapDsCauHoi();
            }
        }
    }

    private void BtnXoaBoLoc_Click(object sender, RoutedEventArgs e)
    {
        TuKhoaTimKiemCauHoi = string.Empty;
        MucDoLoc = null;
        LoaiLoc = null;
        MonHocLoc = null;
        ChuongLoc = null;
    }

    // Methods.
    private void NapDsCauHoi()
    {
        var dsCauHoi = LayDsCauHoi() ?? [];
        _dsCauHoi = new ObservableCollection<CauHoi>(dsCauHoi);
        _cauHoiView = CollectionViewSource.GetDefaultView(_dsCauHoi);
        _cauHoiView.Filter = FilterCauHoi;
        OnPropertyChanged(nameof(CauHoiView));
        OnPropertyChanged(nameof(TongDangHienThi)); 
        OnPropertyChanged(nameof(TongTatCa));

        DsMonHoc = new ObservableCollection<MonHoc>(_dbContext.MonHoc.OrderBy(m => m.TenMon));
        OnPropertyChanged(nameof(DsMonHoc));
    }

    private MucDoCauHoi? _mucDoLoc = null;
    public MucDoCauHoi? MucDoLoc
    {
        get => _mucDoLoc;
        set
        {
            if (_mucDoLoc != value)
            {
                _mucDoLoc = value;
                OnPropertyChanged(nameof(MucDoLoc));
                CauHoiView.Refresh();
                OnPropertyChanged(nameof(TongDangHienThi));
            }
        }
    }

    private LoaiCauHoi? _loaiLoc = null;
    public LoaiCauHoi? LoaiLoc
    {
        get => _loaiLoc;
        set
        {
            if (_loaiLoc != value)
            {
                _loaiLoc = value;
                OnPropertyChanged(nameof(LoaiLoc));
                CauHoiView.Refresh();
                OnPropertyChanged(nameof(TongDangHienThi));
            }
        }
    }

    private MonHoc? _monHocLoc;
    public MonHoc? MonHocLoc
    {
        get => _monHocLoc;
        set
        {
            if (_monHocLoc != value)
            {
                _monHocLoc = value;
                OnPropertyChanged(nameof(MonHocLoc));
                CapNhatDsChuongTheoMon();
                CauHoiView.Refresh();
                OnPropertyChanged(nameof(TongDangHienThi));
            }
        }
    }

    private Chuong? _chuongLoc;
    public Chuong? ChuongLoc
    {
        get => _chuongLoc;
        set
        {
            if (_chuongLoc != value)
            {
                _chuongLoc = value;
                OnPropertyChanged(nameof(ChuongLoc));
                CauHoiView.Refresh();
                OnPropertyChanged(nameof(TongDangHienThi));
            }
        }
    }

    private bool FilterCauHoi(object obj)
    {
        if (obj is not CauHoi ch) return false;

        if (!string.IsNullOrWhiteSpace(TuKhoaTimKiemCauHoi))
        {
            var keyword = TuKhoaTimKiemCauHoi.ToLowerInvariant();
            if (!(ch.NoiDung?.Contains(keyword, StringComparison.OrdinalIgnoreCase) ?? false))
                return false;
        }

        if (MucDoLoc.HasValue && ch.MucDo != MucDoLoc.Value)
            return false;

        if (LoaiLoc.HasValue && ch.Loai != LoaiLoc.Value)
            return false;

        if (MonHocLoc != null && (ch.Chuong == null || ch.Chuong.MonHocId != MonHocLoc.Id))
            return false;

        if (ChuongLoc != null && ch.ChuongId != ChuongLoc.Id)
            return false;

        return true;
    }

    private void CapNhatDsChuongTheoMon()
    {
        if (MonHocLoc != null)
        {
            var dsChuong = _dbContext.Chuong
                .Where(c => c.MonHocId == MonHocLoc.Id)
                .OrderBy(c => c.ViTri)
                .ToList();

            DsChuong = new ObservableCollection<Chuong>(dsChuong);
        }
        else
        {
            DsChuong = new ObservableCollection<Chuong>();
        }

        ChuongLoc = null;
        OnPropertyChanged(nameof(DsChuong));
    }

    private List<CauHoi> LayDsCauHoi()
    {
        return [.. _dbContext.CauHoi.Include(x => x.Chuong)];
    }

    // Triển khai INotifyPropertyChanged.
    public event PropertyChangedEventHandler? PropertyChanged;
    protected void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
