using Microsoft.Win32;
using NganHangDeThi.Data.DataContext;
using NganHangDeThi.Data.Entity;
using NganHangDeThi.Helpers;
using NganHangDeThi.Models;
using NganHangDeThi.Services;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;

namespace NganHangDeThi;

public partial class ThemCauHoiTuFileWindow : Window, INotifyPropertyChanged
{
    private readonly AppDbContext _db;
    private readonly QuestionExtractorService _questionExtractorService;
    public ObservableCollection<CauHoiRaw> DsCauHoiRaw { get; set; } = new();
    public ObservableCollection<MonHoc> DsMonHoc { get; set; } = [];
    public ObservableCollection<Chuong> DsChuongTheoMonHoc { get; set; } = [];

    private MonHoc? _monHocDangChon;
    public MonHoc? MonHocDangChon
    {
        get => _monHocDangChon;
        set
        {
            if (_monHocDangChon != value)
            {
                _monHocDangChon = value;
                OnPropertyChanged(nameof(MonHocDangChon));
                LoadDsChuongTheoMonHoc();
            }
        }
    }

    private Chuong? _chuongDangChon;
    public Chuong? ChuongDangChon
    {
        get => _chuongDangChon;
        set
        {
            if (_chuongDangChon != value)
            {
                _chuongDangChon = value;
                OnPropertyChanged(nameof(ChuongDangChon));
            }
        }
    }

    public ThemCauHoiTuFileWindow(AppDbContext dbContext, QuestionExtractorService questionExtractorService) 
    {
        InitializeComponent();
        DataContext = this;
        _db = dbContext;
        _questionExtractorService = questionExtractorService;

        LoadDsMonHoc();
    }

    private void LoadDsMonHoc()
    {
        DsMonHoc.Clear();
        var monHocs = _db.MonHoc.ToList();
        foreach (var mon in monHocs)
        {
            DsMonHoc.Add(mon);
        }

        // Gán mặc định môn đầu tiên nếu có
        if (DsMonHoc.Any())
        {
            MonHocDangChon = DsMonHoc.First();
        }
    }

    private void LoadDsChuongTheoMonHoc()
    {
        DsChuongTheoMonHoc.Clear();

        if (MonHocDangChon != null)
        {
            var chuongs = _db.Chuong
                .Where(c => c.MonHocId == MonHocDangChon.Id)
                .OrderBy(c => c.ViTri)
                .ToList();

            foreach (var ch in chuongs)
            {
                DsChuongTheoMonHoc.Add(ch);
            }

            // Gán mặc định chương đầu tiên nếu có
            if (DsChuongTheoMonHoc.Any())
            {
                ChuongDangChon = DsChuongTheoMonHoc.First();
            }
        }
    }

