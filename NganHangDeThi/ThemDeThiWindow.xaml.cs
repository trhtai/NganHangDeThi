using Microsoft.EntityFrameworkCore;
using NganHangDeThi.Data.DataContext;
using NganHangDeThi.Data.Entity;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;

namespace NganHangDeThi;

public partial class ThemDeThiWindow : Window, INotifyPropertyChanged
{
    // Các biến Binding giữ nguyên
    public int SoLuongDe { get; set; } = 1;
    public int MaDeBatDau { get; set; } = 101;
    public string TieuDe { get; set; } = "KIỂM TRA CHẤT LƯỢNG CUỐI KÌ...";
    public bool ChoPhepTronDapAn { get; set; } = true;
    public int ThoiGianLamBai { get; set; } = 60;
    public string GhiChu { get; set; } = "Sinh viên không được sử dụng tài liệu";
    public string KyThi { get; set; } = "HỌC KỲ ... NĂM HỌC ... (LẦN ...)";

    public MaTran? SelectedMaTran { get; set; }
    public MonHoc? SelectedMonHoc { get; set; }
    public LopHoc? SelectedLopHoc { get; set; }

    public ObservableCollection<MaTran> DsMaTran { get; set; } = [];
    public ObservableCollection<MonHoc> DsMonHoc { get; set; } = [];
    public ObservableCollection<LopHoc> DsLopHoc { get; set; } = [];

    private readonly AppDbContext _dbContext;

    public ThemDeThiWindow(AppDbContext dbContext)
    {
        InitializeComponent();
        DataContext = this;
        _dbContext = dbContext;

        LoadData();
    }

    private void LoadData()
    {
        DsMaTran = new ObservableCollection<MaTran>(_dbContext.MaTran.ToList());
        DsMonHoc = new ObservableCollection<MonHoc>(_dbContext.MonHoc.ToList());
        DsLopHoc = new ObservableCollection<LopHoc>(_dbContext.LopHoc.ToList());

        OnPropertyChanged(nameof(DsMaTran));
        OnPropertyChanged(nameof(DsMonHoc));
        OnPropertyChanged(nameof(DsLopHoc));
    }

    private void CbbMaTran_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (SelectedMaTran == null)
        {
            SelectedMonHoc = null;
            OnPropertyChanged(nameof(SelectedMonHoc));
            return;
        }

        var monHocId = _dbContext.ChiTietMaTran
            .Where(ct => ct.MaTranId == SelectedMaTran.Id)
            .Select(ct => ct.Chuong.MonHocId)
            .FirstOrDefault();

