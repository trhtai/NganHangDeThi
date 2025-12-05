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
        // 1. LẤY DỮ LIỆU TỪ GIAO DIỆN & VALIDATE
        //if (!int.TryParse(TxtSoLuongDe.textBox.Text, out int soLuongDeThucTe)) soLuongDeThucTe = 1;
        //if (!int.TryParse(TxtMaDeBatDau.textBox.Text, out int maDeBatDauThucTe)) maDeBatDauThucTe = 101;
        //if (!int.TryParse(TxtThoiGianLamBai.textBox.Text, out int thoiGianLamBaiThucTe)) thoiGianLamBaiThucTe = 60;

        if (SoLuongDe <= 0 || SelectedMaTran == null || SelectedMonHoc == null || SelectedLopHoc == null)
        {
            MessageBox.Show("Vui lòng kiểm tra lại thông tin!", "Thiếu thông tin", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        // 2. LOAD CẤU TRÚC MA TRẬN
        // Gom nhóm để giữ thứ tự Chương/Phần (tránh việc trộn đề làm đảo lộn cấu trúc chương)
        var rawChiTietMaTrans = _dbContext.ChiTietMaTran
            .Include(ct => ct.Chuong)
            .Where(ct => ct.MaTranId == SelectedMaTran.Id)
            .ToList();

        var cauTrucDeThi = rawChiTietMaTrans
            .GroupBy(x => new { x.ChuongId, x.MucDoCauHoi, x.LoaiCauHoi })
            .Select(g => new
            {
                FirstId = g.Min(item => item.Id), // Dùng ID này để sort giữ thứ tự
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

        // 3. BỐC CÂU HỎI (CÓ CƠ CHẾ RESET THÔNG MINH)
        using var transaction = _dbContext.Database.BeginTransaction();
        try
        {
            var daChonIds = new HashSet<int>();

            foreach (var phan in cauTrucDeThi)
            {
                // Hàm nội bộ: Tìm câu hỏi theo tiêu chí
                List<CauHoi> TimCauHoi(int soLuongCan)
                {
                    // Lưu ý: Dùng AsNoTracking() ở đây chưa đủ nếu Context bị bẩn, 
                    // nhưng ChangeTracker.Clear() bên dưới sẽ lo việc đó.
                    var query = _dbContext.CauHoi
                        .Include(c => c.DsCauTraLoi)
                        .Include(c => c.DsCauHoiCon).ThenInclude(ch => ch.DsCauTraLoi)
                        .Where(c => c.ChuongId == phan.ChuongId &&
                                    c.MucDo == phan.MucDo &&
                                    c.Loai == phan.Loai &&
                                    c.ParentId == null);

                    var candidates = query.AsEnumerable()
                                          .Where(c => !c.DaRaDe && !daChonIds.Contains(c.Id))
                                          .OrderBy(x => rand.Next())
                                          .ToList();

                    var ketQua = new List<CauHoi>();
                    int daLay = 0;

                    foreach (var cau in candidates)
                    {
                        if (daLay >= soLuongCan) break;
                        int trongSo = (cau.DsCauHoiCon != null && cau.DsCauHoiCon.Any()) ? cau.DsCauHoiCon.Count : 1;

                        if (daLay + trongSo <= soLuongCan)
                        {
                            ketQua.Add(cau);
                            daLay += trongSo;
                        }
                    }
                    return ketQua;
                }

                // --- Lần 1: Thử lấy ---
                var listDaChon = TimCauHoi(phan.SoCauCanLay);
                int demDuoc = listDaChon.Sum(q => (q.DsCauHoiCon != null && q.DsCauHoiCon.Any()) ? q.DsCauHoiCon.Count : 1);

                // --- Lần 2: Nếu thiếu -> Reset DB -> Xóa Cache -> Lấy tiếp ---
                if (demDuoc < phan.SoCauCanLay)
                {
                    // 1. Reset DB (SQL thuần)
                    _dbContext.Database.ExecuteSqlRaw(
                        "UPDATE CauHoi SET DaRaDe = 0 WHERE ChuongId IN (SELECT Id FROM Chuong WHERE MonHocId = {0})",
                        SelectedMonHoc.Id);

                    // 2. QUAN TRỌNG: Xóa bộ nhớ đệm để EF biết DB đã thay đổi
                    _dbContext.ChangeTracker.Clear();

                    // 3. Lấy tiếp phần còn thiếu
                    int conThieu = phan.SoCauCanLay - demDuoc;
                    var listBoSung = TimCauHoi(conThieu); // Lúc này DB đã reset, Query sẽ thấy câu hỏi

                    listDaChon.AddRange(listBoSung);

                    // Cập nhật lại số lượng
                    demDuoc = listDaChon.Sum(q => (q.DsCauHoiCon != null && q.DsCauHoiCon.Any()) ? q.DsCauHoiCon.Count : 1);
                }

                // --- Kiểm tra cuối cùng ---
                if (demDuoc < phan.SoCauCanLay)
                {
                    MessageBox.Show($"Không đủ câu hỏi cho phần: {phan.TenChuong} - {phan.MucDo}.\nCần: {phan.SoCauCanLay}, Có: {demDuoc}.",
                        "Thiếu dữ liệu", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                // Add vào bộ gốc
                foreach (var q in listDaChon)
                {
                    boCauHoiGoc.Add(q);
                    daChonIds.Add(q.Id);
                }
            }

            // --- CẬP NHẬT TRẠNG THÁI DARADE ---
            // Phải cập nhật lại toàn bộ vì lệnh Reset ở giữa vòng lặp có thể đã xóa trạng thái của các câu đã chọn ở phần trước
            foreach (var q in boCauHoiGoc)
            {
                // Do ChangeTracker.Clear() nên object có thể bị detach, cần Attach lại
                if (_dbContext.Entry(q).State == EntityState.Detached)
                {
                    _dbContext.CauHoi.Attach(q);
                }

                q.DaRaDe = true;
                if (q.DsCauHoiCon != null)
                {
                    foreach (var child in q.DsCauHoiCon)
                    {
                        // Attach child nếu cần thiết (thường EF tự lo qua nav prop nhưng an toàn thì check)
                        child.DaRaDe = true;
                    }
                }
            }

            _dbContext.SaveChanges();
            transaction.Commit();
        }
        catch (Exception ex)
        {
            transaction.Rollback();
            MessageBox.Show("Lỗi: " + ex.Message, "Lỗi hệ thống", MessageBoxButton.OK, MessageBoxImage.Error);
            return;
        }

        // 4. SINH ĐỀ THI (HOÁN VỊ TỪ BỘ GỐC)
        var thoiGianTao = DateTime.Now;
        var batchId = Guid.NewGuid();

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
                CreatedAt = thoiGianTao,
                ThoiGianLamBai = ThoiGianLamBai,
                GhiChu = GhiChu,
                BatchId = batchId,
                DaThi = false
            };

            bool laDeGoc = (i == 0);
            if (laDeGoc) deThi.GhiChu += " (Đề gốc)";

            List<CauHoi> dsCauHoiCuaDe = new List<CauHoi>();

            if (laDeGoc)
            {
                // Đề gốc: Giữ nguyên thứ tự từ Ma trận
                dsCauHoiCuaDe = new List<CauHoi>(boCauHoiGoc);
            }
            else
            {
                // Đề hoán vị: Trộn câu hỏi NHƯNG GIỮ CẤU TRÚC (theo từng phần ma trận)
                foreach (var phan in cauTrucDeThi)
                {
                    var cauHoiPhanNay = boCauHoiGoc
                        .Where(c => c.ChuongId == phan.ChuongId &&
                                    c.MucDo == phan.MucDo &&
                                    c.Loai == phan.Loai)
                        .OrderBy(x => rand.Next()) // Chỉ trộn nội bộ
                        .ToList();
                    dsCauHoiCuaDe.AddRange(cauHoiPhanNay);
                }
            }

            // Tạo chi tiết đề
            foreach (var ch in dsCauHoiCuaDe)
            {
                var listCauCanXuLy = new List<CauHoi>();
                if (ch.DsCauHoiCon != null && ch.DsCauHoiCon.Any())
                    listCauCanXuLy.AddRange(ch.DsCauHoiCon.OrderBy(x => x.Id));
                else
                    listCauCanXuLy.Add(ch);

                foreach (var qSub in listCauCanXuLy)
                {
                    List<CauTraLoi> dapAnFinal;

                    // Nếu ChoPhepTronDapAn = true -> Trộn cả đề gốc lẫn đề hoán vị
                    if (ChoPhepTronDapAn)
                    {
                        var dapAnTron = qSub.DsCauTraLoi.Where(d => d.DaoViTri).OrderBy(x => rand.Next()).ToList();
                        var dapAnCoDinh = qSub.DsCauTraLoi.Where(d => !d.DaoViTri).OrderBy(d => d.ViTriGoc).ToList();
                        dapAnFinal = dapAnTron.Concat(dapAnCoDinh).ToList();
                    }
                    else
                    {
                        dapAnFinal = qSub.DsCauTraLoi.OrderBy(d => d.ViTriGoc).ToList();
                    }

                    var chiTiet = new ChiTietDeThi
                    {
                        CauHoiId = qSub.Id,
                        // Lưu ý: Không gán object CauHoi ở đây để tránh lỗi EF insert lại, chỉ gán ID
                        DsDapAnTrongDe = dapAnFinal.Select((d, idx) => new ChiTietCauTraLoiTrongDeThi
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
        MessageBox.Show($"Đã tạo thành công {SoLuongDe} đề thi!", "Thành công", MessageBoxButton.OK, MessageBoxImage.Information);
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