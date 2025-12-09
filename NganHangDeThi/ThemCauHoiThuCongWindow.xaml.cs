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

#region ViewModels
// ViewModel cho từng dòng đáp án Trắc nghiệm
public class DapAnViewModel : INotifyPropertyChanged
{
    public bool LaDapAnDung { get; set; } = false;
    public bool DaoViTri { get; set; } = true;

    // Giữ tham chiếu tới RichTextBox của dòng này để lấy dữ liệu
    public RichTextBox? RtbControl { get; set; }

    public event PropertyChangedEventHandler? PropertyChanged;
}
#endregion

public partial class ThemCauHoiThuCongWindow : Window, INotifyPropertyChanged
{
    private readonly AppDbContext? _db; // Nullable để hỗ trợ chế độ ChildMode không cần DB

    // --- DỮ LIỆU METADATA ---
    public ObservableCollection<MonHoc> DsMonHoc { get; set; } = [];
    public ObservableCollection<Chuong> DsChuong { get; set; } = [];

    // --- DỮ LIỆU CÁC TAB ---
    // Tab 1: Trắc nghiệm
    public ObservableCollection<DapAnViewModel> DsDapAnTN { get; set; } = [];
    private bool _isOneChoice = true;
    public bool IsOneChoice
    {
        get => _isOneChoice;
        set { _isOneChoice = value; OnPropertyChanged(nameof(IsOneChoice)); }
    }
    public bool IsMultiChoice
    {
        get => !_isOneChoice;
        set { _isOneChoice = !value; OnPropertyChanged(nameof(IsMultiChoice)); }
    }

    // Tab: Câu chùm (Đã loại bỏ Tab Điền khuyết)
    public ObservableCollection<CauHoiRaw> DsCauHoiCon { get; set; } = [];

    // --- CHẾ ĐỘ CON (CHILD MODE) ---
    private bool _isChildMode = false;
    public CauHoiRaw? CauHoiConResult { get; private set; }

    // ============================================================
    // CONSTRUCTOR
    // ============================================================

    // Constructor chính (Dùng cho cửa sổ cha)
    public ThemCauHoiThuCongWindow(AppDbContext db)
    {
        InitializeComponent();
        DataContext = this;
        _db = db;
        LoadInitialData();
    }

    // Constructor phụ (Dùng cho cửa sổ con - Thêm câu hỏi nhỏ cho câu chùm)
    public ThemCauHoiThuCongWindow()
    {
        InitializeComponent();
        DataContext = this;
        _isChildMode = true;

        Title = "Thêm câu hỏi con";

        // Ẩn các phần không cần thiết ở chế độ con
        CbbMonHoc.Visibility = Visibility.Collapsed;
        CbbChuong.Visibility = Visibility.Collapsed;

        // Ẩn Tab Câu chùm (Không cho tạo chùm lồng chùm để tránh phức tạp)
        // Vì đã xóa tab Điền khuyết, thứ tự tab thay đổi: 0: Trắc nghiệm, 1: Tự luận, 2: Câu chùm
        if (TabControl.Items.Count > 2)
        {
            ((TabItem)TabControl.Items[2]).Visibility = Visibility.Collapsed;
        }

        LoadInitialData();
    }

    private void LoadInitialData()
    {
        // 1. Load Metadata (Chỉ load khi có DB - chế độ cha)
        if (!_isChildMode && _db != null)
        {
            var mons = _db.MonHoc.OrderBy(m => m.TenMon).ToList();
            DsMonHoc = new ObservableCollection<MonHoc>(mons);
            CbbMonHoc.ItemsSource = DsMonHoc;
        }

        CbbMucDo.ItemsSource = MucDoCauHoiEnumHelper.DanhSach;
        CbbMucDo.SelectedIndex = 0; // Mặc định Nhận biết

        // 2. Khởi tạo dữ liệu mẫu cho các Tab

        // Tab Trắc nghiệm: 4 đáp án mặc định
        DsDapAnTN.Clear();
        for (int i = 0; i < 4; i++) DsDapAnTN.Add(new DapAnViewModel());
    }

    // ============================================================
    // XỬ LÝ SỰ KIỆN (EVENTS)
    // ============================================================

