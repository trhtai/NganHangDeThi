using NganHangDeThi.Common.Enum;
using NganHangDeThi.Data.DataContext;
using NganHangDeThi.Data.Entity;
using NganHangDeThi.Helpers;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace NganHangDeThi;

public partial class ThemHoacSuaMaTranWindow : Window, INotifyPropertyChanged
{
    private readonly AppDbContext _db;
    public MaTran MaTran { get; private set; }
    public ObservableCollection<ChiTietMaTran> DsChiTietMaTran { get; set; } = new();

    public int TongSoCau => DsChiTietMaTran.Sum(x => x.SoCau);

    private List<MonHoc> _dsMon = new();
    private List<Chuong> _dsChuong = new();

    public ThemHoacSuaMaTranWindow(AppDbContext db)
    {
        InitializeComponent();
        DataContext = this;
        _db = db;

        DsChiTietMaTran.CollectionChanged += (_, _) => OnPropertyChanged(nameof(TongSoCau));

        LoadComboBox();
        MaTran = new();

        CbbMonHoc.SelectionChanged += (_, _) => LoadChuongTheoMon();
        CbbChuong.SelectionChanged += (_, _) => CapNhatThongTinBoLoc();
        CbbMucDo.SelectionChanged += (_, _) => CapNhatThongTinBoLoc();
        CbbLoai.SelectionChanged += (_, _) => CapNhatThongTinBoLoc();
    }

    public ThemHoacSuaMaTranWindow(AppDbContext db, MaTran existing) : this(db)
    {
        TxtTen.Text = existing.Name;
        MaTran = existing;

        foreach (var ct in _db.ChiTietMaTran
            .Where(x => x.MaTranId == MaTran.Id)
            .Select(x => new ChiTietMaTran
            {
                Id = x.Id,
                ChuongId = x.ChuongId,
                Chuong = x.Chuong,
                MucDoCauHoi = x.MucDoCauHoi,
                LoaiCauHoi = x.LoaiCauHoi,
                SoCau = x.SoCau
            }))
        {
            DsChiTietMaTran.Add(ct);
        }
    }

    private void LoadComboBox()
    {
        _dsMon = _db.MonHoc.OrderBy(m => m.TenMon).ToList();
        CbbMonHoc.ItemsSource = _dsMon;
        CbbMucDo.ItemsSource = MucDoCauHoiEnumHelper.DanhSach;
        CbbLoai.ItemsSource = LoaiCauHoiEnumHelper.DanhSach;
    }

    private void LoadChuongTheoMon()
    {
        if (CbbMonHoc.SelectedItem is not MonHoc mon) return;
        _dsChuong = _db.Chuong.Where(c => c.MonHocId == mon.Id).OrderBy(c => c.ViTri).ToList();
        CbbChuong.ItemsSource = _dsChuong;
        CapNhatThongTinBoLoc();
    }

    private void CapNhatThongTinBoLoc()
    {
        // 1. Chỉ truy vấn các câu hỏi GỐC (Câu cha / Parent)
        IQueryable<CauHoi> query = _db.CauHoi.Where(q => q.ParentId == null);

        // 2. Áp dụng các bộ lọc
        if (CbbChuong.SelectedItem is Chuong chuong)
            query = query.Where(q => q.ChuongId == chuong.Id);
        else if (CbbMonHoc.SelectedItem is MonHoc mon)
            query = query.Where(q => q.Chuong!.MonHocId == mon.Id);

        if (CbbMucDo.SelectedValue is MucDoCauHoi mucDo)
            query = query.Where(q => q.MucDo == mucDo);

        var selectedLoai = CbbLoai.SelectedValue as LoaiCauHoi?;
        if (selectedLoai.HasValue)
            query = query.Where(q => q.Loai == selectedLoai.Value);

        // 3. Xác định chế độ hiển thị dựa trên Loại câu hỏi được chọn
        bool cheDoHienThiChiTiet = selectedLoai.HasValue &&
                                   (selectedLoai == LoaiCauHoi.DienKhuyet || selectedLoai == LoaiCauHoi.CauChum);

        // Đếm tổng số bản ghi (Câu đơn + Bài chùm đều tính là 1)
        int tongSoBanGhi = query.Count();

        if (tongSoBanGhi == 0)
        {
            TxtThongTinBoLoc.Text = "Không tìm thấy câu hỏi nào phù hợp.";
            TxtThongTinBoLoc.Foreground = System.Windows.Media.Brushes.Red;
            TxtThongTinBoLoc.ToolTip = null;
            return;
        }

        if (cheDoHienThiChiTiet)
        {
            // Dạng chùm/điền khuyết: Cần đếm thêm số lượng câu con để User biết nhập
            int tongSoCauCon = query.SelectMany(q => q.DsCauHoiCon).Count();

            TxtThongTinBoLoc.Text = $"Tìm thấy: {tongSoBanGhi} bài (chứa tổng {tongSoCauCon} câu hỏi nhỏ).";
            TxtThongTinBoLoc.Foreground = System.Windows.Media.Brushes.Blue;
            TxtThongTinBoLoc.ToolTip = "Hãy nhập tổng số câu hỏi nhỏ (Items) vào ô 'Số câu'.";
        }
        else
        {
            // Dạng đơn hoặc Tất cả: Chỉ cần hiển thị tổng số lượng bản ghi
            TxtThongTinBoLoc.Text = $"Tổng số câu hỏi khả dụng: {tongSoBanGhi} câu.";
            TxtThongTinBoLoc.Foreground = System.Windows.Media.Brushes.DarkSlateGray;
            TxtThongTinBoLoc.ToolTip = null;
        }
    }

