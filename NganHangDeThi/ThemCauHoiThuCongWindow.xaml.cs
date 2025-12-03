using Microsoft.Win32;
using Microsoft.Extensions.Options;
using NganHangDeThi.Common.Configs;
using NganHangDeThi.Common.Enum;
using NganHangDeThi.Data.DataContext;
using NganHangDeThi.Data.Entity;
using NganHangDeThi.Helpers;
using NganHangDeThi.Models; // Chứa class CauHoiRaw nếu cần map
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;

namespace NganHangDeThi;

// Đặt class này bên dưới class Window hoặc trong file Models
//public class CauTraLoiViewModel : INotifyPropertyChanged
//{
//    public string NoiDung { get; set; } = "";
//    public bool LaDapAnDung { get; set; } = false;
//    public bool DaoViTri { get; set; } = true;
//    public string? HinhAnh { get; set; } // Đường dẫn ảnh tạm

//    // Sự kiện PropertyChanged để DataGrid cập nhật
//    public event PropertyChangedEventHandler? PropertyChanged;
//}

public partial class ThemCauHoiThuCongWindow : Window, INotifyPropertyChanged
{
    private readonly AppDbContext _db;
    private readonly string _imageBasePath;

    // Dữ liệu cho ComboBox
    public ObservableCollection<MonHoc> DsMonHoc { get; set; } = [];
    public ObservableCollection<Chuong> DsChuong { get; set; } = [];

    // Dữ liệu cho Grid (Câu đơn)
    public ObservableCollection<CauTraLoiViewModel> DsDapAnDon { get; set; } = [];

    // Dữ liệu cho Grid (Câu chùm)
    public ObservableCollection<CauHoiRaw> DsCauHoiCon { get; set; } = [];

    // Biến lưu ảnh tạm
    private string? _anhCauHoiDon;
    private string? _anhCauHoiChum;

    // CHẾ ĐỘ NHẬP CÂU CON
    private bool _isChildMode = false;
    public CauHoiRaw? CauHoiConResult { get; private set; } // Kết quả trả về khi ở chế độ con

    // Constructor chính
    public ThemCauHoiThuCongWindow(AppDbContext db, IOptions<ImageStorageOptions> options)
    {
        InitializeComponent();
        DataContext = this;
        _db = db;
        _imageBasePath = options.Value.FolderPath;

        LoadInitialData();
        DsDapAnDon.CollectionChanged += (s, e) => { /* Logic update nếu cần */ };
    }

    // Constructor dành cho chế độ nhập Câu hỏi con
    public ThemCauHoiThuCongWindow(IOptions<ImageStorageOptions> options) : this(null!, options)
    {
        _isChildMode = true;
        // Ẩn các phần không cần thiết của Parent
        Title = "Thêm câu hỏi con";
        CbbMonHoc.Visibility = Visibility.Collapsed;
        CbbChuong.Visibility = Visibility.Collapsed;

        // Ẩn luôn Tab Câu chùm, chỉ để lại Tab Câu đơn
        // (Cách nhanh: Select Tab 0 và ẩn Header TabControl)
        // Nhưng ở đây ta cứ để mặc định Tab 0
        ((TabItem)TabControl.Items[1]).Visibility = Visibility.Collapsed;
    }

    private void LoadInitialData()
    {
        if (_isChildMode)
        {
            // Load Enum
            CbbMucDoDon.ItemsSource = MucDoCauHoiEnumHelper.DanhSach;
            CbbLoaiDon.ItemsSource = LoaiCauHoiEnumHelper.DanhSach;

            // Grid binding
            DgDapAn.ItemsSource = DsDapAnDon;
            return;
        }

        // Load Môn học
        var mons = _db.MonHoc.OrderBy(m => m.TenMon).ToList();
        DsMonHoc = new ObservableCollection<MonHoc>(mons);
        CbbMonHoc.ItemsSource = DsMonHoc;

        // Load Enum
        CbbMucDoDon.ItemsSource = MucDoCauHoiEnumHelper.DanhSach;
        CbbLoaiDon.ItemsSource = LoaiCauHoiEnumHelper.DanhSach;

        // Grid binding
        DgDapAn.ItemsSource = DsDapAnDon;
        DgCauHoiCon.ItemsSource = DsCauHoiCon;
    }

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

