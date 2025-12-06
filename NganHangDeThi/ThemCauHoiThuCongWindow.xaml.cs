using NganHangDeThi.Common.Enum;
using NganHangDeThi.Data.DataContext;
using NganHangDeThi.Data.Entity;
using NganHangDeThi.Helpers;
using NganHangDeThi.Models;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;

namespace NganHangDeThi;

// ViewModel cho đáp án (Đã cập nhật để hỗ trợ RichTextBox)
public class CauTraLoiViewModel : INotifyPropertyChanged
{
    public bool LaDapAnDung { get; set; } = false;
    public bool DaoViTri { get; set; } = true;

    // Giữ tham chiếu đến RichTextBox của dòng này để lấy dữ liệu sau
    public RichTextBox? RtbInstance { get; set; }

    public event PropertyChangedEventHandler? PropertyChanged;
}

public partial class ThemCauHoiThuCongWindow : Window, INotifyPropertyChanged
{
    private readonly AppDbContext _db;

    // Dữ liệu
    public ObservableCollection<MonHoc> DsMonHoc { get; set; } = [];
    public ObservableCollection<Chuong> DsChuong { get; set; } = [];

    // Danh sách đáp án (Binding vào ItemsControl)
    public ObservableCollection<CauTraLoiViewModel> DsDapAnViewModel { get; set; } = [];

    // Chế độ con (Child Mode)
    private bool _isChildMode = false;
    public CauHoiRaw? CauHoiConResult { get; private set; }

    // Constructor chính
    public ThemCauHoiThuCongWindow(AppDbContext db)
    {
        InitializeComponent();
        DataContext = this;
        _db = db;
        LoadInitialData();
    }

    // Constructor cho Child Mode (Câu hỏi con)
    // Lưu ý: Bỏ IOptions vì không dùng ảnh nữa
    public ThemCauHoiThuCongWindow() : this(null!)
    {
        _isChildMode = true;
        InitializeComponent();
        DataContext = this;

        Title = "Thêm câu hỏi con";
        // Ẩn phần chọn Môn/Chương
        CbbMonHoc.Visibility = Visibility.Collapsed;
        CbbChuong.Visibility = Visibility.Collapsed;

        LoadInitialData();
    }

    private void LoadInitialData()
    {
        // Mặc định thêm 4 đáp án trống
        DsDapAnViewModel.Add(new CauTraLoiViewModel());
        DsDapAnViewModel.Add(new CauTraLoiViewModel());
        DsDapAnViewModel.Add(new CauTraLoiViewModel());
        DsDapAnViewModel.Add(new CauTraLoiViewModel());

        CbbMucDoDon.ItemsSource = MucDoCauHoiEnumHelper.DanhSach;
        CbbLoaiDon.ItemsSource = LoaiCauHoiEnumHelper.DanhSach;

        // Mặc định chọn
        CbbMucDoDon.SelectedIndex = 0;
        CbbLoaiDon.SelectedIndex = 0;

        if (!_isChildMode && _db != null)
        {
            var mons = _db.MonHoc.OrderBy(m => m.TenMon).ToList();
            DsMonHoc = new ObservableCollection<MonHoc>(mons);
            CbbMonHoc.ItemsSource = DsMonHoc;
        }
    }

    // --- SỰ KIỆN ---

