using NganHangDeThi.Data.Entities;
using NganHangDeThi.Data.Repositories.Interfaces;
using NganHangDeThi.ViewModels.Curriculum.Factories.Interfaces;

namespace NganHangDeThi.ViewModels.Curriculum.Factories;

public class CurriculumEditViewModelFactory(
    IChuongTrinhHocRepository chuongTrinhHocRepository
) : ICurriculumEditViewModelFactory
{
    private readonly IChuongTrinhHocRepository _chuongTrinhHocRepository = chuongTrinhHocRepository;

    public CurriculumEditViewModel Create(int lopId, ChuongTrinhHoc? existing)
    {
        return new CurriculumEditViewModel(_chuongTrinhHocRepository, lopId, existing);
    }
}