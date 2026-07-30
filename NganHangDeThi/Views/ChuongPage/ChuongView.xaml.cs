using NganHangDeThi.Data.Repositories.Enums;
using NganHangDeThi.ViewModels.Chapters;
using System.Windows;
using System.Windows.Controls;

namespace NganHangDeThi.Views.ChuongPage;

public partial class ChuongView : UserControl
{
    public ChuongView(ChapterViewModel chapterViewModel)
    {
        InitializeComponent();

        DataContextChanged += (_, e) =>
        {
            if (e.NewValue is ChapterViewModel vm)
            {
                vm.EditDialogHost = (ChapterEditViewModel editVm) =>
                {
                    var owner = Window.GetWindow(this);
                    var result = owner is null
                        ? ChinhSuaChuongDialog.ShowDialog(Application.Current.MainWindow, editVm)
                        : ChinhSuaChuongDialog.ShowDialog(owner, editVm);

                    return Task.FromResult(result == true);
                };
            }
        };

        DataContext = chapterViewModel;

        Loaded += async (_, _) =>
        {
            if (DataContext is ChapterViewModel vm && vm.Items.Count == 0)
            {
                await vm.LoadCommand.ExecuteAsync(null);
            }
        };
    }

    private ChapterViewModel ViewModel => (ChapterViewModel)DataContext;

    // Server-side sort: chặn WPF tự sort trên client (vì Items chỉ chứa 1 trang dữ liệu),
    // thay vào đó đổi SortColumn/SortDirection trên ViewModel rồi query lại DB.
    private void Grid_Sorting(object sender, DataGridSortingEventArgs e)
    {
        e.Handled = true;

        var columnName = e.Column.SortMemberPath switch
        {
            nameof(Data.Entities.Chuong.TenChuong) => nameof(ChuongSortColumn.TenChuong),
            nameof(Data.Entities.Chuong.ThuTu) => nameof(ChuongSortColumn.ThuTu),
            nameof(Data.Entities.Chuong.CreatedAt) => nameof(ChuongSortColumn.CreatedAt),
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
        if (((DataGrid)sender).SelectedItem is Data.Entities.Chuong item)
        {
            ViewModel.EditCommand.Execute(item);
        }
    }
}