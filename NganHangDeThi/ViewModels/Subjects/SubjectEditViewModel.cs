using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using NganHangDeThi.Data.Entities;
using NganHangDeThi.Data.Repositories.Interfaces;
using NganHangDeThi.Services.Interfaces;
using System.ComponentModel.DataAnnotations;

namespace NganHangDeThi.ViewModels.Subjects;

public partial class SubjectEditViewModel : ObservableValidator
{
    private readonly IMonHocRepository _repository;
    private readonly IKhoaRepository _khoaRepository;
    private readonly IDateTimeService _dateTime;
    private readonly int? _editingId;

    public bool IsEditMode => _editingId.HasValue;
    public string DialogTitle => IsEditMode ? "Sửa môn học" : "Thêm môn học";

    // TenMonHoc ----------
    [ObservableProperty]
    [NotifyDataErrorInfo]
    [NotifyPropertyChangedFor(nameof(TenMonHocLength))]
    [Required(ErrorMessage = "Tên môn học không được để trống")]
    [StringLength(200, MinimumLength = 2, ErrorMessage = "Tên môn học phải từ 2-200 ký tự")]
    [RegularExpression(@"^[\p{L}0-9\-_ ]+$", ErrorMessage = "Tên môn học hợp lệ chỉ gồm chữ, số, khoảng trắng, - hoặc _")]
    private string tenMonHoc = string.Empty;

    public int TenMonHocLength => TenMonHoc?.Length ?? 0;

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

    public SubjectEditViewModel(
        IMonHocRepository repository, 
        IKhoaRepository khoaRepository,
        IDateTimeService dateTime, 
        MonHoc? monHoc)
    {
        _repository = repository;
        _khoaRepository = khoaRepository;
        _dateTime = dateTime;

        if (monHoc is not null)
        {
            _editingId = monHoc.Id;
            tenMonHoc = monHoc.TenMon;
            selectedKhoaId = monHoc.KhoaId;
        }

        LoadDsKhoaAsync();
    }

    private async void LoadDsKhoaAsync()
    {
        var khoaList = await _khoaRepository.GetAllAsync();
        DanhSachKhoa = khoaList;

        if (DanhSachKhoa.Count > 0)
        {
            // Nếu là trạng thái Thêm mới (IsEditMode = false) và chưa chọn Khoa nào,
            // ta tự động gán SelectedKhoaId bằng Id của phần tử đầu tiên trong danh sách.
            if (!IsEditMode && SelectedKhoaId == null)
            {
                SelectedKhoaId = DanhSachKhoa[0].Id;
            }
        }
    }

    async partial void OnTenMonHocChanged(string value)
    {
        // Clear warning.
        DuplicateWarning = null;
        // Kích hoạt tất cả rule bên trên.
        ValidateAllProperties();
        // Nếu có lỗi thì return luôn, khỏi tốn chi phí đi xuống gọi db.
        if (HasErrors || string.IsNullOrWhiteSpace(value)) return;

        // THUẬT TOÁN DEBOUNCE (CHỐNG DỘI DB) 
        // Vấn đề: Nếu người dùng gõ chữ CNTT, sự kiện sẽ nhảy 4 lần liên tục: C, CN, CNT, CNTT.
        // Nếu không chặn lại, ta sẽ bắn 4 câu query liên tiếp vào Database trong vòng nửa giây
        // -> Quá tải Database.
        var snapshot = value; // Lấy giá trị người dùng vừa gõ ngay tại khoảng khắc này,
                              // vd mới gõ mỗi chữ "C"
        await Task.Delay(300); // Tạm dừng tiến trình này lại 300 mili-giây
                               // await giúp giao diện không bị đơ
        if (snapshot != TenMonHoc) return; // Sau 300 mili-giây tiến trình thức dây và kiểm tra
                                           // Nếu trong 300 mili-giây đó user đã gõ thêm cái gì đó
                                           // Tức biến TenMonHoc lúc này đã là "CN..."
                                           // Hủy bỏ tiến trình này, return để tiến trình không đi tiếp và gọi db
                                           // vì giá trị TenMonHoc lúc này đã khác.
                                           // Nghĩa là chỉ khi người dùng ngừng gõ ít nhất 300ms
                                           // thì giá trị đó là giá trị cuối và cần gọi db để kiểm tra

        var exists = await _repository.TenMonHocExistsAsync(snapshot, _editingId);
        
        // Tại sao phải kiểm tra (snapshot == TenMonHoc) một lần nữa?
        // Việc gọi db cũng tốn thời gian, nhỡ đâu trong lúc đang gọi db
        // thì user lại gõ thêm/xóa gì đó => dữ liệu đem đi gọi db != với dữ liệu đg hiện trên TextBox
        if (snapshot == TenMonHoc && exists)
        {
            DuplicateWarning = "Tên môn học này đã tồn tại";
        }
    }

    private bool CanSave() => !IsSaving;

    // Validation và vô hiệu hóa nút tránh span click.
    [RelayCommand(CanExecute = nameof(CanSave))]
    public async Task SaveAsync()
    {
        ValidateAllProperties();
        if (HasErrors) return;

        if (await _repository.TenMonHocExistsAsync(TenMonHoc, _editingId))
        {
            DuplicateWarning = "Tên môn học này đã tồn tại";
            return;
        }

        IsSaving = true;
        try
        {
            if (IsEditMode)
            {
                var currentItem = await _repository.GetByIdAsync(_editingId!.Value);
                if (currentItem == null) return;
                currentItem.TenMon = TenMonHoc.Trim();
                currentItem.KhoaId = SelectedKhoaId!.Value;
                await _repository.UpdateAsync(currentItem);
            }
            else
            {
                await _repository.AddAsync(new MonHoc 
                { 
                    TenMon = TenMonHoc.Trim(),
                    KhoaId = SelectedKhoaId!.Value,
                    CreatedAt = _dateTime.GetVietnamTime(),
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
