using CommunityToolkit.Mvvm.Messaging;
using NganHangDeThi.Data.Entities;
using NganHangDeThi.Data.Repositories.Enums;
using NganHangDeThi.Messages;
using NganHangDeThi.ViewModels.LopPage;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace NganHangDeThi.Views.LopPage;

public partial class LopView : UserControl
{
    public LopView(LopViewModel lopVm)
    {
        InitializeComponent();

        DataContextChanged += (_, e) =>
        {
            if (e.NewValue is LopViewModel vm)
            {
                vm.EditDialogHost = (ChinhSuaLopViewModel eidtVm) =>
                {
                    var owner = Window.GetWindow(this);
                    var result = owner is null
                        ? ChinhSuaLopDialog.ShowDialog(Application.Current.MainWindow, eidtVm)
                        : ChinhSuaLopDialog.ShowDialog(owner, eidtVm);

                    return Task.FromResult(result == true);
                };
            }
        };

        DataContext = lopVm;

        Loaded += async (_, _) =>
        {
            if (DataContext is LopViewModel vm && vm.Items.Count == 0)
            {
                await vm.LoadCommand.ExecuteAsync(null);
            }
        };

        // 1. Đăng ký lắng nghe tín hiệu thay đổi định dạng ngày
        WeakReferenceMessenger.Default.Register<DateFormatChangedMessage>(this, (recipient, message) =>
        {
            // Bắt buộc giao diện DataGrid phải render lại (chạy lại Converter)
            Grid.Items.Refresh();
        });

        // 2. [Tùy chọn nhưng khuyên dùng] Hủy lắng nghe khi trang bị đóng để dọn dẹp RAM
        Unloaded += (_, _) =>
        {
            WeakReferenceMessenger.Default.Unregister<DateFormatChangedMessage>(this);
        };
    }

    private LopViewModel ViewModel => (LopViewModel)DataContext;

    private void Grid_Sorting(object sender, DataGridSortingEventArgs e)
    {
        e.Handled = true;

        var columnName = e.Column.SortMemberPath switch
        {
            nameof(Lop.MaLop) => nameof(LopSortColumn.MaLop),
            nameof(Lop.CreatedAt) => nameof(LopSortColumn.CreatedAt),
            _ => null
        };
        if (columnName is null) return;

        ViewModel.ChangeSortCommand.Execute(columnName);

        foreach (var column in ((DataGrid)sender).Columns) column.SortDirection = null;
        e.Column.SortDirection = ViewModel.SortDirection == SortDirection.Ascending
            ? System.ComponentModel.ListSortDirection.Ascending
            : System.ComponentModel.ListSortDirection.Descending;
    }

    private void Grid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
    {
        if (((DataGrid)sender).SelectedItem is Lop item)
        {
            ViewModel.EditCommand.Execute(item);
        }
    }

    // Khi render ra hàng đó thì tính toán stt của nó.
    private void Grid_LoadingRow(object sender, DataGridRowEventArgs e)
    {
        if (DataContext is LopViewModel vm)
        {
            int stt = (vm.PageIndex - 1) * vm.PageSize + e.Row.GetIndex() + 1;

            e.Row.Header = stt.ToString();
        }
    }
}