    // --- LOGIC XỬ LÝ ẢNH ---
    private string? ChonAnhVaLuuTam()
    {
        var openDialog = new OpenFileDialog { Filter = "Image Files|*.jpg;*.jpeg;*.png;*.bmp" };
        if (openDialog.ShowDialog() == true)
        {
            string sourceFile = openDialog.FileName;
            string fileName = $"{Guid.NewGuid()}{Path.GetExtension(sourceFile)}";
            string destPath = Path.Combine(_imageBasePath, fileName);

            // Tạo thư mục nếu chưa có
            Directory.CreateDirectory(_imageBasePath);
            File.Copy(sourceFile, destPath);

            return fileName; // Trả về tên file để lưu DB
        }
        return null;
    }

    private void BtnChonAnhCauHoi_Click(object sender, RoutedEventArgs e)
    {
        var f = ChonAnhVaLuuTam();
        if (f != null)
        {
            _anhCauHoiDon = f;
            ImgCauHoiDon.Source = new BitmapImage(new Uri(Path.Combine(_imageBasePath, f)));
            ImgCauHoiDon.Visibility = Visibility.Visible;
        }
    }

    private void BtnChonAnhChum_Click(object sender, RoutedEventArgs e)
    {
        var f = ChonAnhVaLuuTam();
        if (f != null)
        {
            _anhCauHoiChum = f;
            ImgCauHoiChum.Source = new BitmapImage(new Uri(Path.Combine(_imageBasePath, f)));
            ImgCauHoiChum.Visibility = Visibility.Visible;
        }
    }

    // --- LOGIC TAB CÂU ĐƠN ---
    private void BtnThemDapAn_Click(object sender, RoutedEventArgs e)
    {
        DsDapAnDon.Add(new CauTraLoiViewModel { NoiDung = "", LaDapAnDung = false, DaoViTri = true });
    }

    private void BtnXoaDapAn_Click(object sender, RoutedEventArgs e)
    {
        if ((sender as Button)?.DataContext is CauTraLoiViewModel item)
        {
            DsDapAnDon.Remove(item);
        }
    }

    // --- LOGIC TAB CÂU CHÙM ---
    private void BtnThemCauCon_Click(object sender, RoutedEventArgs e)
    {
        // Mở chính cửa sổ này nhưng ở chế độ ChildMode
        // Lưu ý: Cần inject IOptions<ImageStorageOptions> từ App.xaml.cs hoặc truyền tay
        // Để đơn giản, giả sử bạn truyền tay path hoặc resolve service
        // Ở đây tôi demo cách truyền tay path
        var options = new ImageStorageOptions { FolderPath = _imageBasePath };
        var iOptions = Options.Create(options);

        var childWindow = new ThemCauHoiThuCongWindow(iOptions);
        if (childWindow.ShowDialog() == true && childWindow.CauHoiConResult != null)
        {
            DsCauHoiCon.Add(childWindow.CauHoiConResult);
        }
    }

    private void BtnXoaCauHoiCon_Click(object sender, RoutedEventArgs e)
    {
        if ((sender as Button)?.DataContext is CauHoiRaw item)
        {
            DsCauHoiCon.Remove(item);
        }
    }

    // --- LOGIC LƯU DỮ LIỆU ---
    private void BtnLuu_Click(object sender, RoutedEventArgs e)
    {
        // 1. Kiểm tra Tab đang chọn
        if (TabControl.SelectedIndex == 0) // CÂU ĐƠN
        {
            SaveCauDon();
        }
        else // CÂU CHÙM
        {
            SaveCauChum();
        }
    }

    private void SaveCauDon()
    {
        // Validate
        if (!_isChildMode && (CbbChuong.SelectedItem == null)) { Msg("Vui lòng chọn chương."); return; }
        if (string.IsNullOrWhiteSpace(TxtNoiDungDon.Text)) { Msg("Nội dung câu hỏi trống."); return; }
        if (DsDapAnDon.Count < 2) { Msg("Cần ít nhất 2 đáp án."); return; }
        if (!DsDapAnDon.Any(d => d.LaDapAnDung)) { Msg("Cần ít nhất 1 đáp án đúng."); return; }

        var cauHoi = new CauHoiRaw
        {
            NoiDung = TxtNoiDungDon.Text,
            MucDo = (MucDoCauHoi)(CbbMucDoDon.SelectedValue ?? MucDoCauHoi.NhanBiet),
            Loai = (LoaiCauHoi)(CbbLoaiDon.SelectedValue ?? LoaiCauHoi.TracNghiemMotDapAn),
            HinhAnh = _anhCauHoiDon,
            DapAn = DsDapAnDon.Select((d, i) => new CauTraLoiRaw(d.NoiDung, d.LaDapAnDung, (byte)(i + 1), d.DaoViTri, d.HinhAnh)).ToList()
        };

        if (_isChildMode)
        {
            CauHoiConResult = cauHoi;
            DialogResult = true;
            Close();
        }
        else
        {
            // Lưu vào DB
            LuuVaoDB(cauHoi, null);
        }
    }

