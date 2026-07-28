using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using NganHangDeThi.Data.Entities;
using NganHangDeThi.Data.Repositories.Interfaces;
using System.ComponentModel.DataAnnotations;

namespace NganHangDeThi.ViewModels.LopPage;

public partial class ChinhSuaLopViewModel : ObservableValidator
{
    private readonly ILopRepository _repository;
    private readonly IKhoaRepository _khoaRepository;
    private readonly int? _editingId;

    public bool IsEditMode => _editingId.HasValue;
    public string DialogTitle => IsEditMode ? "Sửa lớp học" : "Thêm lớp học";

    // MaLop ----------
    [ObservableProperty]
    [NotifyDataErrorInfo]
    [NotifyPropertyChangedFor(nameof(MaLopLength))]
    [Required(ErrorMessage = "Mã lớp học không được để trống")]
    [StringLength(20, MinimumLength = 2, ErrorMessage = "Mã lớp học phải từ 2-20 ký tự")]
    [RegularExpression(@"^[\p{L}0-9\-_ ]+$", ErrorMessage = "Mã lớp học hợp lệ chỉ gồm chữ, số, khoảng trắng, - hoặc _")]
    private string maLop = string.Empty;

    public int MaLopLength => MaLop?.Length ?? 0;

    [ObservableProperty]
    public string? duplicateWarning;
    // -------------------

    // Khoa ----------
    [ObservableProperty]
    private List<Khoa> danhSachKhoa = [];

    [ObservableProperty]
    [NotifyDataErrorInfo]
    [Required(ErrorMessage = "Vui lòng chọn khoa quản lý")]
    private int? selectedKhoaId;
    // ---------------

    public bool? DialogResult { get; private set; }
    public event Action? RequestClose;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(SaveCommand))]
    private bool isSaving;

    public ChinhSuaLopViewModel(
        ILopRepository repository,
        IKhoaRepository khoaRepository,
        Lop? lop)
    {
        _repository = repository;
        _khoaRepository = khoaRepository;

        if (lop is not null)
        {
            _editingId = lop.Id;
            maLop = lop.MaLop;
            selectedKhoaId = lop.KhoaId;
        }

        LoadDsKhoaAsync();
    }

    private async void LoadDsKhoaAsync()
    {
        var khoaList = await _khoaRepository.GetAllAsync();
        DanhSachKhoa = khoaList;

        if (DanhSachKhoa.Count > 0)
        {
            if (!IsEditMode && SelectedKhoaId == null)
            {
                SelectedKhoaId = DanhSachKhoa[0].Id;
            }
        }
    }

    async partial void OnMaLopChanged(string value)
    {
        DuplicateWarning = null;
        ValidateAllProperties();
        if (HasErrors || string.IsNullOrWhiteSpace(value)) return;

        var snapshot = value;
        await Task.Delay(300);
        if (snapshot != MaLop) return;

        var exists = await _repository.MaLopExistsAsync(snapshot, _editingId);

        if (snapshot == MaLop && exists)
        {
            DuplicateWarning = "Mã lớp học này đã tồn tại";
        }
    }

    private bool CanSave() => !IsSaving;

    // Validation và vô hiệu hóa nút tránh spam click.
    [RelayCommand(CanExecute = nameof(CanSave))]
    public async Task SaveAsync()
    {
        ValidateAllProperties();
        if (HasErrors) return;

        if (await _repository.MaLopExistsAsync(MaLop, _editingId))
        {
            DuplicateWarning = "Mã lớp học này đã tồn tại";
            return;
        }

        IsSaving = true;
        try
        {
            if (IsEditMode)
            {
                var currentItem = await _repository.GetByIdAsync(_editingId!.Value);
                if (currentItem == null) return;
                currentItem.MaLop = MaLop.Trim();
                currentItem.KhoaId = SelectedKhoaId!.Value;
                await _repository.UpdateAsync(currentItem);
            }
            else
            {
                await _repository.AddAsync(new Lop
                {
                    MaLop = MaLop.Trim(),
                    KhoaId = SelectedKhoaId!.Value,
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
