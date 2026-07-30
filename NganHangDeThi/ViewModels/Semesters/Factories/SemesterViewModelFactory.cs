using NganHangDeThi.Data.Entities;
using NganHangDeThi.Data.Repositories.Interfaces;
using NganHangDeThi.Services.Interfaces;
using NganHangDeThi.ViewModels.Semesters.Factories.Interfaces;

namespace NganHangDeThi.ViewModels.Semesters.Factories;

public class SemesterViewModelFactory(
    IHocKyRepository hocKyRepository,
    ISemesterEditViewModelFactory semesterEditViewModelFactory,
    IToastService toast,
    IConfirmService confirm
) : ISemesterViewModelFactory
{
    private readonly IHocKyRepository _hocKyRepository = hocKyRepository;
    private readonly ISemesterEditViewModelFactory _semesterEditViewModelFactory = semesterEditViewModelFactory;
    private readonly IToastService _toast = toast;
    private readonly IConfirmService _confirm = confirm;

    public SemesterViewModel Create(NienKhoa nienKhoa)
    {
        return new SemesterViewModel(
            nienKhoa,
            _hocKyRepository,
            _semesterEditViewModelFactory,
            _toast,
            _confirm);
    }
}
