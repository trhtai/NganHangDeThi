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

    // --- Sửa hàm BtnSuaCauHoi_Click ---
    private void BtnSuaCauHoi_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button btn && btn.Tag is CauHoi cauHoi)
        {
            // 1. Lấy câu hỏi đầy đủ từ DB (Bao gồm cả con và cháu)
            var full = _dbContext.CauHoi
                .Include(x => x.DsCauTraLoi)
                .Include(x => x.DsCauHoiCon)
                    .ThenInclude(child => child.DsCauTraLoi) // Load đáp án của câu con
                .FirstOrDefault(x => x.Id == cauHoi.Id);

            if (full == null) { /* Báo lỗi... */ return; }

            // 2. Tạo file Word tạm
            string folder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "TempDocs");
            Directory.CreateDirectory(folder);
            string filePath = Path.Combine(folder, $"CauHoi-{cauHoi.Id}.docx");

            // 3. Export
            var exporter = new ExportCauHoiToDocxService();
            var imagePath = App.AppHost!.Services.GetRequiredService<IOptions<ImageStorageOptions>>().Value.FolderPath;
            exporter.ExportToDocx(full, filePath, imagePath);

            // 4. Mở file
            Process.Start(new ProcessStartInfo { FileName = filePath, UseShellExecute = true });

            // 5. Hỏi xác nhận cập nhật
            var result = MessageBox.Show("...", "Cập nhật lại câu hỏi", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    // 6. Đọc lại file
                    var updatedList = _questionExtractorService.ExtractQuestionsFromDocx(filePath);
                    var updated = updatedList.FirstOrDefault();

                    if (updated != null)
                    {
                        // 7. Gọi hàm cập nhật thông minh
                        CapNhatCauHoi(full.Id, updated);
                        _questionExtractorService.CommitImages();

                        MessageBox.Show("Cập nhật thành công!", "Thông báo");
                        NapDsCauHoi();
                    }
                    else
                    {
                        MessageBox.Show("File rỗng hoặc lỗi định dạng.", "Lỗi");
                    }
                }
                catch (Exception ex)
                {
                    _questionExtractorService.CleanupTemporaryImages();
                    MessageBox.Show($"Lỗi: {ex.Message}", "Lỗi");
                }
            }
        }
    }

    // --- Viết lại hàm CapNhatCauHoi để xử lý đệ quy ---
    private void CapNhatCauHoi(int cauHoiId, CauHoiRaw qNew)
    {
        // Load lại DB object (để đảm bảo tracking của EF)
        var dbEntity = _dbContext.CauHoi
            .Include(x => x.DsCauTraLoi)
            .Include(x => x.DsCauHoiCon)
                .ThenInclude(x => x.DsCauTraLoi)
            .First(x => x.Id == cauHoiId);

        var imageBasePath = App.AppHost!.Services.GetRequiredService<IOptions<ImageStorageOptions>>().Value.FolderPath;

        // 1. Cập nhật thông tin bản thân (Cha hoặc Đơn)
        UpdateSingleQuestionData(dbEntity, qNew, imageBasePath);

        // 2. Cập nhật danh sách câu con (nếu có)
        // Chiến thuật: Đồng bộ theo Index (Con 1 update Con 1, Con 2 update Con 2...)
        // Nếu file có nhiều con hơn -> Thêm mới.
        // Nếu file có ít con hơn -> Xóa bớt (Cần check ràng buộc nếu câu con đã ra đề).

        var dbChildren = dbEntity.DsCauHoiCon.OrderBy(x => x.Id).ToList();
        var newChildren = qNew.CauHoiCon;

        int maxCount = Math.Max(dbChildren.Count, newChildren.Count);

        for (int i = 0; i < maxCount; i++)
        {
            if (i < dbChildren.Count && i < newChildren.Count)
            {
                // Trường hợp 1: Cả 2 đều có -> UPDATE
                UpdateSingleQuestionData(dbChildren[i], newChildren[i], imageBasePath);
            }
            else if (i >= dbChildren.Count && i < newChildren.Count)
            {
                // Trường hợp 2: File có, DB chưa có -> ADD NEW CHILD
                var newChildRaw = newChildren[i];
                var newChildEntity = new CauHoi
                {
                    ParentId = dbEntity.Id, // Gắn vào cha
                    ChuongId = dbEntity.ChuongId // Kế thừa chương của cha
                };
                UpdateSingleQuestionData(newChildEntity, newChildRaw, imageBasePath);
                _dbContext.CauHoi.Add(newChildEntity);
            }
            else if (i < dbChildren.Count && i >= newChildren.Count)
            {
                // Trường hợp 3: DB có, File không có -> DELETE CHILD
                // (Lưu ý: Nếu câu con đã được dùng trong đề thi, lệnh này sẽ lỗi do khóa ngoại.
                // Tạm thời ta cho phép xóa, nếu lỗi EF sẽ throw exception ra ngoài để catch)
                var childToDelete = dbChildren[i];

                // Xóa ảnh của con trước
                TryDeleteImage(childToDelete.HinhAnh, imageBasePath);
                foreach (var ans in childToDelete.DsCauTraLoi) TryDeleteImage(ans.HinhAnh, imageBasePath);

                _dbContext.CauHoi.Remove(childToDelete);
            }
        }

        _dbContext.SaveChanges();
    }

    // Hàm helper để cập nhật dữ liệu 1 câu hỏi (không đệ quy)
    private void UpdateSingleQuestionData(CauHoi dbEntity, CauHoiRaw rawData, string imageBasePath)
    {
        // Xóa ảnh cũ nếu có thay đổi ảnh
        if (dbEntity.HinhAnh != rawData.HinhAnh)
            TryDeleteImage(dbEntity.HinhAnh, imageBasePath);

        dbEntity.NoiDung = rawData.NoiDung;
        dbEntity.MucDo = rawData.MucDo;
        dbEntity.Loai = rawData.Loai;
        dbEntity.HinhAnh = rawData.HinhAnh;

        // Cập nhật đáp án (Xóa hết cũ, thêm mới cho đơn giản và sạch sẽ)
        // Lưu ý: Xóa đáp án không ảnh hưởng nhiều ràng buộc bằng xóa câu hỏi
        foreach (var oldAns in dbEntity.DsCauTraLoi)
        {
            TryDeleteImage(oldAns.HinhAnh, imageBasePath);
        }
        dbEntity.DsCauTraLoi.Clear();

        foreach (var ansRaw in rawData.DapAn)
        {
            dbEntity.DsCauTraLoi.Add(new CauTraLoi
            {
                NoiDung = ansRaw.NoiDung,
                LaDapAnDung = ansRaw.LaDapAnDung,
                ViTriGoc = ansRaw.ViTriGoc,
                DaoViTri = ansRaw.DaoViTri,
                HinhAnh = ansRaw.HinhAnh
            });
        }
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
