using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HandyControl.Data;
using NganHangDeThi.Data.Entities;
using NganHangDeThi.Data.Repositories.Enums;
using NganHangDeThi.Data.Repositories.Interfaces;
using NganHangDeThi.Services.Interfaces;
using NganHangDeThi.ViewModels.Chapters.Factories.Interfaces;
using System.Collections.ObjectModel;

namespace NganHangDeThi.ViewModels.Chapters;

public partial class ChapterViewModel : ObservableObject
{
    private readonly IChuongRepository _repository;
    private readonly IToastService _toast;
    private readonly IConfirmService _confirm;
    private readonly IChapterEditViewModelFactory _chapterEditViewModelFactory;

    private CancellationTokenSource? _loadCts;
    private CancellationTokenSource? _searchDebounceCts;

    // Môn học đang được quản lý chương - cố định trong suốt vòng đời ViewModel này.
    public int MonHocId { get; }
    public string TenMon { get; }

    public ObservableCollection<Chuong> Items { get; } = [];
    public ObservableCollection<Chuong> SelectedItems { get; } = [];

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
    private ChuongSortColumn sortColumn = ChuongSortColumn.ThuTu;

    [ObservableProperty]
    private SortDirection sortDirection = SortDirection.Ascending;

    public bool IsEmpty => !IsLoading && TotalCount == 0;
    public bool HasSelection => SelectedItems.Count > 0;
    public int[] PageSizeOptions { get; } = { 20, 50, 100 };

    public ChapterViewModel(
        MonHoc monHoc,
        IChuongRepository repository,
        IChapterEditViewModelFactory chapterEditViewModelFactory,
        IToastService toast,
        IConfirmService confirm)
    {
        MonHocId = monHoc.Id;
        TenMon = monHoc.TenMon;

        _repository = repository;
        _chapterEditViewModelFactory = chapterEditViewModelFactory;
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
                MonHocId, SearchText, SortColumn, SortDirection, PageIndex, PageSize, cts.Token);

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
            _toast.Error("Không tải được danh sách chương");
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
            _toast.Success("Thêm chương thành công");
            await ReloadAsync();
        }
    }

    [RelayCommand]
    private async Task EditAsync(Chuong? item)
    {
        if (item is null) return;
        var opened = await OpenEditDialogAsync(item);
        if (opened)
        {
            _toast.Success("Cập nhật chương thành công");
            await ReloadAsync();
        }
    }

    public Func<ChapterEditViewModel, Task<bool>>? EditDialogHost { get; set; }

    private async Task<bool> OpenEditDialogAsync(Chuong? chuong)
    {
        if (EditDialogHost is null) return false;
        var editVm = _chapterEditViewModelFactory.Create(MonHocId, chuong);

        return await EditDialogHost(editVm);
    }
    #endregion

    #region Delete commands.
    [RelayCommand]
    private async Task DeleteAsync(Chuong? item, CancellationToken ct)
    {
        if (item is null) return;
        if (!_confirm.Confirm($"Bạn có chắc muốn xóa chương \"{item.TenChuong}\"?")) return;

        try
        {
            IsLoading = true;
            await _repository.DeleteRangeAsync(new[] { item.Id }, ct);
            _toast.Success("Đã xóa chương");
            await ReloadAsync();
        }
        catch (Exception ex)
        {
            _toast.Error("Không thể xóa chương. Lỗi: " + ex.Message);
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
        if (!_confirm.Confirm($"Bạn có chắc muốn xóa {count} chương đã chọn?")) return;

        try
        {
            IsLoading = true;
            var ids = SelectedItems.Select(x => x.Id).ToArray();

            await _repository.DeleteRangeAsync(ids, ct);
            _toast.Success($"Đã xóa {count} chương");

            SelectedItems.Clear();
            await ReloadAsync();
        }
        catch (Exception)
        {
            _toast.Error("Không thể xóa danh sách chương đã chọn!");
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
        var column = Enum.Parse<ChuongSortColumn>(columnName);
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
