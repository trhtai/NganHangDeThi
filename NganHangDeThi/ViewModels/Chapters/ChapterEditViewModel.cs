using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using NganHangDeThi.Data.Entities;
using NganHangDeThi.Data.Repositories.Interfaces;
using System.ComponentModel.DataAnnotations;

namespace NganHangDeThi.ViewModels.Chapters;

public partial class ChapterEditViewModel : ObservableValidator
{
    private readonly IChuongRepository _repository;
    private readonly int _monHocId;
    private readonly int? _editingId;

    public bool IsEditMode => _editingId.HasValue;
    public string DialogTitle => IsEditMode ? "Sửa chương" : "Thêm chương";

    // TenChuong ----------
    [ObservableProperty]
    [NotifyDataErrorInfo]
    [NotifyPropertyChangedFor(nameof(TenChuongLength))]
    [Required(ErrorMessage = "Tên chương không được để trống")]
    [StringLength(200, MinimumLength = 2, ErrorMessage = "Tên chương phải từ 2-200 ký tự")]
    private string tenChuong = string.Empty;

    public int TenChuongLength => TenChuong?.Length ?? 0;

    [ObservableProperty]
    public string? duplicateWarning;
    // -------------------

    // ThuTu ----------
    [ObservableProperty]
    [NotifyDataErrorInfo]
    [Required(ErrorMessage = "Vui lòng nhập thứ tự chương")]
    [Range(1, 9999, ErrorMessage = "Thứ tự chương phải là số từ 1-9999")]
    private int? thuTu;
    // ---------------

    public bool? DialogResult { get; private set; }
    public event Action? RequestClose;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(SaveCommand))]
    private bool isSaving;

    public ChapterEditViewModel(
        IChuongRepository repository,
        int monHocId,
        Chuong? chuong)
    {
        _repository = repository;
        _monHocId = monHocId;

        if (chuong is not null)
        {
            _editingId = chuong.Id;
            tenChuong = chuong.TenChuong;
            thuTu = chuong.ThuTu;
        }
        else
        {
            // Thêm mới: tự động đề xuất thứ tự kế tiếp (người dùng vẫn có thể sửa lại).
            LoadNextThuTuAsync();
        }
    }

    private async void LoadNextThuTuAsync()
    {
        ThuTu = await _repository.GetNextThuTuAsync(_monHocId);
    }

    async partial void OnTenChuongChanged(string value)
    {
        // Clear warning.
        DuplicateWarning = null;
        ValidateAllProperties();
        if (HasErrors || string.IsNullOrWhiteSpace(value)) return;

        // Debounce chống dội DB (giống hệt cơ chế của SubjectEditViewModel).
        var snapshot = value;
        await Task.Delay(300);
        if (snapshot != TenChuong) return;

        var exists = await _repository.TenChuongExistsAsync(_monHocId, snapshot, _editingId);

        if (snapshot == TenChuong && exists)
        {
            DuplicateWarning = "Tên chương này đã tồn tại trong môn học";
        }
    }

    private bool CanSave() => !IsSaving;

    [RelayCommand(CanExecute = nameof(CanSave))]
    public async Task SaveAsync()
    {
        ValidateAllProperties();
        if (HasErrors) return;

        if (await _repository.TenChuongExistsAsync(_monHocId, TenChuong, _editingId))
        {
            DuplicateWarning = "Tên chương này đã tồn tại trong môn học";
            return;
        }

        IsSaving = true;
        try
        {
            if (IsEditMode)
            {
                var currentItem = await _repository.GetByIdAsync(_editingId!.Value);
                if (currentItem == null) return;
                currentItem.TenChuong = TenChuong.Trim();
                currentItem.ThuTu = ThuTu!.Value;
                await _repository.UpdateAsync(currentItem);
            }
            else
            {
                await _repository.AddAsync(new Chuong
                {
                    TenChuong = TenChuong.Trim(),
                    ThuTu = ThuTu!.Value,
                    MonHocId = _monHocId,
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
