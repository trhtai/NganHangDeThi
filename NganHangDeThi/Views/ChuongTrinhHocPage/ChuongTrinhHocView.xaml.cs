using NganHangDeThi.Data.Repositories.Enums;
using NganHangDeThi.ViewModels.Curriculum;
using System.Windows;
using System.Windows.Controls;

namespace NganHangDeThi.Views.ChuongTrinhHocPage;

public partial class ChuongTrinhHocView : UserControl
{
    public ChuongTrinhHocView(CurriculumViewModel curriculumViewModel)
    {
        InitializeComponent();

        DataContextChanged += (_, e) =>
        {
            if (e.NewValue is CurriculumViewModel vm)
            {
                vm.EditDialogHost = (CurriculumEditViewModel editVm) =>
                {
                    var owner = Window.GetWindow(this);
                    var result = owner is null
                        ? ChinhSuaChuongTrinhHocDialog.ShowDialog(Application.Current.MainWindow, editVm)
                        : ChinhSuaChuongTrinhHocDialog.ShowDialog(owner, editVm);

                    return Task.FromResult(result == true);
                };
            }
        };

        DataContext = curriculumViewModel;

        Loaded += async (_, _) =>
        {
            if (DataContext is CurriculumViewModel vm && vm.Items.Count == 0)
            {
                await vm.LoadCommand.ExecuteAsync(null);
            }
        };
    }

    private CurriculumViewModel ViewModel => (CurriculumViewModel)DataContext;

    // Server-side sort: chặn WPF tự sort trên client (vì Items chỉ chứa 1 trang dữ liệu),
    // thay vào đó đổi SortColumn/SortDirection trên ViewModel rồi query lại DB.
    private void Grid_Sorting(object sender, DataGridSortingEventArgs e)
    {
        e.Handled = true;

        var columnName = e.Column.SortMemberPath switch
        {
            "TenMon" => nameof(CurriculumSortColumn.TenMon),
            nameof(Data.Entities.ChuongTrinhHoc.NamHoc) => nameof(CurriculumSortColumn.NamHoc),
            _ => null
        };
        if (columnName is null) return;

        ViewModel.ChangeSortCommand.Execute(columnName);

        foreach (var column in ((DataGrid)sender).Columns) column.SortDirection = null;
        e.Column.SortDirection = ViewModel.SortDirection == SortDirection.Ascending
            ? System.ComponentModel.ListSortDirection.Ascending
            : System.ComponentModel.ListSortDirection.Descending;
    }

    private void Grid_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
    {
        if (((DataGrid)sender).SelectedItem is Data.Entities.ChuongTrinhHoc item)
        {
            ViewModel.EditCommand.Execute(item);
        }
    }
}