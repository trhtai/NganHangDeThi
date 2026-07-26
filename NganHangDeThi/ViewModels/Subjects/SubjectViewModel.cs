using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HandyControl.Data;
using NganHangDeThi.Data.Entities;
using NganHangDeThi.Data.Repositories.Enums;
using NganHangDeThi.Data.Repositories.Interfaces;
using NganHangDeThi.Services.Interfaces;
using NganHangDeThi.ViewModels.Subjects.Factories.Interfaces;
using System.Collections.ObjectModel;

namespace NganHangDeThi.ViewModels.Subjects;

public partial class SubjectViewModel : ObservableObject
{
    private readonly IMonHocRepository _repository;
    private readonly IToastService _toast;
    private readonly IConfirmService _confirm;
    private readonly ISubjectEditViewModelFactory _subjectEditViewModelFactory;

    private CancellationTokenSource? _loadCts;
    private CancellationTokenSource? _searchDebounceCts;

    public ObservableCollection<MonHoc> Items { get; } = [];
    public ObservableCollection<MonHoc> SelectedItems { get; } = [];

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsEmpty))]
    private bool isLoading;

    [ObservableProperty]
    private int pageIndex = 1;

    [ObservableProperty]
    private int pageSize = 20;

    [ObservableProperty]
    private int totalCount;

    [ObservableProperty]
    private int totalPages = 1;

    [ObservableProperty]
    private string? searchText;

    [ObservableProperty]
    private MonHocSortColumn sortColumn = MonHocSortColumn.CreatedAt;

    [ObservableProperty]
    private SortDirection sortDirection = SortDirection.Descending;

    public bool IsEmpty => !IsLoading && TotalCount == 0;
    public bool HasSelection => SelectedItems.Count > 0;
    public int[] PageSizeOptions { get; } = { 20, 50, 100 };

    public SubjectViewModel(
        IMonHocRepository repository,
        ISubjectEditViewModelFactory subjectEditViewModelFactory,
        IToastService toast,
        IConfirmService confirm)
    {
        _repository = repository;
        _subjectEditViewModelFactory = subjectEditViewModelFactory;
        _toast = toast;
        _confirm = confirm; 
        
        SelectedItems.CollectionChanged += (_, _) =>
        {
            // Báo cho UI biết HasSelection đã thay đổi để ẩn/hiện nút
            OnPropertyChanged(nameof(HasSelection));

            // Báo cho UI update lại biến Count trong StringFormat="Xóa đã chọn ({0})"
            OnPropertyChanged(nameof(SelectedItems));

            // Kích hoạt lại điều kiện CanExecute của hàm DeleteSelected
            DeleteSelectedCommand.NotifyCanExecuteChanged();
        };
    }

    #region Load
    [RelayCommand]
    private Task LoadAsync() => ReloadAsync();

    [RelayCommand]
    private Task RefreshAsync() => ReloadAsync();

    private async Task ReloadAsync()
    {
        _loadCts?.Cancel();
        var cts = new CancellationTokenSource();
        _loadCts = cts;

        IsLoading = true;
        try
        {
            var result = await _repository.GetPagedAsync(
                SearchText, SortColumn, SortDirection, PageIndex, PageSize, cts.Token);

            if (cts.IsCancellationRequested) return;

            Items.Clear();
            foreach (var item in result.Items) Items.Add(item);
            SelectedItems.Clear();

            TotalCount = result.TotalCount;
            TotalPages = result.TotalPages;
            OnPropertyChanged(nameof(IsEmpty));
        }
        catch (OperationCanceledException) { }
        catch (Exception)
        {
            _toast.Error("Không tải được danh sách môn học");
        }
        finally
        {
            if (_loadCts == cts) IsLoading = false;
        }
    }
    #endregion

    #region Pagination commands
    [RelayCommand]
    private Task PageUpdatedAsync(FunctionEventArgs<int> e)
    {
        PageIndex = e.Info; // Lấy số trang người dùng vừa bấm từ UI
        return ReloadAsync();
    }

    partial void OnPageSizeChanged(int value)
    {
        PageIndex = 1;
        _ = ReloadAsync();
    }
    #endregion

    #region Add and Edit Commands
    [RelayCommand]
    private async Task AddAsync()
    {
        var opened = await OpenEditDialogAsync(null);
        if (opened)
        {
            _toast.Success("Thêm môn học thành công");
            await ReloadAsync();
        }
    }

    [RelayCommand]
    private async Task EditAsync(MonHoc? item)
    {
        if (item is null) return;
        var opened = await OpenEditDialogAsync(item);
        if (opened)
        {
            _toast.Success("Cập nhật lớp học thành công");
            await ReloadAsync();
        }
    }

    public Func<SubjectEditViewModel, Task<bool>>? EditDialogHost { get; set; }

    private async Task<bool> OpenEditDialogAsync(MonHoc? monHoc)
    {
        if (EditDialogHost is null) return false;
        var editVm = _subjectEditViewModelFactory.Create(monHoc);

        return await EditDialogHost(editVm);
    }
    #endregion

    #region Delete commands.
    [RelayCommand]
    private async Task DeleteAsync(MonHoc? item, CancellationToken ct)
    {
        if (item is null) return;
        if (!_confirm.Confirm($"Bạn có chắc muốn xóa môn \"{item.TenMon}\"?")) return;

        try
        {
            IsLoading = true;
            await _repository.DeleteRangeAsync(new[] { item.Id }, ct);
            _toast.Success("Đã xóa môn học");
            await ReloadAsync();
        }
        catch (Exception ex)
        {
            _toast.Error("Không thể xóa môn học. Lỗi: " + ex.Message);
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand(CanExecute = nameof(HasSelection))]
    private async Task DeleteSelectedAsync(CancellationToken ct)
    {
        var count = SelectedItems.Count;
        if (count == 0) return;
        if (!_confirm.Confirm($"Bạn có chắc muốn xóa {count} môn học đã chọn?")) return;

        try
        {
            IsLoading = true;
            var ids = SelectedItems.Select(x => x.Id).ToArray();

            await _repository.DeleteRangeAsync(ids, ct);
            _toast.Success($"Đã xóa {count} môn học");

            SelectedItems.Clear();
            await ReloadAsync();
        }
        catch (Exception)
        {
            _toast.Error("Không thể xóa danh sách môn học đã chọn!");
        }
        finally
        {
            IsLoading = false;
        }
    }
    #endregion

    #region Search commands.
    partial void OnSearchTextChanged(string? value)
    {
        _searchDebounceCts?.Cancel();
        var cts = new CancellationTokenSource();
        _searchDebounceCts = cts;
        DebounceSearchAsync(cts.Token);
    }

    private async void DebounceSearchAsync(CancellationToken ct)
    {
        try
        {
            await Task.Delay(300, ct);
            if (ct.IsCancellationRequested) return;
            PageIndex = 1;
            await ReloadAsync();
        }
        catch (TaskCanceledException) { }
    }
    #endregion

    #region Sort commands.
    [RelayCommand]
    private void ChangeSort(string columnName)
    {
        var column = Enum.Parse<MonHocSortColumn>(columnName);
        if (SortColumn == column)
        {
            SortDirection = SortDirection == SortDirection.Ascending
                ? SortDirection.Descending
                : SortDirection.Ascending;
        }
        else
        {
            SortColumn = column;
            SortDirection = SortDirection.Ascending;
        }

        _ = ReloadAsync();
    }
    #endregion
}
