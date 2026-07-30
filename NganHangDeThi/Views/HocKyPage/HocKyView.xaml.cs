using NganHangDeThi.Data.Repositories.Enums;
using NganHangDeThi.ViewModels.Semesters;
using System.Windows;
using System.Windows.Controls;

namespace NganHangDeThi.Views.HocKyPage;

public partial class HocKyView : UserControl
{
    public HocKyView(SemesterViewModel semesterViewModel)
    {
        InitializeComponent();

        DataContextChanged += (_, e) =>
        {
            if (e.NewValue is SemesterViewModel vm)
            {
                vm.EditDialogHost = (SemesterEditViewModel editVm) =>
                {
                    var owner = Window.GetWindow(this);
                    var result = owner is null
                        ? ChinhSuaHocKyDialog.ShowDialog(Application.Current.MainWindow, editVm)
                        : ChinhSuaHocKyDialog.ShowDialog(owner, editVm);

                    return Task.FromResult(result == true);
                };
            }
        };

        DataContext = semesterViewModel;

        Loaded += async (_, _) =>
        {
            if (DataContext is SemesterViewModel vm && vm.Items.Count == 0)
            {
                await vm.LoadCommand.ExecuteAsync(null);
            }
        };
    }

    private SemesterViewModel ViewModel => (SemesterViewModel)DataContext;

    // Server-side sort: chặn WPF tự sort trên client (vì Items chỉ chứa 1 trang dữ liệu),
    // thay vào đó đổi SortColumn/SortDirection trên ViewModel rồi query lại DB.
    private void Grid_Sorting(object sender, DataGridSortingEventArgs e)
    {
        e.Handled = true;

        var columnName = e.Column.SortMemberPath switch
        {
            nameof(Data.Entities.HocKy.TenHocKy) => nameof(HocKySortColumn.TenHocKy),
            nameof(Data.Entities.HocKy.CreatedAt) => nameof(HocKySortColumn.CreatedAt),
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
        if (((DataGrid)sender).SelectedItem is Data.Entities.HocKy item)
        {
            ViewModel.EditCommand.Execute(item);
        }
    }
}