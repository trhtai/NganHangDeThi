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
        if (window.ShowDialog() != true) return;

        var soLuong = window.SoLuongDe;
        var maDeBatDau = window.MaDeBatDau;
        var maTran = window.SelectedMaTran;
        var monHoc = window.SelectedMonHoc;
        var lopHoc = window.SelectedLopHoc;
        var tieuDe = window.TieuDe;
        var choPhepTronDapAn = window.ChoPhepTronDapAn;
        var thoiGianLamBai = window.ThoiGianLamBai;
        var ghiChu = window.GhiChu;
        var kyThi = window.KyThi;

        var chiTietMaTrans = _dbContext.ChiTietMaTran
            .Include(ct => ct.Chuong)
            .Where(ct => ct.MaTranId == maTran.Id)
            .ToList();

        var cauHoiTheoDieuKien = _dbContext.CauHoi
            .Include(c => c.DsCauTraLoi)
            .Where(c => !c.DaRaDe && c.Chuong != null)
            .ToList();

        bool duCauHoi = chiTietMaTrans.All(ct =>
            cauHoiTheoDieuKien.Count(c =>
                c.MucDo == ct.MucDoCauHoi &&
                c.Loai == ct.LoaiCauHoi &&
                c.ChuongId == ct.ChuongId) >= ct.SoCau);

        if (!duCauHoi)
        {
            foreach (var ch in _dbContext.CauHoi)
                ch.DaRaDe = false;
            _dbContext.SaveChanges();

            cauHoiTheoDieuKien = _dbContext.CauHoi
                .Include(c => c.DsCauTraLoi)
                .Where(c => c.Chuong != null)
                .ToList();
        }

        var rand = new Random();
        var cauHoiGoc = new List<CauHoi>();
        foreach (var ct in chiTietMaTrans)
        {
            var selected = cauHoiTheoDieuKien
                .Where(c => c.MucDo == ct.MucDoCauHoi &&
                            c.Loai == ct.LoaiCauHoi &&
                            c.ChuongId == ct.ChuongId)
                .OrderBy(x => rand.Next())
                .Take(ct.SoCau)
                .ToList();
            cauHoiGoc.AddRange(selected);
        }

        foreach (var ch in cauHoiGoc)
            ch.DaRaDe = true;

        var danhSachDeVuaTao = new List<DeThi>();
        var danhSachDeThiExport = new List<DeThiExportData>();

        for (int i = 0; i < soLuong; i++)
        {
            var deThi = new DeThi
            {
                TieuDe = tieuDe,
                KyThi = kyThi,
                MaDe = maDeBatDau + i,
                MonHocId = monHoc.Id,
                LopHocId = lopHoc.Id,
                MaTranId = maTran.Id,
                CreatedAt = DateTime.Now,
                ThoiGianLamBai = thoiGianLamBai,
                GhiChu = ghiChu,
                DsChiTietDeThi = []
            };

            var deThiExport = new DeThiExportData
            {
                DeThi = deThi
            };

            var boCauHoiXaoTron = cauHoiGoc.OrderBy(_ => rand.Next()).ToList();

            foreach (var ch in boCauHoiXaoTron)
            {
                List<CauTraLoi> dapAnDaTron;

                if (choPhepTronDapAn)
                {
                    var dapAnDuocTron = ch.DsCauTraLoi.Where(d => d.DaoViTri).OrderBy(_ => rand.Next()).ToList();
                    var dapAnKhongTron = ch.DsCauTraLoi.Where(d => !d.DaoViTri).ToList();

                    dapAnDaTron = dapAnDuocTron.Concat(dapAnKhongTron).ToList();

                    for (int viTriMoi = 0; viTriMoi < dapAnDaTron.Count; viTriMoi++)
                    {
                        dapAnDaTron[viTriMoi] = new CauTraLoi
                        {
                            NoiDung = dapAnDaTron[viTriMoi].NoiDung,
                            LaDapAnDung = dapAnDaTron[viTriMoi].LaDapAnDung,
                            DaoViTri = dapAnDaTron[viTriMoi].DaoViTri,
                            ViTriGoc = (byte)viTriMoi,
                            HinhAnh = dapAnDaTron[viTriMoi].HinhAnh
                        };
                    }
                }
                else
                {
                    dapAnDaTron = ch.DsCauTraLoi.Select(d => new CauTraLoi
                    {
                        NoiDung = d.NoiDung,
                        LaDapAnDung = d.LaDapAnDung,
                        DaoViTri = d.DaoViTri,
                        ViTriGoc = d.ViTriGoc,
                        HinhAnh = d.HinhAnh
                    }).OrderBy(d => d.ViTriGoc).ToList();
                }

                // Lưu vào ChiTietDeThi
                var chiTietDeThi = new ChiTietDeThi
                {
                    CauHoiId = ch.Id,
                    DsDapAnTrongDe = dapAnDaTron.Select((d, index) => new ChiTietCauTraLoiTrongDeThi
                    {
                        NoiDung = d.NoiDung,
                        LaDapAnDung = d.LaDapAnDung,
                        ViTri = (byte)index,
                        HinhAnh = d.HinhAnh
                    }).ToList()
                };

                deThi.DsChiTietDeThi.Add(chiTietDeThi);
                deThiExport.CauHoiVaDapAn.Add((ch, dapAnDaTron));
            }

            _dbContext.DeThi.Add(deThi);
            danhSachDeVuaTao.Add(deThi);
            danhSachDeThiExport.Add(deThiExport);
        }

        _dbContext.SaveChanges();
        LoadDeThi();

        for (int i = 0; i < danhSachDeVuaTao.Count; i++)
        {
            var exportData = danhSachDeThiExport[i];
            var deThi = danhSachDeVuaTao[i];

            var saveFileDialog = new Microsoft.Win32.SaveFileDialog
            {
                Title = $"Lưu đề thi: {exportData.DeThi.TieuDe}_{exportData.DeThi.MaDe}",
                FileName = $"{exportData.DeThi.TieuDe}_{exportData.DeThi.MaDe}.docx",
                Filter = "Word Document (*.docx)|*.docx"
            };

            if (saveFileDialog.ShowDialog() == true)
            {
                // 1. Lưu đề thi
                ExportDeThiToWordService.Export(exportData, saveFileDialog.FileName, _baseImageFolder);

                // 2. Lưu đáp án ra file riêng (ví dụ cùng thư mục)
                var dapAnFilePath = Path.Combine(
                    Path.GetDirectoryName(saveFileDialog.FileName)!,
                    Path.GetFileNameWithoutExtension(saveFileDialog.FileName) + "_DapAn.docx");

                // Load lại DsChiTietDeThi từ DB (nếu cần)
                var deThiFull = _dbContext.DeThi
                    .Include(d => d.DsChiTietDeThi)
                    .ThenInclude(ct => ct.DsDapAnTrongDe)
                    .First(d => d.Id == deThi.Id);

                ExportDapAnService.Export(deThiFull, dapAnFilePath, _baseImageFolder);
            }
        }
    }

    private void BtnXuatDeThi_Click(object sender, RoutedEventArgs e)
    {
        if ((sender as Button)?.Tag is not DeThi selectedDeThi) return;

        // Load đầy đủ câu hỏi và đáp án
        var deThiDayDu = _dbContext.DeThi
            .Include(x => x.MonHoc)
            .Include(x => x.LopHoc)
            .Include(d => d.DsChiTietDeThi)
                .ThenInclude(ct => ct.CauHoi)
            .Include(d => d.DsChiTietDeThi)
                .ThenInclude(ct => ct.DsDapAnTrongDe)
            .FirstOrDefault(d => d.Id == selectedDeThi.Id);

        if (deThiDayDu == null)
        {
            MessageBox.Show("Không tìm thấy dữ liệu đề thi.", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            return;
        }

        // Tạo dữ liệu cho xuất Word
        var exportData = new DeThiExportData
        {
            DeThi = deThiDayDu
        };

        foreach (var chiTiet in deThiDayDu.DsChiTietDeThi.OrderBy(ct => ct.Id))
        {
            var ch = chiTiet.CauHoi;

            var dapAnDaTron = chiTiet.DsDapAnTrongDe
                .OrderBy(d => d.ViTri)
                .Select(d => new CauTraLoi
                {
                    NoiDung = d.NoiDung,
                    LaDapAnDung = d.LaDapAnDung,
                    ViTriGoc = d.ViTri, // dùng ViTri của bản trộn
                    DaoViTri = false // không quan trọng ở đây
                }).ToList();

            exportData.CauHoiVaDapAn.Add((ch, dapAnDaTron));
        }

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
                ExportDeThiToWordService.Export(exportData, saveFileDialog.FileName, _baseImageFolder);
                var dapAnPath = Path.Combine(
                    Path.GetDirectoryName(saveFileDialog.FileName)!,
                    Path.GetFileNameWithoutExtension(saveFileDialog.FileName) + "_DapAn.docx");

                ExportDapAnService.Export(deThiDayDu, dapAnPath);

                MessageBox.Show("Xuất đề thi thành công!", "Thành công", MessageBoxButton.OK, MessageBoxImage.Information);
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