    private void BtnImportFile_Click(object sender, RoutedEventArgs e)
    {
        var openFileDialog = new OpenFileDialog
        {
            Filter = "Word Documents (*.docx)|*.docx",
            Title = "Chọn file Word chứa câu hỏi"
        };

        if (openFileDialog.ShowDialog() == true)
        {
            string filePath = openFileDialog.FileName;

            if (FileHelpers.IsFileLocked(filePath))
            {
                MessageBox.Show("File đang được sử dụng bởi một ứng dụng khác (có thể là Word).\nVui lòng đóng file rồi thử lại.",
                            "Cảnh báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var cauHoiRawList = _questionExtractorService.ExtractQuestionsFromDocx(filePath);

            DsCauHoiRaw.Clear();
            foreach (var cauHoi in cauHoiRawList)
            {
                DsCauHoiRaw.Add(cauHoi);
            }
        }
    }

    // INotifyPropertyChanged implementation
    public event PropertyChangedEventHandler? PropertyChanged;
    protected void OnPropertyChanged(string propertyName)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

    private void BtnCancel_Click(object sender, RoutedEventArgs e)
    {
        _questionExtractorService.CleanupTemporaryImages();
        Close();
    }

    private void BtnSave_Click(object sender, RoutedEventArgs e)
    {
        if (ChuongDangChon == null)
        {
            MessageBox.Show("Vui lòng chọn chương để lưu vào.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        if (!DsCauHoiRaw.Any())
        {
            MessageBox.Show("Chưa có câu hỏi để lưu.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        try
        {
            int soLuong = _questionExtractorService.SaveToDatabase([.. DsCauHoiRaw], ChuongDangChon.Id);
            _questionExtractorService.CommitImages();

            MessageBox.Show($"Đã lưu {soLuong} câu hỏi vào CSDL.", "Thành công", MessageBoxButton.OK, MessageBoxImage.Information);
            //MessageBox.Show($"Đã xử lý thành công {soLuong} mục dữ liệu (bao gồm cả đoạn văn và câu hỏi).", "Thành công", MessageBoxButton.OK, MessageBoxImage.Information);
            DialogResult = true;
            Close();
        }
        catch (Exception ex)
        {
            _questionExtractorService.CleanupTemporaryImages();
            MessageBox.Show($"Lưu câu hỏi thất bại.\n\nChi tiết: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            DialogResult = false;
        }
    }

    private void MainBorder_MouseDown(object sender, MouseButtonEventArgs e)
    {
        if (e.ChangedButton == MouseButton.Left)
        {
            DragMove();
        }
    }

    private bool _isMaximized = false;
    private void MainBorder_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (e.ClickCount == 2)
        {
            if (_isMaximized)
            {
                WindowState = WindowState.Normal;
                Width = 1080;
                Height = 720;
                _isMaximized = false;
            }
            else
            {
                WindowState = WindowState.Maximized;
                _isMaximized = true;
            }
        }
    }

    private void BtnThemMonNhanh_Click(object sender, RoutedEventArgs e)
    {
        var window = new ThemMonHocWindow
        {
            Owner = this
        };

        if (window.ShowDialog() == true && window.MonHocMoi != null)
        {
            try
            {
                // Lưu vào CSDL
                _db.MonHoc.Add(window.MonHocMoi);
                _db.SaveChanges();

                // Tải lại danh sách môn
                LoadDsMonHoc();

                // Tự động chọn môn vừa tạo
                // (Cần tìm object trong DsMonHoc tương ứng với Id mới tạo để Binding hoạt động đúng)
                MonHocDangChon = DsMonHoc.FirstOrDefault(m => m.Id == window.MonHocMoi.Id);

                MessageBox.Show($"Đã thêm môn \"{window.MonHocMoi.TenMon}\" thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khi lưu môn học: " + ex.Message, "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }

    // 2. Xử lý thêm nhanh Chương
    private void BtnThemChuongNhanh_Click(object sender, RoutedEventArgs e)
    {
        // Kiểm tra xem đã chọn môn học chưa
        if (MonHocDangChon == null)
        {
            MessageBox.Show("Vui lòng chọn Môn học trước khi thêm Chương.", "Cảnh báo", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        var window = new ThemChuongWindow(MonHocDangChon.Id)
        {
            Owner = this
        };

        if (window.ShowDialog() == true && window.ChuongMoi != null)
        {
            try
            {
                // Lưu vào CSDL
                _db.Chuong.Add(window.ChuongMoi);
                _db.SaveChanges();

                // Tải lại danh sách chương (Hàm LoadDsChuongTheoMonHoc sẽ chạy vì MonHocDangChon không đổi, ta cần gọi thủ công hoặc refresh)
                LoadDsChuongTheoMonHoc();

                // Tự động chọn chương vừa tạo
                ChuongDangChon = DsChuongTheoMonHoc.FirstOrDefault(c => c.Id == window.ChuongMoi.Id);

                MessageBox.Show($"Đã thêm chương \"{window.ChuongMoi.TenChuong}\" thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khi lưu chương: " + ex.Message, "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}