    private void CbbMonHoc_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (_db != null && CbbMonHoc.SelectedItem is MonHoc selectedMon)
        {
            var chuongs = _db.Chuong.Where(c => c.MonHocId == selectedMon.Id).OrderBy(c => c.ViTri).ToList();
            DsChuong = new ObservableCollection<Chuong>(chuongs);
            CbbChuong.ItemsSource = DsChuong;
            if (DsChuong.Any()) CbbChuong.SelectedIndex = 0;
        }
    }

    // Sự kiện quan trọng: Gán RichTextBox vào ViewModel khi giao diện được vẽ
    private void RichTextBox_Loaded(object sender, RoutedEventArgs e)
    {
        if (sender is RichTextBox rtb)
        {
            if (rtb.Tag is DapAnViewModel vmTN)
            {
                vmTN.RtbControl = rtb;
            }
        }
    }

    // --- Toolbar Actions ---
    private void BtnToDo_Click(object sender, RoutedEventArgs e)
    {
        var rtb = GetFocusedRichTextBox();
        if (rtb != null && !rtb.Selection.IsEmpty)
        {
            rtb.Selection.ApplyPropertyValue(TextElement.ForegroundProperty, Brushes.Red);
        }
    }

    private void BtnXoaMau_Click(object sender, RoutedEventArgs e)
    {
        var rtb = GetFocusedRichTextBox();
        if (rtb != null && !rtb.Selection.IsEmpty)
        {
            rtb.Selection.ApplyPropertyValue(TextElement.ForegroundProperty, Brushes.Black);
        }
    }

    private RichTextBox? GetFocusedRichTextBox()
    {
        var element = FocusManager.GetFocusedElement(this) as DependencyObject;
        while (element != null)
        {
            if (element is RichTextBox rtb) return rtb;
            element = VisualTreeHelper.GetParent(element);
        }
        return null;
    }

    // --- Tab Trắc Nghiệm ---
    private void BtnThemDapAnTN_Click(object sender, RoutedEventArgs e)
    {
        DsDapAnTN.Add(new DapAnViewModel());
    }

    private void BtnXoaDapAnTN_Click(object sender, RoutedEventArgs e)
    {
        if ((sender as Button)?.Tag is DapAnViewModel item)
        {
            DsDapAnTN.Remove(item);
        }
    }

    // --- Tab Câu Chùm ---
    private void BtnThemCauCon_Click(object sender, RoutedEventArgs e)
    {
        // Mở chính cửa sổ này ở chế độ ChildMode
        var childWindow = new ThemCauHoiThuCongWindow();

        if (childWindow.ShowDialog() == true && childWindow.CauHoiConResult != null)
        {
            DsCauHoiCon.Add(childWindow.CauHoiConResult);
        }
    }

    private void BtnXoaCauCon_Click(object sender, RoutedEventArgs e)
    {
        if ((sender as Button)?.DataContext is CauHoiRaw item)
        {
            DsCauHoiCon.Remove(item);
        }
    }

    private void BtnHuy_Click(object sender, RoutedEventArgs e) => Close();
    private void Border_MouseDown(object sender, MouseButtonEventArgs e) { if (e.ChangedButton == MouseButton.Left) DragMove(); }


    // ============================================================
    // LOGIC LƯU (SAVE)
    // ============================================================

    private void BtnLuu_Click(object sender, RoutedEventArgs e)
    {
        // 0. Validate chung (Chỉ check khi ở chế độ cha)
        if (!_isChildMode)
        {
            if (CbbMonHoc.SelectedItem == null) { Msg("Vui lòng chọn Môn học."); return; }
            if (CbbChuong.SelectedItem == null) { Msg("Vui lòng chọn Chương."); return; }
        }
        if (CbbMucDo.SelectedItem == null) { Msg("Vui lòng chọn Mức độ."); return; }

        // 1. Điều hướng xử lý theo Tab
        switch (TabControl.SelectedIndex)
        {
            case 0: LuuTabTracNghiem(); break;
            case 1: LuuTabTuLuan(); break;
            case 2: LuuTabCauChum(); break; // Index đã thay đổi vì xóa tab Điền khuyết
        }
    }

    private void LuuTabTracNghiem()
    {
        string noiDungCH = RichTextHelper.GetHtmlFromRichTextBox(RtbCauHoiTN);
        if (string.IsNullOrWhiteSpace(noiDungCH)) { Msg("Nội dung câu hỏi không được để trống."); return; }

        var listDapAnClean = new List<CauTraLoiRaw>();
        int stt = 1;
        foreach (var vm in DsDapAnTN)
        {
            if (vm.RtbControl == null) continue;
            string noiDungDA = RichTextHelper.GetHtmlFromRichTextBox(vm.RtbControl);
            if (!string.IsNullOrWhiteSpace(noiDungDA))
            {
                listDapAnClean.Add(new CauTraLoiRaw(noiDungDA, vm.LaDapAnDung, (byte)stt++, vm.DaoViTri, null));
            }
        }

        if (listDapAnClean.Count < 2) { Msg("Câu hỏi trắc nghiệm cần ít nhất 2 đáp án."); return; }

        int soDapAnDung = listDapAnClean.Count(x => x.LaDapAnDung);
        if (soDapAnDung == 0) { Msg("Chưa chọn đáp án đúng nào."); return; }
        if (IsOneChoice && soDapAnDung > 1) { Msg("Loại '1 Đáp án đúng' nhưng bạn đang chọn nhiều đáp án đúng."); return; }

        var loai = IsOneChoice ? LoaiCauHoi.TracNghiemMotDapAn : LoaiCauHoi.TracNghiemNhieuDapAn;

        ProcessSave(new CauHoiRaw
        {
            NoiDung = noiDungCH,
            MucDo = (MucDoCauHoi)CbbMucDo.SelectedValue,
            Loai = loai,
            DapAn = listDapAnClean
        });
    }

    private void LuuTabTuLuan()
    {
        string noiDungCH = RichTextHelper.GetHtmlFromRichTextBox(RtbCauHoiTL);
        string noiDungDA = RichTextHelper.GetHtmlFromRichTextBox(RtbDapAnTL);

        if (string.IsNullOrWhiteSpace(noiDungCH)) { Msg("Nội dung câu hỏi không được để trống."); return; }

        var listDapAn = new List<CauTraLoiRaw>();
        if (!string.IsNullOrWhiteSpace(noiDungDA))
        {
            // Tự luận chỉ có 1 đáp án là hướng dẫn chấm, mặc định đúng
            listDapAn.Add(new CauTraLoiRaw(noiDungDA, true, 1, false, null));
        }

        ProcessSave(new CauHoiRaw
        {
            NoiDung = noiDungCH,
            MucDo = (MucDoCauHoi)CbbMucDo.SelectedValue,
            Loai = LoaiCauHoi.TuLuan,
            DapAn = listDapAn
        });
    }

    private void LuuTabCauChum()
    {
        string noiDungDoanVan = RichTextHelper.GetHtmlFromRichTextBox(RtbCauHoiChum);
        if (string.IsNullOrWhiteSpace(noiDungDoanVan)) { Msg("Nội dung đoạn văn không được để trống."); return; }

        if (DsCauHoiCon.Count == 0) { Msg("Câu chùm cần ít nhất 1 câu hỏi con."); return; }

        // Loại của câu cha sẽ phụ thuộc vào câu con đầu tiên (tạm thời) hoặc loại riêng
        // Ở đây ta gán tạm là ChumTracNghiemMotDapAn, logic xử lý đề thi sẽ lo việc render
        ProcessSave(new CauHoiRaw
        {
            NoiDung = noiDungDoanVan,
            MucDo = (MucDoCauHoi)CbbMucDo.SelectedValue,
            Loai = LoaiCauHoi.ChumTracNghiemMotDapAn,
            CauHoiCon = DsCauHoiCon.ToList()
        });
    }

    // Hàm xử lý lưu chung (Phân biệt Child Mode và Parent Mode)
    private void ProcessSave(CauHoiRaw raw)
    {
        if (_isChildMode)
        {
            // Chế độ con: Trả về kết quả cho cửa sổ cha
            CauHoiConResult = raw;
            DialogResult = true;
            Close();
        }
        else
        {
            // Chế độ cha: Lưu vào CSDL
            LuuVaoDB(raw);
        }
    }

    private void LuuVaoDB(CauHoiRaw raw)
    {
        if (_db == null) return;

        try
        {
            var chuongId = ((Chuong)CbbChuong.SelectedItem).Id;

            // Hàm đệ quy lưu entity
            void SaveEntityRecursive(CauHoiRaw qRaw, int? parentId)
            {
                var entity = new CauHoi
                {
                    NoiDung = qRaw.NoiDung,
                    MucDo = qRaw.MucDo,
                    Loai = qRaw.Loai,
                    ChuongId = chuongId,
                    ParentId = parentId,
                    DaRaDe = false,
                    HinhAnh = null
                };

                foreach (var d in qRaw.DapAn)
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
                _db.SaveChanges(); // Save để lấy ID cho con

                if (qRaw.CauHoiCon != null && qRaw.CauHoiCon.Any())
                {
                    foreach (var child in qRaw.CauHoiCon)
                    {
                        SaveEntityRecursive(child, entity.Id);
                    }
                }
            }

            SaveEntityRecursive(raw, null);

            MessageBox.Show("Lưu thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);

            // Reset Form (Tùy chọn)
            ResetForm();
        }
        catch (Exception ex)
        {
            Msg("Lỗi khi lưu: " + ex.Message);
        }
    }

    private void ResetForm()
    {
        // Reset cơ bản
        RtbCauHoiTN.Document.Blocks.Clear();
        RtbCauHoiTL.Document.Blocks.Clear();
        RtbDapAnTL.Document.Blocks.Clear();
        RtbCauHoiChum.Document.Blocks.Clear();

        DsDapAnTN.Clear();
        for (int i = 0; i < 4; i++) DsDapAnTN.Add(new DapAnViewModel());

        DsCauHoiCon.Clear();
    }

    private void Msg(string s) => MessageBox.Show(s, "Cảnh báo", MessageBoxButton.OK, MessageBoxImage.Warning);

    public event PropertyChangedEventHandler? PropertyChanged;
    protected void OnPropertyChanged(string name) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}