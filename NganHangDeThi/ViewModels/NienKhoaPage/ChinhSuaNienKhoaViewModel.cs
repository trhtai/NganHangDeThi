using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using NganHangDeThi.Data.Entities;
using NganHangDeThi.Data.Repositories.Interfaces;
using System.ComponentModel.DataAnnotations;

namespace NganHangDeThi.ViewModels.NienKhoaPage;

public partial class ChinhSuaNienKhoaViewModel : ObservableValidator
{
    private readonly INienKhoaRepository _repository;
    private readonly int? _editingId;

    public bool IsEditMode => _editingId.HasValue;
    public string DialogTitle => IsEditMode ? "Sửa niên khóa" : "Thêm niên khóa";

    // TenNienKhoa ----------
    [ObservableProperty]
    [NotifyDataErrorInfo]
    [NotifyPropertyChangedFor(nameof(TenNienKhoaLength))]
    [Required(ErrorMessage = "Tên niên khóa không được để trống")]
    [StringLength(50, MinimumLength = 2, ErrorMessage = "Tên niên khóa phải từ 2-50 ký tự")]
    [RegularExpression(@"^[0-9\-]+$", ErrorMessage = "Niên khóa chỉ được chứa chữ số và dấu gạch ngang (-).")]
    private string tenNienKhoa = string.Empty;

    public int TenNienKhoaLength => TenNienKhoa?.Length ?? 0;

    [ObservableProperty]
    public string? duplicateWarning;
    // -------------------

    public bool? DialogResult { get; private set; }
    public event Action? RequestClose;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(SaveCommand))]
    private bool isSaving;

    public ChinhSuaNienKhoaViewModel(
        INienKhoaRepository repository,
        NienKhoa? nienKhoa)
    {
        _repository = repository;

        if (nienKhoa is not null)
        {
            _editingId = nienKhoa.Id;
            tenNienKhoa = nienKhoa.TenNienKhoa;
        }
    }

    // live-search
    async partial void OnTenNienKhoaChanged(string value)
    {
        DuplicateWarning = null;
        ValidateAllProperties();
        if (HasErrors || string.IsNullOrWhiteSpace(value)) return;

        var snapshot = value;
        await Task.Delay(300);
        if (snapshot != TenNienKhoa) return;

        var exists = await _repository.TenNienKhoaExistsAsync(snapshot, _editingId);

        if (snapshot == TenNienKhoa && exists)
        {
            DuplicateWarning = "Niên khóa này đã tồn tại";
        }
    }

    private bool CanSave() => !IsSaving;

    // Validation và vô hiệu hóa nút tránh span click.
    [RelayCommand(CanExecute = nameof(CanSave))]
    public async Task SaveAsync()
    {
        ValidateAllProperties();
        if (HasErrors) return;

        if (await _repository.TenNienKhoaExistsAsync(TenNienKhoa, _editingId))
        {
            DuplicateWarning = "Niên khóa này đã tồn tại";
            return;
        }

        IsSaving = true;
        try
        {
            if (IsEditMode)
            {
                var currentItem = await _repository.GetByIdAsync(_editingId!.Value);
                if (currentItem == null) return;
                currentItem.TenNienKhoa = TenNienKhoa.Trim();
                await _repository.UpdateAsync(currentItem);
            }
            else
            {
                await _repository.AddAsync(new NienKhoa
                {
                    TenNienKhoa = TenNienKhoa.Trim()
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
