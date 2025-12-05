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
using System.Windows.Input;

namespace NganHangDeThi.MyUserControl;

public partial class RaDeControl : UserControl, INotifyPropertyChanged
{
    private readonly AppDbContext _dbContext;
    private ObservableCollection<DeThi> _dsDeThi = [];
    private readonly string _baseImageFolder;
    public ObservableCollection<MaTran> MaTranView { get; set; } = [];
    public ObservableCollection<LopHoc> DsLopHoc { get; set; } = [];

    private LopHoc? _lopHocLoc;
    public LopHoc? LopHocLoc
    {
        get => _lopHocLoc;
        set
        {
            if (_lopHocLoc != value)
            {
                _lopHocLoc = value;
                OnPropertyChanged(nameof(LopHocLoc));
                DeThiView.Refresh(); // Kích hoạt lại bộ lọc khi chọn lớp khác
            }
        }
    }

    private void LoadDsLopHoc()
    {
        DsLopHoc.Clear();
        var listLop = _dbContext.LopHoc.OrderBy(x => x.MaLop).ToList();
        foreach (var lop in listLop)
        {
            DsLopHoc.Add(lop);
        }
    }

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

        _deThiView.GroupDescriptions.Add(new PropertyGroupDescription("CreatedAt"));
        _deThiView.Filter = FilterDeThi;
        OnPropertyChanged(nameof(DeThiView));

        LoadDeThi();
        LoadMaTran();
        LoadDsLopHoc();
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

        // Thay vì tạo mới ObservableCollection, hãy giữ nguyên instance cũ nếu có thể, 
        // hoặc cấu hình lại View sau khi tạo mới.
        // Cách an toàn nhất với code hiện tại của bạn là cấu hình lại View ngay tại đây:

        _dsDeThi = new ObservableCollection<DeThi>(dsDeThi);
        _deThiView = CollectionViewSource.GetDefaultView(_dsDeThi);

        // --- QUAN TRỌNG: THIẾT LẬP LẠI GOM NHÓM VÀ BỘ LỌC ---

        // 1. Thêm Gom nhóm theo Ngày tạo
        _deThiView.GroupDescriptions.Clear(); // Xóa cũ nếu có
        //_deThiView.GroupDescriptions.Add(new PropertyGroupDescription("CreatedAt"));
        _deThiView.GroupDescriptions.Add(new PropertyGroupDescription("BatchId"));

        // 2. Thiết lập Bộ lọc
        _deThiView.Filter = FilterDeThi;

