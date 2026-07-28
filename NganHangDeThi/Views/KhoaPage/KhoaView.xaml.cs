using CommunityToolkit.Mvvm.Messaging;
using NganHangDeThi.Data.Repositories.Enums;
using NganHangDeThi.Messages;
using NganHangDeThi.ViewModels.KhoaPage;
using System.Windows;
using System.Windows.Controls;

namespace NganHangDeThi.Views.KhoaPage;

public partial class KhoaView : UserControl
{
    public KhoaView(KhoaViewModel khoaViewModel)
    {
        InitializeComponent();

        DataContextChanged += (_, e) =>
        {
            if (e.NewValue is KhoaViewModel vm)
            {
                vm.EditDialogHost = (ChinhSuaKhoaViewModel eidtVm) =>
                {
                    var owner = Window.GetWindow(this);
                    var result = owner is null
                        ? ChinhSuaKhoaDialogView.ShowDialog(Application.Current.MainWindow, eidtVm)
                        : ChinhSuaKhoaDialogView.ShowDialog(owner, eidtVm);

                    return Task.FromResult(result == true);
                };
            }
        };

        DataContext = khoaViewModel;

        Loaded += async (_, _) =>
        {
            if (DataContext is KhoaViewModel vm && vm.Items.Count == 0)
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

    private KhoaViewModel ViewModel => (KhoaViewModel)DataContext;

    private void Grid_Sorting(object sender, DataGridSortingEventArgs e)
    {
        e.Handled = true;

        var columnName = e.Column.SortMemberPath switch
        {
            nameof(Data.Entities.Khoa.TenKhoa) => nameof(KhoaSortColumn.TenKhoa),
            nameof(Data.Entities.Khoa.CreatedAt) => nameof(KhoaSortColumn.CreatedAt),
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
        if (((DataGrid)sender).SelectedItem is Data.Entities.Khoa item)
        {
            ViewModel.EditCommand.Execute(item);
        }
    }

    private void Grid_LoadingRow(object sender, DataGridRowEventArgs e)
    {
        if (DataContext is KhoaViewModel vm)
        {
            int stt = (vm.PageIndex - 1) * vm.PageSize + e.Row.GetIndex() + 1;

            e.Row.Header = stt.ToString();
        }
    }
}
