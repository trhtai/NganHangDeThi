using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HandyControl.Data;
using NganHangDeThi.Data.Entities;
using NganHangDeThi.Data.Repositories.Enums;
using NganHangDeThi.Data.Repositories.Interfaces;
using NganHangDeThi.Services.Interfaces;
using NganHangDeThi.ViewModels.Curriculum.Factories.Interfaces;
using System.Collections.ObjectModel;
using System.Drawing.Printing;

namespace NganHangDeThi.ViewModels.Curriculum;

public partial class CurriculumViewModel : ObservableObject
{
    private readonly IChuongTrinhHocRepository _repository;
    private readonly IToastService _toast;
    private readonly IConfirmService _confirm;
    private readonly ICurriculumEditViewModelFactory _curriculumEditViewModelFactory;

    private CancellationTokenSource? _loadCts;
    private CancellationTokenSource? _searchDebounceCts;

    // Lớp học đang được quản lý môn học - cố định trong suốt vòng đời ViewModel này.
    public int LopId { get; }
    public string MaLop { get; }

    public ObservableCollection<ChuongTrinhHoc> Items { get; } = [];
    public ObservableCollection<ChuongTrinhHoc> SelectedItems { get; } = [];

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
    private CurriculumSortColumn sortColumn = CurriculumSortColumn.NamHoc;

    [ObservableProperty]
    private SortDirection sortDirection = SortDirection.Descending;

    public bool IsEmpty => !IsLoading && TotalCount == 0;
    public bool HasSelection => SelectedItems.Count > 0;
    public int[] PageSizeOptions { get; } = { 20, 50, 100 };

    public CurriculumViewModel(
        Lop lop,
        IChuongTrinhHocRepository repository,
        ICurriculumEditViewModelFactory curriculumEditViewModelFactory,
        IToastService toast,
        IConfirmService confirm)
    {
        LopId = lop.Id;
        MaLop = lop.MaLop;

        _repository = repository;
        _curriculumEditViewModelFactory = curriculumEditViewModelFactory;
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
                LopId, SearchText, SortColumn, SortDirection, PageIndex, PageSize, cts.Token);

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
            _toast.Error("Không tải được danh sách môn học của lớp");
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
            _toast.Success("Thêm môn học vào lớp thành công");
            await ReloadAsync();
        }
    }

    [RelayCommand]
    private async Task EditAsync(ChuongTrinhHoc? item)
    {
        if (item is null) return;
        var opened = await OpenEditDialogAsync(item);
        if (opened)
        {
            _toast.Success("Cập nhật thành công");
            await ReloadAsync();
        }
    }

    public Func<CurriculumEditViewModel, Task<bool>>? EditDialogHost { get; set; }

    private async Task<bool> OpenEditDialogAsync(ChuongTrinhHoc? item)
    {
        if (EditDialogHost is null) return false;
        var editVm = _curriculumEditViewModelFactory.Create(LopId, item);

        return await EditDialogHost(editVm);
    }
    #endregion

    #region Delete commands.
    [RelayCommand]
    private async Task DeleteAsync(ChuongTrinhHoc? item, CancellationToken ct)
    {
        if (item is null) return;
        if (!_confirm.Confirm($"Bạn có chắc muốn xóa môn \"{item.MonHoc.TenMon}\" (năm học {item.NamHoc}) khỏi lớp?")) return;

        try
        {
            IsLoading = true;
            await _repository.DeleteRangeAsync(new[] { item.Id }, ct);
            _toast.Success("Đã xóa khỏi chương trình học");
            await ReloadAsync();
        }
        catch (Exception ex)
        {
            _toast.Error("Không thể xóa. Lỗi: " + ex.Message);
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
        if (!_confirm.Confirm($"Bạn có chắc muốn xóa {count} môn học đã chọn khỏi lớp?")) return;

        try
        {
            IsLoading = true;
            var ids = SelectedItems.Select(x => x.Id).ToArray();

            await _repository.DeleteRangeAsync(ids, ct);
            _toast.Success($"Đã xóa {count} môn học khỏi lớp");

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
        var column = Enum.Parse<CurriculumSortColumn>(columnName);
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