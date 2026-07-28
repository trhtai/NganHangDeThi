using CommunityToolkit.Mvvm.Messaging;
using NganHangDeThi.Data.Entities;
using NganHangDeThi.Data.Repositories.Enums;
using NganHangDeThi.Messages;
using NganHangDeThi.ViewModels.KhoaPage;
using NganHangDeThi.ViewModels.NienKhoaPage;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace NganHangDeThi.Views.NienKhoaPage;

public partial class NienKhoaView : UserControl
{
    public NienKhoaView(NienKhoaViewModel nienKhoavm)
    {
        InitializeComponent();

        DataContextChanged += (_, e) =>
        {
            if (e.NewValue is NienKhoaViewModel vm)
            {
                vm.EditDialogHost = (ChinhSuaNienKhoaViewModel eidtVm) =>
                {
                    var owner = Window.GetWindow(this);
                    var result = owner is null
                        ? ChinhSuaNienKhoaDialog.ShowDialog(Application.Current.MainWindow, eidtVm)
                        : ChinhSuaNienKhoaDialog.ShowDialog(owner, eidtVm);

                    return Task.FromResult(result == true);
                };
            }
        };

        DataContext = nienKhoavm;

        Loaded += async (_, _) =>
        {
            if (DataContext is NienKhoaViewModel vm && vm.Items.Count == 0)
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

    private NienKhoaViewModel ViewModel => (NienKhoaViewModel)DataContext;

    private void Grid_Sorting(object sender, DataGridSortingEventArgs e)
    {
        e.Handled = true;

        var columnName = e.Column.SortMemberPath switch
        {
            nameof(NienKhoa.TenNienKhoa) => nameof(NienKhoaSortColumn.TenNienKhoa),
            nameof(NienKhoa.CreatedAt) => nameof(NienKhoaSortColumn.CreatedAt),
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
        if (((DataGrid)sender).SelectedItem is NienKhoa item)
        {
            ViewModel.EditCommand.Execute(item);
        }
    }

    private void Grid_LoadingRow(object sender, DataGridRowEventArgs e)
    {
        if (DataContext is NienKhoaViewModel vm)
        {
            int stt = (vm.PageIndex - 1) * vm.PageSize + e.Row.GetIndex() + 1;

            e.Row.Header = stt.ToString();
        }
    }
}