    private void BtnThemChiTiet_Click(object sender, RoutedEventArgs e)
    {
        if (CbbChuong.SelectedItem is not Chuong chuong) return;
        if (CbbMucDo.SelectedValue is not MucDoCauHoi mucDo) return;
        if (CbbLoai.SelectedValue is not LoaiCauHoi loai) return;
        if (!int.TryParse(TxtSoCau.Text, out int soCau) || soCau <= 0)
        {
            MessageBox.Show("Số câu không hợp lệ!");
            return;
        }

        bool daTonTai = DsChiTietMaTran.Any(x =>
            x.ChuongId == chuong.Id &&
            x.MucDoCauHoi == mucDo &&
            x.LoaiCauHoi == loai);

        if (daTonTai)
        {
            MessageBox.Show("Chi tiết ma trận này đã tồn tại!");
            return;
        }

        // --- BẮT ĐẦU SỬA ĐỔI ---
        int tongSoCauHoi;

        // Nếu là Điền khuyết hoặc Câu chùm -> Đếm tổng số câu con
        if (loai == LoaiCauHoi.DienKhuyet || loai == LoaiCauHoi.CauChum)
        {
            // Lấy danh sách các câu cha phù hợp
            var queryParents = _db.CauHoi.Where(q =>
                q.ChuongId == chuong.Id &&
                q.MucDo == mucDo &&
                q.Loai == loai &&
                q.ParentId == null); // Chỉ lấy câu cha

            // Tính tổng số lượng con (Sum)
            // Lưu ý: Có thể cần Include(x => x.DsCauHoiCon) nếu EF không tự load, 
            // nhưng query trực tiếp như sau thường ổn với EF Core:
            tongSoCauHoi = queryParents.SelectMany(p => p.DsCauHoiCon).Count();
        }
        else
        {
            // Logic cũ cho câu đơn: Trắc nghiệm, Tự luận...
            // Lưu ý: Phải loại trừ các câu là CON của câu chùm (để tránh đếm trùng nếu câu con cũng có loại này)
            // Hoặc đơn giản nhất theo logic hiện tại là đếm tất cả câu có loại này
            // Tuy nhiên, các câu con trong bài điền khuyết được lưu là TracNghiemMotDapAn.
            // Nếu bạn chọn TracNghiemMotDapAn ở đây, nó sẽ đếm cả câu lẻ và câu con trong bài điền khuyết.
            // Để an toàn và khớp với logic sinh đề (sinh đề lấy cả con), ta giữ nguyên logic đếm count thông thường.
            tongSoCauHoi = _db.CauHoi.Count(q =>
                q.ChuongId == chuong.Id &&
                q.MucDo == mucDo &&
                q.Loai == loai);
        }
        // --- KẾT THÚC SỬA ĐỔI ---

        if (soCau > tongSoCauHoi)
        {
            MessageBox.Show($"Chỉ có {tongSoCauHoi} câu phù hợp!", "Vượt số lượng", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        // ... (Phần code thêm vào DsChiTietMaTran giữ nguyên)
        DsChiTietMaTran.Add(new ChiTietMaTran
        {
            ChuongId = chuong.Id,
            Chuong = chuong,
            MucDoCauHoi = mucDo,
            LoaiCauHoi = loai,
            SoCau = soCau
        });

        OnPropertyChanged(nameof(TongSoCau));
    }

    private void BtnXoaChiTiet_Click(object sender, RoutedEventArgs e)
    {
        if ((sender as Button)?.Tag is ChiTietMaTran ct)
        {
            DsChiTietMaTran.Remove(ct);
            OnPropertyChanged(nameof(TongSoCau));
        }
    }

    private void BtnLuu_Click(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(TxtTen.Text))
        {
            MessageBox.Show("Vui lòng nhập tên ma trận.");
            return;
        }

        MaTran.Name = TxtTen.Text.Trim();
        MaTran.ThoiGianCapNhatGanNhat = DateTime.Now;

        if (MaTran.Id == 0)
        {
            MaTran.CreatedAt = DateTime.Now;
            MaTran.DsChiTietMaTran = DsChiTietMaTran.ToList();
        }

        DialogResult = true;
        Close();
    }

    private void BtnHuy_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
        Close();
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    protected void OnPropertyChanged(string name) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

    private void Border_MouseDown(object sender, MouseButtonEventArgs e)
    {
        if (e.ChangedButton == MouseButton.Left)
        {
            DragMove();
        }
    }
}
