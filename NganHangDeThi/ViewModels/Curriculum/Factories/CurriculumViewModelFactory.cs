using NganHangDeThi.Data.Entities;
using NganHangDeThi.Data.Repositories.Interfaces;
using NganHangDeThi.Services.Interfaces;
using NganHangDeThi.ViewModels.Curriculum.Factories.Interfaces;

namespace NganHangDeThi.ViewModels.Curriculum.Factories;

public class CurriculumViewModelFactory(
    IChuongTrinhHocRepository chuongTrinhHocRepository,
    ICurriculumEditViewModelFactory curriculumEditViewModelFactory,
    IToastService toast,
    IConfirmService confirm
) : ICurriculumViewModelFactory
{
    private readonly IChuongTrinhHocRepository _chuongTrinhHocRepository = chuongTrinhHocRepository;
    private readonly ICurriculumEditViewModelFactory _curriculumEditViewModelFactory = curriculumEditViewModelFactory;
    private readonly IToastService _toast = toast;
    private readonly IConfirmService _confirm = confirm;

    public CurriculumViewModel Create(Lop lop)
    {
        return new CurriculumViewModel(
            lop,
            _chuongTrinhHocRepository,
            _curriculumEditViewModelFactory,
            _toast,
            _confirm);
    }
}