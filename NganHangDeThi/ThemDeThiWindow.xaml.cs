using Microsoft.EntityFrameworkCore;
using NganHangDeThi.Data.DataContext;
using NganHangDeThi.Data.Entity;
using NganHangDeThi.Common.Enum;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;

namespace NganHangDeThi;

public partial class ThemDeThiWindow : Window, INotifyPropertyChanged
{
    // --- Properties Binding ---
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

    // --- BỔ SUNG: Property binding cho RadioButton ---
    private bool _isTaoDeHoanVi = true;
    public bool IsTaoDeHoanVi
    {
        get => _isTaoDeHoanVi;
        set { _isTaoDeHoanVi = value; OnPropertyChanged(nameof(IsTaoDeHoanVi)); }
    }

    public bool IsTaoDeNgauNhien
    {
        get => !_isTaoDeHoanVi;
        set { _isTaoDeHoanVi = !value; OnPropertyChanged(nameof(IsTaoDeNgauNhien)); }
    }
    // --------------------------------------------------

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
            SelectedMonHoc = DsMonHoc.FirstOrDefault(m => m.Id == monHocId);
        else
            SelectedMonHoc = null;

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

    // --- LOGIC TẠO ĐỀ THI ---
    private void BtnTao_Click(object sender, RoutedEventArgs e)
    {
        // 1. VALIDATE
        if (SoLuongDe <= 0 || SelectedMaTran == null || SelectedMonHoc == null || SelectedLopHoc == null)
        {
            MessageBox.Show("Vui lòng kiểm tra lại thông tin!", "Thiếu thông tin", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        // 2. LOAD CẤU TRÚC MA TRẬN
        var rawChiTietMaTrans = _dbContext.ChiTietMaTran
            .Include(ct => ct.Chuong)
            .Where(ct => ct.MaTranId == SelectedMaTran.Id)
            .ToList();

        // Gom nhóm cấu trúc
        var cauTrucDeThi = rawChiTietMaTrans
            .GroupBy(x => new { x.ChuongId, x.MucDoCauHoi, x.LoaiCauHoi })
            .Select(g => new CauTrucPhanThi
            {
                FirstId = g.Min(item => item.Id),
                ChuongId = g.Key.ChuongId,
                TenChuong = g.First().Chuong.TenChuong,
                MucDo = g.Key.MucDoCauHoi,
                Loai = g.Key.LoaiCauHoi,
                SoCauCanLay = g.Sum(x => x.SoCau)
            })
            .OrderBy(x => x.FirstId)
            .ToList();

        var rand = new Random();
        var thoiGianTao = DateTime.Now;
        var batchId = Guid.NewGuid();

        // HashSet lưu ID đã dùng trong đợt tạo này (để tránh trùng lặp giữa các đề ngẫu nhiên)
        var globalUsedIds = new HashSet<int>();

        using var transaction = _dbContext.Database.BeginTransaction();
        try
        {
            List<DeThi> dsDeThiDuocTao = new List<DeThi>();

            if (IsTaoDeHoanVi)
            {
                // === CASE 1: ĐỀ HOÁN VỊ (Logic cũ) ===
                // Chỉ lấy 1 bộ gốc duy nhất
                var boCauHoiGoc = LayBoCauHoiTheoCauTruc(cauTrucDeThi, globalUsedIds, rand);

                for (int i = 0; i < SoLuongDe; i++)
                {
                    var deThi = CreateDeThiObj(i, thoiGianTao, batchId);

                    if (i == 0)
                    {
                        deThi.GhiChu += " (Đề gốc)"; // Đánh dấu đề gốc
                        AddQuestionsToExam(deThi, boCauHoiGoc, rand);
                    }
                    else
                    {
                        // Các đề sau chỉ là hoán vị của bộ gốc
                        var boCauHoiHoanVi = ShuffleQuestionsKeepStructure(boCauHoiGoc, cauTrucDeThi, rand);
                        AddQuestionsToExam(deThi, boCauHoiHoanVi, rand);
                    }
                    dsDeThiDuocTao.Add(deThi);
                }
            }
            else
            {
                // === CASE 2: ĐỀ NGẪU NHIÊN (Logic mới) ===
                for (int i = 0; i < SoLuongDe; i++)
                {
                    // Lấy bộ câu hỏi riêng cho từng đề
                    // globalUsedIds được cập nhật liên tục để đề sau tránh câu hỏi của đề trước
                    var boCauHoiRieng = LayBoCauHoiTheoCauTruc(cauTrucDeThi, globalUsedIds, rand);

                    var deThi = CreateDeThiObj(i, thoiGianTao, batchId);

                    // --- SỬA LỖI HIỂN THỊ: Phải chứa chuỗi "(Đề gốc)" để Converter nhận diện ---
                    deThi.GhiChu += " (Đề gốc) - Ngẫu nhiên";

                    AddQuestionsToExam(deThi, boCauHoiRieng, rand);
                    dsDeThiDuocTao.Add(deThi);
                }
            }

            // Cập nhật trạng thái DaRaDe cho toàn bộ câu hỏi đã dùng
            if (globalUsedIds.Any())
            {
                // Dùng SQL Raw để update nhanh nếu số lượng lớn, hoặc dùng EF loop như dưới
                // Lưu ý: Cần Attach nếu object chưa được track
                var allSelectedQuestions = _dbContext.CauHoi
                    .Include(c => c.DsCauHoiCon)
                    .Where(c => globalUsedIds.Contains(c.Id))
                    .ToList();

                foreach (var q in allSelectedQuestions)
                {
                    q.DaRaDe = true;
                    if (q.DsCauHoiCon != null)
                    {
                        foreach (var child in q.DsCauHoiCon) child.DaRaDe = true;
                    }
                }
            }

            _dbContext.DeThi.AddRange(dsDeThiDuocTao);
            _dbContext.SaveChanges();
            transaction.Commit();

            MessageBox.Show($"Đã tạo thành công {SoLuongDe} đề thi!", "Thành công", MessageBoxButton.OK, MessageBoxImage.Information);
            DialogResult = true;
            Close();
        }
        catch (Exception ex)
        {
            transaction.Rollback();
            MessageBox.Show("Lỗi: " + ex.Message, "Lỗi hệ thống", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    // --- HÀM HELPER: Lấy câu hỏi (Có cơ chế Fallback) ---
    private List<CauHoi> LayBoCauHoiTheoCauTruc(List<CauTrucPhanThi> cauTruc, HashSet<int> globalExcludedIds, Random rand)
    {
        var ketQua = new List<CauHoi>();

        foreach (var phan in cauTruc)
        {
            // Hàm nội bộ tìm kiếm linh hoạt
            List<CauHoi> TimKiem(int soLuongCan, HashSet<int> excludeIds)
            {
                var query = _dbContext.CauHoi
                    .Include(c => c.DsCauTraLoi)
                    .Include(c => c.DsCauHoiCon).ThenInclude(ch => ch.DsCauTraLoi)
                    .Where(c => c.ChuongId == phan.ChuongId &&
                                c.MucDo == phan.MucDo &&
                                c.Loai == phan.Loai &&
                                c.ParentId == null);

                // Lọc Memory để loại trừ ID
                var candidates = query.AsEnumerable()
                                      .Where(c => !excludeIds.Contains(c.Id))
                                      .OrderBy(x => rand.Next())
                                      .ToList();

                var listLayDuoc = new List<CauHoi>();
                int daLay = 0;
                foreach (var cau in candidates)
                {
                    if (daLay >= soLuongCan) break;
                    int trongSo = (cau.DsCauHoiCon != null && cau.DsCauHoiCon.Any()) ? cau.DsCauHoiCon.Count : 1;
                    if (daLay + trongSo <= soLuongCan)
                    {
                        listLayDuoc.Add(cau);
                        daLay += trongSo;
                    }
                }
                return listLayDuoc;
            }

            // [Giai đoạn 1] Tìm câu MỚI TINH (Chưa ra đề + Chưa dùng trong Batch này)
            var excludePhase1 = new HashSet<int>(globalExcludedIds);
            var daRaDeIds = _dbContext.CauHoi.Where(c => c.ChuongId == phan.ChuongId && c.DaRaDe).Select(c => c.Id).ToList();
            excludePhase1.UnionWith(daRaDeIds);

            var listDaChon = TimKiem(phan.SoCauCanLay, excludePhase1);
            int demDuoc = listDaChon.Sum(q => (q.DsCauHoiCon != null && q.DsCauHoiCon.Any()) ? q.DsCauHoiCon.Count : 1);

            // [Giai đoạn 2] Nếu thiếu -> Reset DB -> Tìm câu chưa dùng trong Batch
            if (demDuoc < phan.SoCauCanLay)
            {
                _dbContext.Database.ExecuteSqlRaw(
                    "UPDATE CauHoi SET DaRaDe = 0 WHERE ChuongId IN (SELECT Id FROM Chuong WHERE MonHocId = {0})",
                    SelectedMonHoc!.Id);
                _dbContext.ChangeTracker.Clear();

                int conThieu = phan.SoCauCanLay - demDuoc;
                var excludePhase2 = new HashSet<int>(globalExcludedIds);
                excludePhase2.UnionWith(listDaChon.Select(c => c.Id)); // Tránh trùng với câu vừa chọn

                var listBoSung = TimKiem(conThieu, excludePhase2);
                listDaChon.AddRange(listBoSung);
                demDuoc = listDaChon.Sum(q => (q.DsCauHoiCon != null && q.DsCauHoiCon.Any()) ? q.DsCauHoiCon.Count : 1);
            }

            // [Giai đoạn 3] Nếu VẪN thiếu -> Dùng lại câu hỏi từ các đề trước trong Batch (Fallback)
            if (demDuoc < phan.SoCauCanLay)
            {
                int conThieu = phan.SoCauCanLay - demDuoc;
                // Chỉ cần tránh câu đã chọn cho ĐỀ NÀY (listDaChon), bỏ qua globalExcludedIds
                var excludePhase3 = new HashSet<int>(listDaChon.Select(c => c.Id));

                var listTaiSuDung = TimKiem(conThieu, excludePhase3);
                listDaChon.AddRange(listTaiSuDung);
                demDuoc = listDaChon.Sum(q => (q.DsCauHoiCon != null && q.DsCauHoiCon.Any()) ? q.DsCauHoiCon.Count : 1);
            }

            if (demDuoc < phan.SoCauCanLay)
            {
                throw new Exception($"Ngân hàng không đủ câu hỏi cho phần: {phan.TenChuong}. Cần: {phan.SoCauCanLay}, Có: {demDuoc}.");
            }

            foreach (var q in listDaChon)
            {
                ketQua.Add(q);
                globalExcludedIds.Add(q.Id); // Đánh dấu đã dùng
            }
        }
        return ketQua;
    }

    private DeThi CreateDeThiObj(int index, DateTime createdAt, Guid batchId)
    {
        return new DeThi
        {
            TieuDe = TieuDe,
            KyThi = KyThi,
            MaDe = MaDeBatDau + index,
            MonHocId = SelectedMonHoc!.Id,
            LopHocId = SelectedLopHoc!.Id,
            MaTranId = SelectedMaTran!.Id,
            CreatedAt = createdAt,
            ThoiGianLamBai = ThoiGianLamBai,
            GhiChu = GhiChu,
            BatchId = batchId,
            DaThi = false
        };
    }

    private List<CauHoi> ShuffleQuestionsKeepStructure(List<CauHoi> src, List<CauTrucPhanThi> cauTruc, Random rand)
    {
        var result = new List<CauHoi>();
        foreach (var phan in cauTruc)
        {
            var questionsInPart = src
                .Where(c => c.ChuongId == phan.ChuongId && c.MucDo == phan.MucDo && c.Loai == phan.Loai)
                .OrderBy(x => rand.Next())
                .ToList();
            result.AddRange(questionsInPart);
        }
        return result;
    }

    private void AddQuestionsToExam(DeThi deThi, List<CauHoi> questions, Random rand)
    {
        foreach (var ch in questions)
        {
            var listCauCanXuLy = new List<CauHoi>();
            if (ch.DsCauHoiCon != null && ch.DsCauHoiCon.Any())
                listCauCanXuLy.AddRange(ch.DsCauHoiCon.OrderBy(x => x.Id));
            else
                listCauCanXuLy.Add(ch);

            foreach (var qSub in listCauCanXuLy)
            {
                List<CauTraLoi> dapAnFinal;
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
    }

    private class CauTrucPhanThi
    {
        public int FirstId { get; set; }
        public int ChuongId { get; set; }
        public string TenChuong { get; set; } = "";
        public MucDoCauHoi MucDo { get; set; }
        public LoaiCauHoi Loai { get; set; }
        public int SoCauCanLay { get; set; }
    }

    private void BtnHuy_Click(object sender, RoutedEventArgs e) { DialogResult = false; Close(); }
    public event PropertyChangedEventHandler? PropertyChanged;
    protected void OnPropertyChanged(string name) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    private void Border_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e) { if (e.ChangedButton == System.Windows.Input.MouseButton.Left) DragMove(); }
}