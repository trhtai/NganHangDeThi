using NganHangDeThi.Data.Entities;
using NganHangDeThi.Data.Repositories.Interfaces;
using NganHangDeThi.ViewModels.Semesters.Factories.Interfaces;

namespace NganHangDeThi.ViewModels.Semesters.Factories;

public class SemesterEditViewModelFactory(
    IHocKyRepository hocKyRepository
) : ISemesterEditViewModelFactory
{
    private readonly IHocKyRepository _hocKyRepository = hocKyRepository;

    public SemesterEditViewModel Create(int nienKhoaId, HocKy? existing)
    {
        return new SemesterEditViewModel(_hocKyRepository, nienKhoaId, existing);
    }
}