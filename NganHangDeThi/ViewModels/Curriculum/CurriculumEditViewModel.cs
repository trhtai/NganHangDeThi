using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using NganHangDeThi.Data.Entities;
using NganHangDeThi.Data.Repositories.Interfaces;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;

namespace NganHangDeThi.ViewModels.Curriculum;

public partial class CurriculumEditViewModel : ObservableValidator
{
    private readonly IChuongTrinhHocRepository _repository;
    private readonly int _lopId;
    private readonly int? _editingId;

    public bool IsEditMode => _editingId.HasValue;
    public string DialogTitle => IsEditMode ? "Sửa môn học của lớp" : "Thêm môn học cho lớp";

    public ObservableCollection<MonHoc> MonHocOptions { get; } = [];

    [ObservableProperty]
    private bool isLoadingOptions = true;

    // MonHoc được chọn ----------
    [ObservableProperty]
    [NotifyDataErrorInfo]
    [Required(ErrorMessage = "Vui lòng chọn môn học")]
    private MonHoc? selectedMonHoc;
    // ---------------------------

    // NamHoc ----------
    [ObservableProperty]
    [NotifyDataErrorInfo]
    [Required(ErrorMessage = "Vui lòng nhập năm học")]
    [Range(2000, 2100, ErrorMessage = "Năm học phải từ 2000-2100")]
    private int? namHoc;
    // -----------------

    [ObservableProperty]
    public string? duplicateWarning;

    public bool? DialogResult { get; private set; }
    public event Action? RequestClose;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(SaveCommand))]
    private bool isSaving;

    public CurriculumEditViewModel(
        IChuongTrinhHocRepository repository,
        int lopId,
        ChuongTrinhHoc? existing)
    {
        _repository = repository;
        _lopId = lopId;

        if (existing is not null)
        {
            _editingId = existing.Id;
            namHoc = existing.NamHoc;
        }
        else
        {
            namHoc = DateTime.Now.Year;
        }

        LoadOptionsAsync(existing);
    }

    private async void LoadOptionsAsync(ChuongTrinhHoc? existing)
    {
        try
        {
            var options = await _repository.GetMonHocOptionsAsync(_lopId, _editingId);

            MonHocOptions.Clear();
            foreach (var mh in options) MonHocOptions.Add(mh);

            if (existing is not null)
            {
                // Chọn lại đúng môn học đang gán (so theo Id vì Include trả về instance khác).
                SelectedMonHoc = MonHocOptions.FirstOrDefault(x => x.Id == existing.MonHocId);
            }
        }
        finally
        {
            IsLoadingOptions = false;
        }
    }

    async partial void OnSelectedMonHocChanged(MonHoc? value) => await CheckDuplicateAsync();
    async partial void OnNamHocChanged(int? value) => await CheckDuplicateAsync();

    private async Task CheckDuplicateAsync()
    {
        DuplicateWarning = null;
        ValidateAllProperties();
        if (HasErrors || SelectedMonHoc is null || NamHoc is null) return;

        var monHocId = SelectedMonHoc.Id;
        var year = NamHoc.Value;

        // Debounce nhẹ chống dội DB khi người dùng đổi lựa chọn liên tục.
        await Task.Delay(200);
        if (SelectedMonHoc?.Id != monHocId || NamHoc != year) return;

        var exists = await _repository.ExistsAsync(_lopId, monHocId, year, _editingId);
        if (exists)
        {
            DuplicateWarning = "Lớp này đã được gán môn học này trong năm học đã chọn";
        }
    }

    private bool CanSave() => !IsSaving;

    [RelayCommand(CanExecute = nameof(CanSave))]
    public async Task SaveAsync()
    {
        ValidateAllProperties();
        if (HasErrors) return;

        if (await _repository.ExistsAsync(_lopId, SelectedMonHoc!.Id, NamHoc!.Value, _editingId))
        {
            DuplicateWarning = "Lớp này đã được gán môn học này trong năm học đã chọn";
            return;
        }

        IsSaving = true;
        try
        {
            if (IsEditMode)
            {
                var currentItem = await _repository.GetByIdAsync(_editingId!.Value);
                if (currentItem == null) return;
                currentItem.MonHocId = SelectedMonHoc!.Id;
                currentItem.NamHoc = NamHoc!.Value;
                await _repository.UpdateAsync(currentItem);
            }
            else
            {
                await _repository.AddAsync(new ChuongTrinhHoc
                {
                    LopId = _lopId,
                    MonHocId = SelectedMonHoc!.Id,
                    NamHoc = NamHoc!.Value,
                });
            }

            DialogResult = true;
            RequestClose?.Invoke();
        }
        finally
        {
            IsSaving = false;
        }
    }

    [RelayCommand]
    private void Cancel()
    {
        DialogResult = false;
        RequestClose?.Invoke();
    }
}