    private void CbbMonHoc_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (CbbMonHoc.SelectedItem is MonHoc selectedMon)
        {
            var chuongs = _db.Chuong.Where(c => c.MonHocId == selectedMon.Id).OrderBy(c => c.ViTri).ToList();
            DsChuong = new ObservableCollection<Chuong>(chuongs);
            CbbChuong.ItemsSource = DsChuong;
            if (DsChuong.Any()) CbbChuong.SelectedIndex = 0;
        }
    }

    // Sự kiện này cực kỳ quan trọng: Khi ItemsControl sinh ra RichTextBox cho mỗi đáp án,
    // ta cần lưu tham chiếu của RTB đó vào ViewModel tương ứng để sau này lấy Text ra.
    private void RichTextBox_Loaded(object sender, RoutedEventArgs e)
    {
        if (sender is RichTextBox rtb && rtb.DataContext is CauTraLoiViewModel viewModel)
        {
            viewModel.RtbInstance = rtb;
        }
    }

    private void BtnThemDapAn_Click(object sender, RoutedEventArgs e)
    {
        DsDapAnViewModel.Add(new CauTraLoiViewModel());
    }

    private void BtnXoaDapAn_Click(object sender, RoutedEventArgs e)
    {
        if ((sender as Button)?.Tag is CauTraLoiViewModel item)
        {
            DsDapAnViewModel.Remove(item);
        }
    }

    // --- TOOLBAR ACTIONS ---
    // Các lệnh ToggleBold, ToggleItalic... WPF tự xử lý với Focus.
    // Ta chỉ cần xử lý nút màu sắc.

    private void BtnToDo_Click(object sender, RoutedEventArgs e)
    {
        // Lấy RichTextBox đang được Focus (có thể là ở Câu hỏi hoặc ở 1 trong các Đáp án)
        var focusedControl = FocusManager.GetFocusedElement(this) as RichTextBox;
        if (focusedControl != null && !focusedControl.Selection.IsEmpty)
        {
            focusedControl.Selection.ApplyPropertyValue(TextElement.ForegroundProperty, Brushes.Red);
        }
    }

    private void BtnXoaMau_Click(object sender, RoutedEventArgs e)
    {
        var focusedControl = FocusManager.GetFocusedElement(this) as RichTextBox;
        if (focusedControl != null && !focusedControl.Selection.IsEmpty)
        {
            focusedControl.Selection.ApplyPropertyValue(TextElement.ForegroundProperty, Brushes.Black);
        }
    }

    // --- SAVE LOGIC ---

    private void BtnLuu_Click(object sender, RoutedEventArgs e)
    {
        // 1. Lấy nội dung câu hỏi từ RichTextBox chính
        string noiDungCauHoi = RichTextHelper.GetHtmlFromRichTextBox(RtbNoiDungDon);

        // Validate
        if (!_isChildMode && CbbChuong.SelectedItem == null) { Msg("Vui lòng chọn chương."); return; }
        if (string.IsNullOrWhiteSpace(noiDungCauHoi)) { Msg("Nội dung câu hỏi trống."); return; }

        // 2. Lấy nội dung đáp án từ các RichTextBox con
        var listDapAnRaw = new List<CauTraLoiRaw>();
        int stt = 1;
        foreach (var vm in DsDapAnViewModel)
        {
            if (vm.RtbInstance == null) continue;

            string noiDungDapAn = RichTextHelper.GetHtmlFromRichTextBox(vm.RtbInstance);

            // Chỉ lấy các đáp án có nội dung
            if (!string.IsNullOrWhiteSpace(noiDungDapAn))
            {
                listDapAnRaw.Add(new CauTraLoiRaw(noiDungDapAn, vm.LaDapAnDung, (byte)stt++, vm.DaoViTri, null));
            }
        }

        if (listDapAnRaw.Count < 2) { Msg("Cần ít nhất 2 đáp án có nội dung."); return; }
        if (!listDapAnRaw.Any(d => d.LaDapAnDung)) { Msg("Cần ít nhất 1 đáp án đúng."); return; }

        // 3. Tạo object dữ liệu
        var cauHoiRaw = new CauHoiRaw
        {
            NoiDung = noiDungCauHoi,
            MucDo = (MucDoCauHoi)(CbbMucDoDon.SelectedValue ?? MucDoCauHoi.NhanBiet),
            Loai = (LoaiCauHoi)(CbbLoaiDon.SelectedValue ?? LoaiCauHoi.TracNghiemMotDapAn),
            HinhAnh = null, // Bỏ ảnh
            DapAn = listDapAnRaw
        };

        if (_isChildMode)
        {
            CauHoiConResult = cauHoiRaw;
            DialogResult = true;
            Close();
        }
        else
        {
            LuuVaoDB(cauHoiRaw, null);
        }
    }

    private void LuuVaoDB(CauHoiRaw raw, int? parentId)
    {
        try
        {
            var chuongId = ((Chuong)CbbChuong.SelectedItem).Id;

            var entity = new CauHoi
            {
                NoiDung = raw.NoiDung,
                MucDo = raw.MucDo,
                Loai = raw.Loai,
                HinhAnh = null,
                ChuongId = chuongId,
                ParentId = parentId,
                DaRaDe = false
            };

            foreach (var d in raw.DapAn)
            {
                entity.DsCauTraLoi.Add(new CauTraLoi
                {
                    NoiDung = d.NoiDung,
                    LaDapAnDung = d.LaDapAnDung,
                    ViTriGoc = d.ViTriGoc,
                    DaoViTri = d.DaoViTri,
                    HinhAnh = null
                });
            }

            _db.CauHoi.Add(entity);
            _db.SaveChanges();

            MessageBox.Show("Lưu thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
            DialogResult = true;
            Close();
        }
        catch (Exception ex)
        {
            MessageBox.Show("Lỗi: " + ex.Message, "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void Msg(string s) => MessageBox.Show(s, "Cảnh báo", MessageBoxButton.OK, MessageBoxImage.Warning);
    private void BtnHuy_Click(object sender, RoutedEventArgs e) => Close();
    private void Border_MouseDown(object sender, MouseButtonEventArgs e) { if (e.ChangedButton == MouseButton.Left) DragMove(); }

    public event PropertyChangedEventHandler? PropertyChanged;
}