using NganHangDeThi.Data.Repositories.Enums;
using NganHangDeThi.ViewModels.Subjects;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;

namespace NganHangDeThi.Views.MonHoc;

public partial class MonHocView : UserControl
{
    public MonHocView(SubjectViewModel subjectViewModel)
    {
        InitializeComponent();

        DataContextChanged += (_, e) =>
        {
            if (e.NewValue is SubjectViewModel vm)
            {
                vm.EditDialogHost = (SubjectEditViewModel eidtVm) =>
                {
                    var owner = Window.GetWindow(this);
                    var result = owner is null
                        ? ChinhSuaMonHocDialogView.ShowDialog(Application.Current.MainWindow, eidtVm)
                        : ChinhSuaMonHocDialogView.ShowDialog(owner, eidtVm);

                    return Task.FromResult(result == true);
                };
            }
        };

        DataContext = subjectViewModel;

        Loaded += async (_, _) =>
        {
            if (DataContext is SubjectViewModel vm && vm.Items.Count == 0)
            {
                await vm.LoadCommand.ExecuteAsync(null);
            }
        };
    }

    private SubjectViewModel ViewModel => (SubjectViewModel)DataContext;

    // Server-side sort: chặn WPF tự sort trên client (vì Items chỉ chứa 1 trang dữ liệu),
    // thay vào đó đổi SortColumn/SortDirection trên ViewModel rồi query lại DB.
    private void Grid_Sorting(object sender, DataGridSortingEventArgs e)
    {
        e.Handled = true;

        var columnName = e.Column.SortMemberPath switch
        {
            nameof(Data.Entities.MonHoc.TenMon) => nameof(MonHocSortColumn.TenMon),
            nameof(Data.Entities.MonHoc.CreatedAt) => nameof(MonHocSortColumn.CreatedAt),
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
        if (((DataGrid)sender).SelectedItem is Data.Entities.MonHoc item)
        {
            ViewModel.EditCommand.Execute(item);
        }
    }

    // Khi render ra hàng đó thì tính toán stt của nó.
    private void Grid_LoadingRow(object sender, DataGridRowEventArgs e)
    {
        if (DataContext is SubjectViewModel vm)
        {
            // Công thức: STT = (Trang hiện tại - 1) * Số dòng/trang + Vị trí của dòng + 1
            int stt = (vm.PageIndex - 1) * vm.PageSize + e.Row.GetIndex() + 1;

            // Gán STT vào Header của dòng
            e.Row.Header = stt.ToString();
        }
    }
}
