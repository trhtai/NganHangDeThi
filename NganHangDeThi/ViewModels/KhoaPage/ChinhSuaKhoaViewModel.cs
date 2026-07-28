using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using NganHangDeThi.Data.Entities;
using NganHangDeThi.Data.Repositories.Interfaces;
using System.ComponentModel.DataAnnotations;

namespace NganHangDeThi.ViewModels.KhoaPage;

public partial class ChinhSuaKhoaViewModel : ObservableValidator
{
    private readonly IKhoaRepository _repository;
    private readonly int? _editingId;

    public bool IsEditMode => _editingId.HasValue;
    public string DialogTitle => IsEditMode ? "Sửa khoa" : "Thêm khoa";

    // TenKhoa ----------
    [ObservableProperty]
    [NotifyDataErrorInfo]
    [NotifyPropertyChangedFor(nameof(TenKhoaLength))]
    [Required(ErrorMessage = "Tên khoa không được để trống")]
    [StringLength(200, MinimumLength = 2, ErrorMessage = "Tên khoa phải từ 2-200 ký tự")]
    [RegularExpression(@"^[\p{L}0-9\-_ ]+$", ErrorMessage = "Tên khoa hợp lệ chỉ gồm chữ, số, khoảng trắng, - hoặc _")]
    private string tenKhoa = string.Empty;

    public int TenKhoaLength => TenKhoa?.Length ?? 0;

    [ObservableProperty]
    public string? duplicateWarning;
    // -------------------

    public bool? DialogResult { get; private set; }
    public event Action? RequestClose;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(SaveCommand))]
    private bool isSaving;

    public ChinhSuaKhoaViewModel(
        IKhoaRepository repository,
        Khoa? khoa)
    {
        _repository = repository;

        if (khoa is not null)
        {
            _editingId = khoa.Id;
            tenKhoa = khoa.TenKhoa;
        }
    }

    async partial void OnTenKhoaChanged(string value)
    {
        DuplicateWarning = null;
        ValidateAllProperties();
        if (HasErrors || string.IsNullOrWhiteSpace(value)) return;

        var snapshot = value;
        await Task.Delay(300);
        if (snapshot != TenKhoa) return;

        var exists = await _repository.TenKhoaExistsAsync(snapshot, _editingId);

        if (snapshot == TenKhoa && exists)
        {
            DuplicateWarning = "Tên khoa này đã tồn tại";
        }
    }

    private bool CanSave() => !IsSaving;

    // Validation và vô hiệu hóa nút tránh span click.
    [RelayCommand(CanExecute = nameof(CanSave))]
    public async Task SaveAsync()
    {
        ValidateAllProperties();
        if (HasErrors) return;

        if (await _repository.TenKhoaExistsAsync(TenKhoa, _editingId))
        {
            DuplicateWarning = "Tên khoa này đã tồn tại";
            return;
        }

        IsSaving = true;
        try
        {
            if (IsEditMode)
            {
                var currentItem = await _repository.GetByIdAsync(_editingId!.Value);
                if (currentItem == null) return;
                currentItem.TenKhoa = TenKhoa.Trim();
                await _repository.UpdateAsync(currentItem);
            }
            else
            {
                await _repository.AddAsync(new Khoa
                {
                    TenKhoa = TenKhoa.Trim()
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
