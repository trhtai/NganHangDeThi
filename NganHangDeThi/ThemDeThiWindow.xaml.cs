using NganHangDeThi.Data.DataContext;
using NganHangDeThi.Data.Entity;
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

        DialogResult = true;
        Close();
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
