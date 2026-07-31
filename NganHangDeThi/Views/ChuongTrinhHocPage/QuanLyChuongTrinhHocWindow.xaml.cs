using NganHangDeThi.Data.Entities;
using NganHangDeThi.ViewModels.Curriculum.Factories.Interfaces;
using System.Windows;

namespace NganHangDeThi.Views.ChuongTrinhHocPage;

public partial class QuanLyChuongTrinhHocWindow : Window
{
    public QuanLyChuongTrinhHocWindow(ChuongTrinhHocView view, Lop lop)
    {
        InitializeComponent();
        Title = $"Quản lý môn học - Lớp {lop.MaLop}";
        RootGrid.Children.Add(view);
    }

    /// <summary>
    /// Mở cửa sổ quản lý môn học (không modal, có thể mở nhiều lớp cùng lúc) cho 1 lớp cụ thể.
    /// </summary>
    public static void Show(Window? owner, ICurriculumViewModelFactory curriculumViewModelFactory, Lop lop)
    {
        var vm = curriculumViewModelFactory.Create(lop);
        var view = new ChuongTrinhHocView(vm);

        var window = new QuanLyChuongTrinhHocWindow(view, lop)
        {
            Owner = owner ?? Application.Current.MainWindow
        };
        window.Show();
    }
}