using Microsoft.EntityFrameworkCore;
using NganHangDeThi.Data.DataContext;
using NganHangDeThi.Data.Entity;
using NganHangDeThi.Models;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;

namespace NganHangDeThi;

public partial class ThemDeThiWindow : Window, INotifyPropertyChanged
{
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

    private void BtnHuy_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
        Close();
    }

    private void BtnTao_Click(object sender, RoutedEventArgs e)
    {
        if (SoLuongDe <= 0
            || MaDeBatDau <= 0
            || SelectedMaTran == null
            || SelectedMonHoc == null
            || SelectedLopHoc == null
            || ThoiGianLamBai <= 0)
        {
            MessageBox.Show("Vui lòng nhập đầy đủ và hợp lệ các thông tin!", "Thiếu thông tin", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        // Load chi tiết ma trận
        var chiTietMaTrans = _dbContext.ChiTietMaTran
            .Include(ct => ct.Chuong)
            .Where(ct => ct.MaTranId == SelectedMaTran.Id)
            .ToList();

        // FIX: Load nguồn câu hỏi
        // 1. Chỉ lấy câu cha (ParentId == null) để tránh bốc nhầm câu con lẻ loi.
        // 2. Include DsCauHoiCon và DsCauTraLoi của chúng để dùng khi sinh đề.
        var cauHoiTheoDieuKien = _dbContext.CauHoi
            .Include(c => c.DsCauTraLoi)
            .Include(c => c.DsCauHoiCon)
                .ThenInclude(child => child.DsCauTraLoi)
            .Where(c => !c.DaRaDe && c.Chuong != null && c.ParentId == null)
            .ToList();

        // Kiểm tra đủ số lượng (Dựa trên câu cha)
        bool duCauHoi = chiTietMaTrans.All(ct =>
            cauHoiTheoDieuKien.Count(c =>
                c.MucDo == ct.MucDoCauHoi &&
                c.Loai == ct.LoaiCauHoi &&
                c.ChuongId == ct.ChuongId) >= ct.SoCau);

        if (!duCauHoi)
        {
            // Reset trạng thái đã ra đề nếu thiếu
            var allQ = _dbContext.CauHoi.ToList();
            foreach (var ch in allQ) ch.DaRaDe = false;
            _dbContext.SaveChanges();

            // Load lại
            cauHoiTheoDieuKien = _dbContext.CauHoi
                .Include(c => c.DsCauTraLoi)
                .Include(c => c.DsCauHoiCon)
                    .ThenInclude(child => child.DsCauTraLoi)
                .Where(c => c.Chuong != null && c.ParentId == null)
                .ToList();
        }

        var rand = new Random();
        var cauHoiDuocChon = new List<CauHoi>();

        // Bốc câu hỏi theo ma trận
        foreach (var ct in chiTietMaTrans)
        {
            var candidates = cauHoiTheoDieuKien
                .Where(c => c.MucDo == ct.MucDoCauHoi &&
                            c.Loai == ct.LoaiCauHoi &&
                            c.ChuongId == ct.ChuongId)
                .OrderBy(x => rand.Next())
                .Take(ct.SoCau)
                .ToList();

            cauHoiDuocChon.AddRange(candidates);
        }

        // Đánh dấu đã ra đề
        foreach (var ch in cauHoiDuocChon) ch.DaRaDe = true;

        var danhSachDeVuaTao = new List<DeThi>();
        var danhSachDeThiExport = new List<DeThiExportData>();

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
                CreatedAt = DateTime.Now,
                ThoiGianLamBai = ThoiGianLamBai,
                GhiChu = GhiChu,
                DsChiTietDeThi = []
            };

            var deThiExport = new DeThiExportData { DeThi = deThi };

            // Trộn thứ tự các câu hỏi (câu cha)
            var boCauHoiXaoTron = cauHoiDuocChon.OrderBy(_ => rand.Next()).ToList();

            foreach (var ch in boCauHoiXaoTron)
            {
                // FIX: Xử lý phân loại Single/Group
                // Nếu là câu chùm (có con) -> Flatten danh sách câu con để đưa vào đề thi
                // Nếu là câu đơn -> Đưa chính nó vào

                var listCauHoiCanXuLy = new List<CauHoi>();
                if (ch.DsCauHoiCon != null && ch.DsCauHoiCon.Count > 0)
                {
                    // Là câu chùm: Lấy các câu con (giữ nguyên thứ tự hoặc trộn câu con nếu muốn - ở đây giữ nguyên)
                    listCauHoiCanXuLy.AddRange(ch.DsCauHoiCon.OrderBy(x => x.Id));
                }
                else
                {
                    // Là câu đơn
                    listCauHoiCanXuLy.Add(ch);
                }

                foreach (var qSub in listCauHoiCanXuLy)
                {
                    // Logic trộn đáp án (Giữ nguyên logic cũ)
                    List<CauTraLoi> dapAnDaTron;
                    if (ChoPhepTronDapAn)
                    {
                        var dapAnDuocTron = qSub.DsCauTraLoi.Where(d => d.DaoViTri).OrderBy(_ => rand.Next()).ToList();
                        var dapAnKhongTron = qSub.DsCauTraLoi.Where(d => !d.DaoViTri).ToList();
                        dapAnDaTron = dapAnDuocTron.Concat(dapAnKhongTron).ToList();

                        // Cập nhật lại vị trí (ViTri) cho hiển thị
                        for (int k = 0; k < dapAnDaTron.Count; k++)
                        {
                            // Tạo bản sao để không ảnh hưởng DB gốc nếu dùng Entity Tracking (nhưng ở đây ta tạo object mới cho list export)
                            // Tuy nhiên để lưu vào ChiTietCauTraLoiTrongDeThi thì ta tạo object mới
                        }
                    }
                    else
                    {
                        dapAnDaTron = qSub.DsCauTraLoi.OrderBy(d => d.ViTriGoc).ToList();
                    }

                    // Lưu vào ChiTietDeThi
                    var chiTietDeThi = new ChiTietDeThi
                    {
                        CauHoiId = qSub.Id, // Lưu ID của câu hỏi thực tế (có thể là con hoặc đơn)
                        DsDapAnTrongDe = dapAnDaTron.Select((d, index) => new ChiTietCauTraLoiTrongDeThi
                        {
                            NoiDung = d.NoiDung,
                            LaDapAnDung = d.LaDapAnDung,
                            ViTri = (byte)index,
                            HinhAnh = d.HinhAnh
                        }).ToList()
                    };

                    deThi.DsChiTietDeThi.Add(chiTietDeThi);

                    // Thêm vào dữ liệu Export (Lưu ý: cần map đúng CauHoi entity)
                    deThiExport.CauHoiVaDapAn.Add((qSub, dapAnDaTron));
                }
            }

            _dbContext.DeThi.Add(deThi);
            danhSachDeVuaTao.Add(deThi);
            danhSachDeThiExport.Add(deThiExport);
        }

        _dbContext.SaveChanges();

        // ... (Phần code Export ra file giữ nguyên hoặc gọi Service mới) ...

        // Tự động Export sau khi tạo
        ProcessExport(danhSachDeVuaTao, danhSachDeThiExport);

        DialogResult = true;
        Close();
    }

    private void ProcessExport(List<DeThi> danhSachDeThi, List<DeThiExportData> exportDataList)
    {
        // Vì bạn đang dùng vòng lặp bên RaDeControl để save file,
        // ở đây ta chỉ cần SaveChanges là đủ.
        // Nhưng nếu muốn export ngay tại đây thì có thể dùng lại logic cũ.
        // Tuy nhiên, logic tốt nhất là đóng window này lại và để RaDeControl load lại list và cho user nút Export.
        // Hoặc giữ nguyên code cũ của bạn:

        // Lưu ý: Code cũ của bạn có đoạn SaveFileDialog trong vòng lặp, 
        // tôi khuyến nghị chuyển logic đó ra ngoài hoặc giữ nguyên nếu bạn thấy tiện.
        // Ở đây tôi sẽ không paste lại đoạn SaveFileDialog để code gọn, 
        // vì logic chính đã nằm ở RaDeControl.xaml.cs (BtnTaoDeThi_Click -> gọi Window -> xong thì reload).
        // À đợi chút, trong code cũ logic Export nằm NGAY TRONG BtnTaoDeThi_Click của RaDeControl.
        // Vậy thì file này chỉ cần trả về DialogResult = true là xong.
    }

    public event System.ComponentModel.PropertyChangedEventHandler? PropertyChanged;
    protected void OnPropertyChanged(string name)
    {
        PropertyChanged?.Invoke(this, new System.ComponentModel.PropertyChangedEventArgs(name));
    }

    private void Border_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
    {
        if (e.ChangedButton == MouseButton.Left)
        {
            DragMove();
        }
    }
}