    private void SaveCauChum()
    {
        if (CbbChuong.SelectedItem == null) { Msg("Vui lòng chọn chương."); return; }
        if (string.IsNullOrWhiteSpace(TxtNoiDungChum.Text)) { Msg("Nội dung đoạn văn trống."); return; }
        if (DsCauHoiCon.Count == 0) { Msg("Cần ít nhất 1 câu hỏi con."); return; }

        // Tạo câu cha (Parent)
        var parent = new CauHoiRaw
        {
            NoiDung = TxtNoiDungChum.Text,
            MucDo = MucDoCauHoi.ThongHieu, // Mặc định hoặc tính max
            Loai = LoaiCauHoi.ChumTracNghiemMotDapAn, // Tạm định danh
            HinhAnh = _anhCauHoiChum,
            CauHoiCon = DsCauHoiCon.ToList()
        };

        LuuVaoDB(parent, null);
    }

    private void LuuVaoDB(CauHoiRaw raw, int? parentId)
    {
        try
        {
            var chuongId = ((Chuong)CbbChuong.SelectedItem).Id;

            // Dùng QuestionExtractorService hoặc tự map thủ công
            // Ở đây map thủ công đơn giản
            var entity = new CauHoi
            {
                NoiDung = raw.NoiDung,
                MucDo = raw.MucDo,
                Loai = raw.Loai,
                HinhAnh = raw.HinhAnh,
                ChuongId = chuongId,
                ParentId = parentId,
                DaRaDe = false
            };

            // Map đáp án
            foreach (var d in raw.DapAn)
            {
                entity.DsCauTraLoi.Add(new CauTraLoi
                {
                    NoiDung = d.NoiDung,
                    LaDapAnDung = d.LaDapAnDung,
                    ViTriGoc = d.ViTriGoc,
                    DaoViTri = d.DaoViTri,
                    HinhAnh = d.HinhAnh
                });
            }

            _db.CauHoi.Add(entity);
            _db.SaveChanges(); // Save để lấy Id cho con

            // Lưu con (đệ quy 1 cấp)
            if (raw.CauHoiCon.Any())
            {
                foreach (var child in raw.CauHoiCon)
                {
                    // Update Loai cho Parent nếu chưa chuẩn (Optional)
                    // ...

                    // Gọi đệ quy nhưng truyền parentId thật
                    // Lưu ý: Cần refactor hàm này để nhận entity cha hoặc set ParentId thủ công
                    var childEntity = new CauHoi
                    {
                        NoiDung = child.NoiDung,
                        MucDo = child.MucDo,
                        Loai = child.Loai,
                        HinhAnh = child.HinhAnh,
                        ChuongId = chuongId,
                        ParentId = entity.Id // Link với cha
                    };

                    foreach (var d in child.DapAn)
                    {
                        childEntity.DsCauTraLoi.Add(new CauTraLoi
                        {
                            NoiDung = d.NoiDung,
                            LaDapAnDung = d.LaDapAnDung,
                            ViTriGoc = d.ViTriGoc,
                            DaoViTri = d.DaoViTri,
                            HinhAnh = d.HinhAnh
                        });
                    }
                    _db.CauHoi.Add(childEntity);
                }
                _db.SaveChanges();
            }

            MessageBox.Show("Lưu thành công!", "Thông báo");
            DialogResult = true;
            Close();
        }
        catch (Exception ex)
        {
            MessageBox.Show("Lỗi: " + ex.Message);
        }
    }

    private void Msg(string s) => MessageBox.Show(s, "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);

    private void BtnHuy_Click(object sender, RoutedEventArgs e) => Close();
    private void Border_MouseDown(object sender, MouseButtonEventArgs e) { if (e.ChangedButton == MouseButton.Left) DragMove(); }

    public event PropertyChangedEventHandler? PropertyChanged;
}

// Class ViewModel hỗ trợ binding Grid Đáp án
public class CauTraLoiViewModel : INotifyPropertyChanged
{
    public string NoiDung { get; set; } = "";
    public bool LaDapAnDung { get; set; } = false;
    public bool DaoViTri { get; set; } = true;
    public string? HinhAnh { get; set; }
    public event PropertyChangedEventHandler? PropertyChanged;
}