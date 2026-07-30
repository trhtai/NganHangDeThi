using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HandyControl.Data;
using NganHangDeThi.Data.Entities;
using NganHangDeThi.Data.Repositories.Enums;
using NganHangDeThi.Data.Repositories.Interfaces;
using NganHangDeThi.Services.Interfaces;
using NganHangDeThi.ViewModels.Semesters.Factories.Interfaces;
using System.Collections.ObjectModel;
using System.Drawing.Printing;

namespace NganHangDeThi.ViewModels.Semesters;

public partial class SemesterViewModel : ObservableObject
{
    private readonly IHocKyRepository _repository;
    private readonly IToastService _toast;
    private readonly IConfirmService _confirm;
    private readonly ISemesterEditViewModelFactory _semesterEditViewModelFactory;

    private CancellationTokenSource? _loadCts;
    private CancellationTokenSource? _searchDebounceCts;

    // Niên khóa đang được quản lý học kỳ - cố định trong suốt vòng đời ViewModel này.
    public int NienKhoaId { get; }
    public string TenNienKhoa { get; }

    public ObservableCollection<HocKy> Items { get; } = [];
    public ObservableCollection<HocKy> SelectedItems { get; } = [];

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
    private HocKySortColumn sortColumn = HocKySortColumn.CreatedAt;

    [ObservableProperty]
    private SortDirection sortDirection = SortDirection.Ascending;

    public bool IsEmpty => !IsLoading && TotalCount == 0;
    public bool HasSelection => SelectedItems.Count > 0;
    public int[] PageSizeOptions { get; } = { 20, 50, 100 };

    public SemesterViewModel(
        NienKhoa nienKhoa,
        IHocKyRepository repository,
        ISemesterEditViewModelFactory semesterEditViewModelFactory,
        IToastService toast,
        IConfirmService confirm)
    {
        NienKhoaId = nienKhoa.Id;
        TenNienKhoa = nienKhoa.TenNienKhoa;

        _repository = repository;
        _semesterEditViewModelFactory = semesterEditViewModelFactory;
        _toast = toast;
        _confirm = confirm;

        SelectedItems.CollectionChanged += (_, _) =>
        {
            OnPropertyChanged(nameof(HasSelection));
            OnPropertyChanged(nameof(SelectedItems));
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
                NienKhoaId, SearchText, SortColumn, SortDirection, PageIndex, PageSize, cts.Token);

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
            _toast.Error("Không tải được danh sách học kỳ");
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
        PageIndex = e.Info;
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
            _toast.Success("Thêm học kỳ thành công");
            await ReloadAsync();
        }
    }

    [RelayCommand]
    private async Task EditAsync(HocKy? item)
    {
        if (item is null) return;
        var opened = await OpenEditDialogAsync(item);
        if (opened)
        {
            _toast.Success("Cập nhật học kỳ thành công");
            await ReloadAsync();
        }
    }

    public Func<SemesterEditViewModel, Task<bool>>? EditDialogHost { get; set; }

    private async Task<bool> OpenEditDialogAsync(HocKy? hocKy)
    {
        if (EditDialogHost is null) return false;
        var editVm = _semesterEditViewModelFactory.Create(NienKhoaId, hocKy);

        return await EditDialogHost(editVm);
    }
    #endregion

    #region Delete commands.
    [RelayCommand]
    private async Task DeleteAsync(HocKy? item, CancellationToken ct)
    {
        if (item is null) return;
        if (!_confirm.Confirm($"Bạn có chắc muốn xóa \"{item.TenHocKy}\"?")) return;

        try
        {
            IsLoading = true;
            await _repository.DeleteRangeAsync(new[] { item.Id }, ct);
            _toast.Success("Đã xóa học kỳ");
            await ReloadAsync();
        }
        catch (Exception ex)
        {
            _toast.Error("Không thể xóa học kỳ. Lỗi: " + ex.Message);
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
        if (!_confirm.Confirm($"Bạn có chắc muốn xóa {count} học kỳ đã chọn?")) return;

        try
        {
            IsLoading = true;
            var ids = SelectedItems.Select(x => x.Id).ToArray();

            await _repository.DeleteRangeAsync(ids, ct);
            _toast.Success($"Đã xóa {count} học kỳ");

            SelectedItems.Clear();
            await ReloadAsync();
        }
        catch (Exception)
        {
            _toast.Error("Không thể xóa danh sách học kỳ đã chọn!");
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
        var column = Enum.Parse<HocKySortColumn>(columnName);
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