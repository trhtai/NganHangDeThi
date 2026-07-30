using NganHangDeThi.Data.Entities;
using NganHangDeThi.ViewModels.Chapters.Factories.Interfaces;
using System.Windows;

namespace NganHangDeThi.Views.ChuongPage;

public partial class QuanLyChuongWindow : Window
{
    public QuanLyChuongWindow(ChuongView chuongView, MonHoc monHoc)
    {
        InitializeComponent();
        Title = $"Quản lý chương - {monHoc.TenMon}";
        RootGrid.Children.Add(chuongView);
    }

    /// <summary>
    /// Mở cửa sổ quản lý chương (không modal, có thể mở nhiều môn cùng lúc)
    /// cho 1 môn học cụ thể.
    /// </summary>
    public static void Show(Window? owner, IChapterViewModelFactory chapterViewModelFactory, MonHoc monHoc)
    {
        var vm = chapterViewModelFactory.Create(monHoc);
        var view = new ChuongView(vm);

        var window = new QuanLyChuongWindow(view, monHoc)
        {
            Owner = owner ?? Application.Current.MainWindow
        };
        window.Show();
    }
}