        // 3. Thông báo cập nhật UI
        OnPropertyChanged(nameof(DeThiView));
    }

    //private bool FilterDeThi(object obj)
    //{
    //    if (obj is not DeThi lh) return false;
    //    if (string.IsNullOrWhiteSpace(TuKhoaTimKiemDeThi)) return true;

    //    var keyword = TuKhoaTimKiemDeThi.ToLowerInvariant();

    //    return (lh.TieuDe?.Contains(keyword, StringComparison.OrdinalIgnoreCase) ?? false);
    //}
    private bool FilterDeThi(object obj)
    {
        if (obj is not DeThi dt) return false;

        // 1. Kiểm tra Lớp học (Giữ nguyên)
        if (LopHocLoc != null && dt.LopHocId != LopHocLoc.Id)
            return false;

        // 2. Tìm kiếm thông minh (Mới)
        if (!string.IsNullOrWhiteSpace(TuKhoaTimKiemDeThi))
        {
            var k = TuKhoaTimKiemDeThi.Trim().ToLowerInvariant();

            // Kiểm tra Tiêu đề
            bool matchTieuDe = dt.TieuDe?.ToLowerInvariant().Contains(k) ?? false;

            // Kiểm tra Mã đề (User gõ 101, 102...)
            bool matchMaDe = dt.MaDe.ToString().Contains(k);

            // Kiểm tra Kỳ thi (User gõ "Cuối kỳ", "Giữa kỳ")
            bool matchKyThi = dt.KyThi?.ToLowerInvariant().Contains(k) ?? false;

            // Kiểm tra Môn học (User gõ "Toán", "Văn") - Cần Include MonHoc khi load
            bool matchMon = dt.MonHoc?.TenMon?.ToLowerInvariant().Contains(k) ?? false;

            // Kết hợp: Chỉ cần khớp 1 trong các tiêu chí trên là hiện
            return matchTieuDe || matchMaDe || matchKyThi || matchMon;
        }

        return true;
    }

    private void BtnXoaBoLoc_Click(object sender, RoutedEventArgs e)
    {
        TuKhoaTimKiemDeThi = string.Empty;
        LopHocLoc = null;
    }

    private List<DeThi> LayDsDeThi()
    {
        return [.. _dbContext.DeThi
                .Include(x => x.MonHoc)
                .Include(x => x.LopHoc)
                .OrderByDescending(x => x.CreatedAt)];
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

    // File: NganHangDeThi/MyUserControl/RaDeControl.xaml.cs

    private void BtnXoaMaTran_Click(object sender, System.Windows.RoutedEventArgs e)
    {
        var button = sender as Button;
        if (button?.Tag is MaTran selected)
        {
            // 1. Kiểm tra logic nghiệp vụ: Có đề thi nào ĐÃ THI chưa?
            bool coDeThiDaThi = _dbContext.DeThi.Any(d => d.MaTranId == selected.Id && d.DaThi);

            if (coDeThiDaThi)
            {
                // Nếu đã có đề thi thật -> TUYỆT ĐỐI KHÔNG XÓA
                MessageBox.Show(
                    $"Không thể xóa ma trận \"{selected.Name}\".\n\n" +
                    "Lý do: Ma trận này đã được sử dụng cho các kỳ thi chính thức (có đề thi đã được tổ chức).\n" +
                    "Dữ liệu cần được lưu trữ để tra cứu lịch sử.",
                    "Không thể xóa",
                    MessageBoxButton.OK,
                    MessageBoxImage.Stop);
                return;
            }

            // 2. Nếu chưa có đề thi nào (hoặc chỉ toàn đề nháp chưa thi) -> Cho phép xóa sạch
            // Tìm các đề thi "nháp" đang liên kết để xóa dọn đường
            var dsDeThiNhap = _dbContext.DeThi.Where(d => d.MaTranId == selected.Id).ToList();

            string thongBao = $"Bạn có chắc chắn muốn xóa ma trận \"{selected.Name}\" không?";
            if (dsDeThiNhap.Count > 0)
            {
                thongBao += $"\n\n⚠️ CHÚ Ý: Hệ thống sẽ xóa kèm {dsDeThiNhap.Count} đề thi NHÁP (chưa thi) được tạo từ ma trận này.";
            }

            var result = MessageBox.Show(thongBao, "Xác nhận xóa", MessageBoxButton.YesNo, MessageBoxImage.Warning);

            if (result == MessageBoxResult.Yes)
            {
                using var transaction = _dbContext.Database.BeginTransaction();
                try
                {
                    // Xóa các đề thi nháp trước (để gỡ khóa ngoại)
                    if (dsDeThiNhap.Any())
                    {
                        _dbContext.DeThi.RemoveRange(dsDeThiNhap);
                    }

                    // Sau đó xóa Ma trận
                    _dbContext.MaTran.Remove(selected);

                    _dbContext.SaveChanges();
                    transaction.Commit();

                    LoadMaTran(); // Tải lại danh sách
                    MessageBox.Show("Đã xóa thành công.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    MessageBox.Show("Lỗi khi xóa: " + ex.Message, "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                }
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

                // --- BỔ SUNG MỚI: Đánh dấu đề này ĐÃ THI ---
                if (!selectedDeThi.DaThi)
                {
                    selectedDeThi.DaThi = true; // Đánh dấu
                    _dbContext.DeThi.Update(selectedDeThi);
                    _dbContext.SaveChanges();
                }

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

    // Thêm sự kiện Click
    private void BtnXuatBoDe_Click(object sender, RoutedEventArgs e)
    {
        if ((sender as Button)?.Tag is not DeThi deThiMau) return;

        // 1. Lấy BatchId
        var batchId = deThiMau.BatchId;

        // 2. Load dữ liệu
        var danhSachDeThi = _dbContext.DeThi
            .Where(x => x.BatchId == batchId)
            .Include(x => x.MonHoc)
            .Include(x => x.LopHoc)
            .Include(d => d.DsChiTietDeThi)
                .ThenInclude(ct => ct.CauHoi)
                    .ThenInclude(ch => ch.Parent)
            .Include(d => d.DsChiTietDeThi)
                .ThenInclude(ct => ct.DsDapAnTrongDe)
            .ToList();

        if (!danhSachDeThi.Any())
        {
            MessageBox.Show("Không tìm thấy dữ liệu đề thi.", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            return;
        }

        // 3. Chọn thư mục
        var dialog = new Microsoft.Win32.OpenFolderDialog
        {
            Title = "Chọn thư mục để lưu bộ đề thi",
        };

        if (dialog.ShowDialog() == true)
        {
            string folderPath = dialog.FolderName;
            int countSuccess = 0;

            try
            {
                Mouse.OverrideCursor = Cursors.Wait;

                // --- VÒNG LẶP: CHỈ XUẤT ĐỀ THI ---
                foreach (var deThi in danhSachDeThi)
                {
                    // Tạo tên file an toàn cho ĐỀ THI
                    string safeTitle = string.Join("_", deThi.TieuDe.Split(Path.GetInvalidFileNameChars()));
                    string fileNameDe = $"De_{deThi.MaDe}_{safeTitle}.docx";
                    string pathDe = Path.Combine(folderPath, fileNameDe);

                    // Chuẩn bị dữ liệu Export Đề
                    var exportData = new DeThiExportData { DeThi = deThi };
                    foreach (var chiTiet in deThi.DsChiTietDeThi.OrderBy(ct => ct.Id))
                    {
                        var ch = chiTiet.CauHoi;
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

                    // Xuất file Đề thi
                    ExportDeThiToWordService.Export(exportData, pathDe, _baseImageFolder);

                    // (ĐÃ XÓA): Dòng ExportDapAnService.Export(...) ở đây để không xuất lẻ nữa

                    // Đánh dấu đã thi
                    if (!deThi.DaThi) deThi.DaThi = true;

                    countSuccess++;
                }

                // --- MỚI: XUẤT ĐÁP ÁN TỔNG HỢP (1 LẦN DUY NHẤT) ---
                if (countSuccess > 0)
                {
                    // Tạo tên file đáp án tổng hợp (Ví dụ: DapAn_TongHop_20231025.docx)
                    string timeStamp = danhSachDeThi.First().CreatedAt.ToString("yyyyMMdd_HHmmss");
                    string fileNameDapAnTong = $"DapAn_TongHop_{timeStamp}.docx";
                    string pathDapAn = Path.Combine(folderPath, fileNameDapAnTong);

                    // Gọi hàm ExportBatch
                    ExportDapAnService.ExportBatch(danhSachDeThi, pathDapAn, _baseImageFolder);
                }

                _dbContext.SaveChanges();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Có lỗi xảy ra: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                Mouse.OverrideCursor = null;
            }

            if (countSuccess > 0)
            {
                MessageBox.Show($"Đã xuất thành công {countSuccess} đề thi và 1 file đáp án tổng hợp vào thư mục:\n{folderPath}",
                                "Thành công", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
    }
}
