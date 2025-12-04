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
        // Vì đã fix binding ở bước 1, ta có thể dùng trực tiếp các biến Property
        if (SoLuongDe <= 0 || MaDeBatDau <= 0 || SelectedMaTran == null || SelectedMonHoc == null || SelectedLopHoc == null)
        {
            MessageBox.Show("Vui lòng kiểm tra lại thông tin (Số lượng, Môn, Lớp...)", "Thiếu thông tin", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        // 2. Load cấu trúc ma trận (Gom nhóm để giữ thứ tự Chương/Phần)
        var rawChiTietMaTrans = _dbContext.ChiTietMaTran
            .Include(ct => ct.Chuong)
            .Where(ct => ct.MaTranId == SelectedMaTran.Id)
            .ToList();

        var cauTrucDeThi = rawChiTietMaTrans
            .GroupBy(x => new { x.ChuongId, x.MucDoCauHoi, x.LoaiCauHoi })
            .Select(g => new
            {
                FirstId = g.Min(item => item.Id), // Dùng ID này để sort giữ thứ tự hiển thị
                ChuongId = g.Key.ChuongId,
                TenChuong = g.First().Chuong.TenChuong,
                MucDo = g.Key.MucDoCauHoi,
                Loai = g.Key.LoaiCauHoi,
                SoCauCanLay = g.Sum(x => x.SoCau)
            })
            .OrderBy(x => x.FirstId)
            .ToList();

        var boCauHoiGoc = new List<CauHoi>();
        var rand = new Random();

        // 3. BỐC CÂU HỎI (Logic: Thử -> Thiếu thì Reset -> Thử lại)
        // Dùng Transaction để đảm bảo tính toàn vẹn khi Update DaRaDe
        using var transaction = _dbContext.Database.BeginTransaction();
        try
        {
            var daChonIds = new HashSet<int>(); // Tránh chọn trùng trong 1 lần tạo

            foreach (var phan in cauTrucDeThi)
            {
                // Hàm local: Lấy danh sách câu hỏi theo tiêu chí
                List<CauHoi> TimCauHoi(int soLuongCan)
                {
                    var query = _dbContext.CauHoi
                        .Include(c => c.DsCauTraLoi)
                        .Include(c => c.DsCauHoiCon).ThenInclude(ch => ch.DsCauTraLoi)
                        .Where(c => c.ChuongId == phan.ChuongId &&
                                    c.MucDo == phan.MucDo &&
                                    c.Loai == phan.Loai &&
                                    c.ParentId == null); // Chỉ lấy câu cha/câu đơn

                    // Lọc câu chưa ra đề và chưa được chọn
                    var candidates = query.AsEnumerable()
                                          .Where(c => !c.DaRaDe && !daChonIds.Contains(c.Id))
                                          .OrderBy(x => rand.Next()) // Trộn ngẫu nhiên
                                          .ToList();

                    var ketQua = new List<CauHoi>();
                    int daLay = 0;

                    foreach (var cau in candidates)
                    {
                        if (daLay >= soLuongCan) break;

                        // Nếu là câu chùm: đếm số câu con. Câu đơn: tính là 1.
                        int trongSo = (cau.DsCauHoiCon != null && cau.DsCauHoiCon.Any()) ? cau.DsCauHoiCon.Count : 1;

                        // Chỉ lấy nếu không vượt quá số lượng yêu cầu (hoặc bạn có thể cho phép dư nhẹ ở đây nếu muốn)
                        if (daLay + trongSo <= soLuongCan)
                        {
                            ketQua.Add(cau);
                            daLay += trongSo;
                        }
                    }
                    return ketQua;
                }

                // --- BƯỚC A: Thử lấy lần 1 ---
                var listDaChon = TimCauHoi(phan.SoCauCanLay);
                int demDuoc = listDaChon.Sum(q => (q.DsCauHoiCon != null && q.DsCauHoiCon.Any()) ? q.DsCauHoiCon.Count : 1);

                // --- BƯỚC B: Nếu thiếu -> Reset DaRaDe -> Lấy tiếp phần còn thiếu ---
                if (demDuoc < phan.SoCauCanLay)
                {
                    // Reset toàn bộ câu hỏi của Môn học hiện tại (để công bằng cho các chương khác)
                    _dbContext.Database.ExecuteSqlRaw(
                        "UPDATE CauHoi SET DaRaDe = 0 WHERE ChuongId IN (SELECT Id FROM Chuong WHERE MonHocId = {0})",
                        SelectedMonHoc.Id);

                    // Cần SaveChanges để lệnh update có hiệu lực ngay trong transaction này (hoặc transaction tự lo)
                    _dbContext.SaveChanges();

                    int conThieu = phan.SoCauCanLay - demDuoc;
                    var listBoSung = TimCauHoi(conThieu);

                    listDaChon.AddRange(listBoSung);

                    // Cập nhật lại số lượng
                    demDuoc = listDaChon.Sum(q => (q.DsCauHoiCon != null && q.DsCauHoiCon.Any()) ? q.DsCauHoiCon.Count : 1);
                }

                // --- BƯỚC C: Kiểm tra cuối cùng ---
                if (demDuoc < phan.SoCauCanLay)
                {
                    MessageBox.Show($"Không đủ câu hỏi cho phần: {phan.TenChuong} - {phan.MucDo}.\n" +
                        $"Cần: {phan.SoCauCanLay}, Tìm được: {demDuoc}.\n\n" +
                        "Ngân hàng câu hỏi không đủ đáp ứng ngay cả khi đã reset.",
                        "Thiếu dữ liệu", MessageBoxButton.OK, MessageBoxImage.Error);
                    return; // Dừng lại, rollback transaction
                }

                // --- BƯỚC D: Thêm vào bộ gốc và Đánh dấu ---
                foreach (var q in listDaChon)
                {
                    boCauHoiGoc.Add(q);
                    daChonIds.Add(q.Id);

                    // Đánh dấu đã dùng
                    q.DaRaDe = true;
                    if (q.DsCauHoiCon != null)
                    {
                        foreach (var child in q.DsCauHoiCon) child.DaRaDe = true;
                    }
                }
            }

            _dbContext.SaveChanges();
            transaction.Commit();
        }
        catch (Exception ex)
        {
            transaction.Rollback();
            MessageBox.Show("Lỗi trong quá trình lấy câu hỏi: " + ex.Message, "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            return;
        }

        // --- 4. SINH ĐỀ THI (HOÁN VỊ TỪ BỘ GỐC) ---
        var thoiGianTao = DateTime.Now;

        for (int i = 0; i < SoLuongDe; i++) // Vòng lặp chạy đúng số lượng đề
        {
            var deThi = new DeThi
            {
                TieuDe = TieuDe,
                KyThi = KyThi,
                MaDe = MaDeBatDau + i,
                MonHocId = SelectedMonHoc.Id,
                LopHocId = SelectedLopHoc.Id,
                MaTranId = SelectedMaTran.Id,
                CreatedAt = thoiGianTao,
                ThoiGianLamBai = ThoiGianLamBai,
                GhiChu = GhiChu,
                DaThi = false,
                DsChiTietDeThi = []
            };

            bool laDeGoc = (i == 0);
            if (laDeGoc) deThi.GhiChu += " (Đề gốc)";

            // Xây dựng danh sách câu hỏi cho mã đề này
            List<CauHoi> dsCauHoiCuaDe = new List<CauHoi>();

            if (laDeGoc)
            {
                // Đề gốc: Giữ nguyên thứ tự gốc (đã sắp xếp theo cấu trúc ma trận)
                dsCauHoiCuaDe = new List<CauHoi>(boCauHoiGoc);
            }
            else
            {
                // Đề hoán vị: Trộn câu hỏi NHƯNG PHẢI GIỮ CẤU TRÚC (Chương 1 xong mới tới Chương 2)
                foreach (var phan in cauTrucDeThi)
                {
                    // Lọc ra các câu thuộc phần này từ bộ gốc
                    var cauHoiPhanNay = boCauHoiGoc
                        .Where(c => c.ChuongId == phan.ChuongId &&
                                    c.MucDo == phan.MucDo &&
                                    c.Loai == phan.Loai)
                        .OrderBy(x => rand.Next()) // Chỉ trộn nội bộ trong phần này
                        .ToList();

                    dsCauHoiCuaDe.AddRange(cauHoiPhanNay);
                }
            }

            // Tạo chi tiết đề thi (Xử lý trộn đáp án)
            foreach (var ch in dsCauHoiCuaDe)
            {
                // Lấy danh sách câu đơn hoặc câu con (để lưu vào bảng ChiTietDeThi)
                var listCauCanXuLy = new List<CauHoi>();
                if (ch.DsCauHoiCon != null && ch.DsCauHoiCon.Any())
                {
                    // Nếu là câu chùm -> Lấy các câu con (Giữ nguyên thứ tự câu con trong bài đọc)
                    listCauCanXuLy.AddRange(ch.DsCauHoiCon.OrderBy(x => x.Id));
                }
                else
                {
                    listCauCanXuLy.Add(ch);
                }

                foreach (var qSub in listCauCanXuLy)
                {
                    List<CauTraLoi> dapAnDaXuLy;

                    // Logic trộn đáp án: Áp dụng cho TẤT CẢ các đề nếu được phép
                    if (ChoPhepTronDapAn)
                    {
                        // Tách các đáp án được phép trộn và cố định
                        var dapAnDuocTron = qSub.DsCauTraLoi.Where(d => d.DaoViTri).OrderBy(x => rand.Next()).ToList();
                        var dapAnCoDinh = qSub.DsCauTraLoi.Where(d => !d.DaoViTri).OrderBy(d => d.ViTriGoc).ToList();

                        // Nối lại: Trộn lên trước, Cố định nằm dưới
                        dapAnDaXuLy = dapAnDuocTron.Concat(dapAnCoDinh).ToList();
                    }
                    else
                    {
                        // Giữ nguyên thứ tự gốc
                        dapAnDaXuLy = qSub.DsCauTraLoi.OrderBy(d => d.ViTriGoc).ToList();
                    }

                    // Lưu vào DB
                    var chiTiet = new ChiTietDeThi
                    {
                        CauHoiId = qSub.Id,
                        DsDapAnTrongDe = dapAnDaXuLy.Select((d, idx) => new ChiTietCauTraLoiTrongDeThi
                        {
                            NoiDung = d.NoiDung,
                            LaDapAnDung = d.LaDapAnDung,
                            ViTri = (byte)idx, // Vị trí mới sau khi trộn (0->A, 1->B...)
                            HinhAnh = d.HinhAnh
                        }).ToList()
                    };
                    deThi.DsChiTietDeThi.Add(chiTiet);
                }
            }

            _dbContext.DeThi.Add(deThi);
        }

        _dbContext.SaveChanges();

        MessageBox.Show($"Đã tạo thành công {SoLuongDe} mã đề (từ {MaDeBatDau} đến {MaDeBatDau + SoLuongDe - 1})!",
            "Thành công", MessageBoxButton.OK, MessageBoxImage.Information);
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