        if (monHocId > 0)
        {
            SelectedMonHoc = DsMonHoc.FirstOrDefault(m => m.Id == monHocId);
        }
        else
        {
            SelectedMonHoc = null;
        }
        OnPropertyChanged(nameof(SelectedMonHoc));
    }

    private void BtnThemLopHocNhanh_Click(object sender, RoutedEventArgs e)
    {
        var window = new ThemLopHocWindow { Owner = this };
        if (window.ShowDialog() == true && window.LopHocMoi != null)
        {
            _dbContext.LopHoc.Add(window.LopHocMoi);
            _dbContext.SaveChanges();
            DsLopHoc.Add(window.LopHocMoi);
            SelectedLopHoc = window.LopHocMoi;
            MessageBox.Show($"Đã thêm lớp {window.LopHocMoi.MaLop} thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }

    private void BtnTao_Click(object sender, RoutedEventArgs e)
    {
        // 1. Validate dữ liệu
        if (SoLuongDe <= 0 || MaDeBatDau <= 0 || SelectedMaTran == null || SelectedMonHoc == null || SelectedLopHoc == null || ThoiGianLamBai <= 0)
        {
            MessageBox.Show("Vui lòng nhập đầy đủ thông tin!", "Thiếu thông tin", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        // 2. Load cấu trúc ma trận và GOM NHÓM ĐỂ TRÁNH TRÙNG LẶP
        // Lỗi 40 câu thường do DB lưu nhiều dòng chi tiết ma trận giống nhau
        var rawChiTietMaTrans = _dbContext.ChiTietMaTran
            .Include(ct => ct.Chuong)
            .Where(ct => ct.MaTranId == SelectedMaTran.Id)
            .ToList();

        // Group by để gộp các dòng trùng chương/mức độ/loại
        var chiTietMaTrans = rawChiTietMaTrans
            .GroupBy(x => new { x.ChuongId, x.MucDoCauHoi, x.LoaiCauHoi })
            .Select(g => new
            {
                ChuongId = g.Key.ChuongId,
                TenChuong = g.First().Chuong.TenChuong, // Lấy tên chương để hiển thị lỗi nếu cần
                MucDoCauHoi = g.Key.MucDoCauHoi,
                LoaiCauHoi = g.Key.LoaiCauHoi,
                SoCau = g.Sum(x => x.SoCau) // Cộng dồn số câu nếu có nhiều dòng
            })
            .ToList();

        // 3. Load kho câu hỏi (Sử dụng AsSplitQuery để tránh nhân bản dữ liệu do Cartesian Product)
        var khoCauHoi = _dbContext.CauHoi
            .AsSplitQuery() // <-- QUAN TRỌNG: Ngăn chặn nhân bản dữ liệu khi Include nhiều bảng con
            .Include(c => c.DsCauTraLoi)
            .Include(c => c.DsCauHoiCon).ThenInclude(child => child.DsCauTraLoi)
            .Where(c => c.ParentId == null && c.Chuong.MonHocId == SelectedMonHoc.Id) // Thêm lọc theo môn cho chắc chắn
            .ToList();

        var rand = new Random();
        // Sử dụng HashSet để đảm bảo tính duy nhất tuyệt đối theo Id
        var selectedQuestionIds = new HashSet<int>();
        var boCauHoiGoc = new List<CauHoi>();

        // 4. BỐC CÂU HỎI THEO MA TRẬN
        foreach (var ct in chiTietMaTrans)
        {
            // Lấy các ứng viên phù hợp
            var candidates = khoCauHoi
                .Where(c => c.MucDo == ct.MucDoCauHoi &&
                            c.Loai == ct.LoaiCauHoi &&
                            c.ChuongId == ct.ChuongId &&
                            !selectedQuestionIds.Contains(c.Id)) // Kiểm tra trùng bằng ID
                .OrderBy(x => rand.Next())
                .ToList();

            int daChon = 0;
            foreach (var cau in candidates)
            {
                if (daChon >= ct.SoCau) break;

                // Tính trọng số: Nếu là câu chùm thì đếm số câu con, câu đơn tính là 1
                // Cần đảm bảo DsCauHoiCon không null
                int soCauCon = (cau.DsCauHoiCon != null && cau.DsCauHoiCon.Any()) ? cau.DsCauHoiCon.Count : 0;
                int trongSo = soCauCon > 0 ? soCauCon : 1;

                // Kiểm tra: Nếu thêm câu này vào mà vượt quá số lượng yêu cầu thì bỏ qua (trừ khi chưa chọn được câu nào)
                if (daChon + trongSo <= ct.SoCau)
                {
                    boCauHoiGoc.Add(cau);
                    selectedQuestionIds.Add(cau.Id);
                    daChon += trongSo;
                }
            }

            // Nếu không đủ câu hỏi
            if (daChon < ct.SoCau)
            {
                MessageBox.Show($"Thiếu dữ liệu! Chương '{ct.TenChuong}' - {ct.MucDoCauHoi}.\nCần: {ct.SoCau} câu.\nTìm được: {daChon} câu.",
                    "Cảnh báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
        }

        // Đánh dấu đã ra đề
        foreach (var q in boCauHoiGoc) q.DaRaDe = true;

        // 5. SINH CÁC MÃ ĐỀ (Logic giữ nguyên nhưng thêm kiểm tra null)
        var createTime = DateTime.Now;

        for (int i = 0; i < SoLuongDe; i++)
        {
            var deThi = new DeThi
            {
                TieuDe = TieuDe,
                KyThi = KyThi,
                MaDe = MaDeBatDau + i,
                MonHocId = SelectedMonHoc.Id,
                LopHocId = SelectedLopHoc.Id,
                MaTranId = SelectedMaTran.Id,
                CreatedAt = createTime,
                ThoiGianLamBai = ThoiGianLamBai,
                GhiChu = GhiChu,
                DaThi = false,
                DsChiTietDeThi = []
            };

            bool laDeGoc = (i == 0);
            List<CauHoi> dsCauHoiCuaDe;

            if (laDeGoc)
            {
                dsCauHoiCuaDe = new List<CauHoi>(boCauHoiGoc);
                deThi.GhiChu += " (Đề gốc)";
            }
            else
            {
                dsCauHoiCuaDe = boCauHoiGoc.OrderBy(x => rand.Next()).ToList();
            }

            foreach (var ch in dsCauHoiCuaDe)
            {
                var listCauHoiCanXuLy = new List<CauHoi>();
                // Kiểm tra kỹ null trước khi truy cập
                if (ch.DsCauHoiCon != null && ch.DsCauHoiCon.Count > 0)
                {
                    listCauHoiCanXuLy.AddRange(ch.DsCauHoiCon.OrderBy(x => x.Id));
                }
                else
                {
                    listCauHoiCanXuLy.Add(ch);
                }

                foreach (var qSub in listCauHoiCanXuLy)
                {
                    List<CauTraLoi> dapAnDaXuLy;

                    if (!laDeGoc && ChoPhepTronDapAn)
                    {
                        var dapAnDuocTron = qSub.DsCauTraLoi.Where(d => d.DaoViTri).OrderBy(_ => rand.Next()).ToList();
                        var dapAnCoDinh = qSub.DsCauTraLoi.Where(d => !d.DaoViTri).ToList();
                        dapAnDaXuLy = dapAnDuocTron.Concat(dapAnCoDinh).ToList();
                    }
                    else
                    {
                        dapAnDaXuLy = qSub.DsCauTraLoi.OrderBy(d => d.ViTriGoc).ToList();
                    }

                    var chiTiet = new ChiTietDeThi
                    {
                        CauHoiId = qSub.Id,
                        DsDapAnTrongDe = dapAnDaXuLy.Select((d, idx) => new ChiTietCauTraLoiTrongDeThi
                        {
                            NoiDung = d.NoiDung,
                            LaDapAnDung = d.LaDapAnDung,
                            ViTri = (byte)idx,
                            HinhAnh = d.HinhAnh
                        }).ToList()
                    };

                    deThi.DsChiTietDeThi.Add(chiTiet);
                }
            }

            _dbContext.DeThi.Add(deThi);
        }

        _dbContext.SaveChanges();

        MessageBox.Show($"Đã tạo thành công {SoLuongDe} mã đề (bắt đầu từ {MaDeBatDau})!", "Thành công", MessageBoxButton.OK, MessageBoxImage.Information);
        DialogResult = true;
        Close();
    }

    private void BtnHuy_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
        Close();
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    protected void OnPropertyChanged(string name) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

    private void Border_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
    {
        if (e.ChangedButton == System.Windows.Input.MouseButton.Left) DragMove();
    }
}