using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using NganHangDeThi.Data.Entities;
using NganHangDeThi.Data.Repositories.Interfaces;
using System.ComponentModel.DataAnnotations;

namespace NganHangDeThi.ViewModels.Semesters;

public partial class SemesterEditViewModel : ObservableValidator
{
    private readonly IHocKyRepository _repository;
    private readonly int _nienKhoaId;
    private readonly int? _editingId;

    public bool IsEditMode => _editingId.HasValue;
    public string DialogTitle => IsEditMode ? "Sửa học kỳ" : "Thêm học kỳ";

    // TenHocKy ----------
    [ObservableProperty]
    [NotifyDataErrorInfo]
    [NotifyPropertyChangedFor(nameof(TenHocKyLength))]
    [Required(ErrorMessage = "Tên học kỳ không được để trống")]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "Tên học kỳ phải từ 2-100 ký tự")]
    private string tenHocKy = string.Empty;

    public int TenHocKyLength => TenHocKy?.Length ?? 0;

    [ObservableProperty]
    public string? duplicateWarning;
    // -------------------

    public bool? DialogResult { get; private set; }
    public event Action? RequestClose;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(SaveCommand))]
    private bool isSaving;

    public SemesterEditViewModel(
        IHocKyRepository repository,
        int nienKhoaId,
        HocKy? hocKy)
    {
        _repository = repository;
        _nienKhoaId = nienKhoaId;

        if (hocKy is not null)
        {
            _editingId = hocKy.Id;
            tenHocKy = hocKy.TenHocKy;
        }
    }

    async partial void OnTenHocKyChanged(string value)
    {
        // Clear warning.
        DuplicateWarning = null;
        ValidateAllProperties();
        if (HasErrors || string.IsNullOrWhiteSpace(value)) return;

        var snapshot = value;
        await Task.Delay(300);
        if (snapshot != TenHocKy) return;

        var exists = await _repository.TenHocKyExistsAsync(_nienKhoaId, snapshot, _editingId);

        if (snapshot == TenHocKy && exists)
        {
            DuplicateWarning = "Tên học kỳ này đã tồn tại trong niên khóa";
        }
    }

    private bool CanSave() => !IsSaving;

    [RelayCommand(CanExecute = nameof(CanSave))]
    public async Task SaveAsync()
    {
        ValidateAllProperties();
        if (HasErrors) return;

        if (await _repository.TenHocKyExistsAsync(_nienKhoaId, TenHocKy, _editingId))
        {
            DuplicateWarning = "Tên học kỳ này đã tồn tại trong niên khóa";
            return;
        }

        IsSaving = true;
        try
        {
            if (IsEditMode)
            {
                var currentItem = await _repository.GetByIdAsync(_editingId!.Value);
                if (currentItem == null) return;
                currentItem.TenHocKy = TenHocKy.Trim();
                await _repository.UpdateAsync(currentItem);
            }
            else
            {
                await _repository.AddAsync(new HocKy
                {
                    TenHocKy = TenHocKy.Trim(),
                    NienKhoaId = _nienKhoaId,
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