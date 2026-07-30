using NganHangDeThi.Data.Entities;
using NganHangDeThi.ViewModels.Semesters.Factories.Interfaces;
using System.Windows;

namespace NganHangDeThi.Views.HocKyPage;

public partial class QuanLyHocKyWindow : Window
{
    public QuanLyHocKyWindow(HocKyView hocKyView, NienKhoa nienKhoa)
    {
        InitializeComponent();
        Title = $"Quản lý học kỳ - {nienKhoa.TenNienKhoa}";
        RootGrid.Children.Add(hocKyView);
    }

    /// <summary>Mở cửa sổ quản lý học kỳ (không modal, có thể mở nhiều niên khóa cùng lúc)
    /// cho 1 niên khóa cụ thể.</summary>
    public static void Show(Window? owner, ISemesterViewModelFactory semesterViewModelFactory, NienKhoa nienKhoa)
    {
        var vm = semesterViewModelFactory.Create(nienKhoa);
        var view = new HocKyView(vm);

        var window = new QuanLyHocKyWindow(view, nienKhoa)
        {
            Owner = owner ?? Application.Current.MainWindow
        };
        window.Show();
    }
}