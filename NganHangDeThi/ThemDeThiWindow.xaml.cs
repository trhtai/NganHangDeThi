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

        // 2. Load cấu trúc ma trận
        var chiTietMaTrans = _dbContext.ChiTietMaTran
            .Include(ct => ct.Chuong)
            .Where(ct => ct.MaTranId == SelectedMaTran.Id)
            .ToList();

        // 3. Load kho câu hỏi (Lấy cả câu con và đáp án để xử lý trọn gói)
        var khoCauHoi = _dbContext.CauHoi
            .Include(c => c.DsCauTraLoi)
            .Include(c => c.DsCauHoiCon).ThenInclude(child => child.DsCauTraLoi)
            .Where(c => c.ParentId == null) // Chỉ lấy câu gốc (bao gồm câu đơn và câu cha của chùm)
            .ToList();

        var rand = new Random();
        var boCauHoiGoc = new List<CauHoi>();

        // 4. BỐC CÂU HỎI THEO MA TRẬN (Tạo Bộ Câu Hỏi Gốc)
        foreach (var ct in chiTietMaTrans)
        {
            // Lấy các ứng viên phù hợp với tiêu chí (Mức độ, Loại, Chương) và chưa được chọn vào đề này
            var candidates = khoCauHoi
                .Where(c => c.MucDo == ct.MucDoCauHoi &&
                            c.Loai == ct.LoaiCauHoi &&
                            c.ChuongId == ct.ChuongId &&
                            !boCauHoiGoc.Contains(c)) // Tránh trùng lặp
                .OrderBy(x => rand.Next()) // Trộn ngẫu nhiên để lấy
                .ToList();

            int daChon = 0;
            foreach (var cau in candidates)
            {
                if (daChon >= ct.SoCau) break;

                // Tính trọng số: Nếu là câu chùm thì đếm số câu con, câu đơn tính là 1
                int trongSo = (cau.DsCauHoiCon != null && cau.DsCauHoiCon.Any()) ? cau.DsCauHoiCon.Count : 1;

                // Kiểm tra xem nếu thêm câu này vào có bị vượt quá số lượng yêu cầu không
                if (daChon + trongSo <= ct.SoCau)
                {
                    boCauHoiGoc.Add(cau);
                    daChon += trongSo;
                }
            }

            // Nếu không đủ câu hỏi
            if (daChon < ct.SoCau)
            {
                MessageBox.Show($"Không đủ câu hỏi cho chương '{ct.Chuong.TenChuong}' mức độ {ct.MucDoCauHoi}. Cần {ct.SoCau}, chỉ tìm được {daChon}.", "Thiếu dữ liệu", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
        }

        // Đánh dấu đã ra đề cho bộ câu hỏi gốc
        foreach (var q in boCauHoiGoc) q.DaRaDe = true;

        // 5. SINH CÁC MÃ ĐỀ (Permutation)
        // Tạo một ID chung cho đợt tạo đề này để dễ quản lý (nếu cần)
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

            // Xác định đây có phải là đề gốc (Mã đầu tiên) không?
            bool laDeGoc = (i == 0);

            // Danh sách câu hỏi cho đề này
            List<CauHoi> dsCauHoiCuaDe;

            if (laDeGoc)
            {
                // Đề gốc: Giữ nguyên thứ tự từ lúc bốc (thường là theo chương)
                dsCauHoiCuaDe = new List<CauHoi>(boCauHoiGoc);
                deThi.GhiChu += " (Đề gốc)";
            }
            else
            {
                // Các đề sau: Trộn ngẫu nhiên vị trí các câu hỏi
                dsCauHoiCuaDe = boCauHoiGoc.OrderBy(x => rand.Next()).ToList();
            }

            // Duyệt từng câu hỏi để đưa vào đề
            foreach (var ch in dsCauHoiCuaDe)
            {
                // Flatten: Nếu là câu chùm -> lấy câu con. Nếu câu đơn -> lấy chính nó.
                var listCauHoiCanXuLy = new List<CauHoi>();
                if (ch.DsCauHoiCon != null && ch.DsCauHoiCon.Count > 0)
                {
                    // Với câu chùm, ta giữ nguyên thứ tự câu con (để đảm bảo mạch bài đọc)
                    listCauHoiCanXuLy.AddRange(ch.DsCauHoiCon.OrderBy(x => x.Id));
                }
                else
                {
                    listCauHoiCanXuLy.Add(ch);
                }

                foreach (var qSub in listCauHoiCanXuLy)
                {
                    List<CauTraLoi> dapAnDaXuLy;

                    // Logic trộn đáp án:
                    // Nếu là đề gốc -> Không trộn (giữ nguyên A,B,C,D chuẩn)
                    // Nếu là đề khác và có chọn trộn -> Trộn (trừ các câu cố định <@>)
                    if (!laDeGoc && ChoPhepTronDapAn)
                    {
                        var dapAnDuocTron = qSub.DsCauTraLoi.Where(d => d.DaoViTri).OrderBy(_ => rand.Next()).ToList();
                        var dapAnCoDinh = qSub.DsCauTraLoi.Where(d => !d.DaoViTri).ToList(); // Vị trí cố định (VD: Cả A,B đều đúng)

                        // Ở đây ta đơn giản nối cố định vào sau, hoặc bạn có thể xử lý phức tạp hơn
                        dapAnDaXuLy = dapAnDuocTron.Concat(dapAnCoDinh).ToList();
                    }
                    else
                    {
                        // Giữ nguyên thứ tự gốc
                        dapAnDaXuLy = qSub.DsCauTraLoi.OrderBy(d => d.ViTriGoc).ToList();
                    }

                    // Tạo ChiTietDeThi lưu vào DB
                    var chiTiet = new ChiTietDeThi
                    {
                        CauHoiId = qSub.Id,
                        DsDapAnTrongDe = dapAnDaXuLy.Select((d, idx) => new ChiTietCauTraLoiTrongDeThi
                        {
                            NoiDung = d.NoiDung,
                            LaDapAnDung = d.LaDapAnDung,
                            ViTri = (byte)idx, // Vị trí mới (0=A, 1=B...)
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