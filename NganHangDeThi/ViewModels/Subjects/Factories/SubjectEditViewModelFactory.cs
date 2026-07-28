using NganHangDeThi.Data.Entities;
using NganHangDeThi.Data.Repositories.Interfaces;
using NganHangDeThi.ViewModels.Subjects.Factories.Interfaces;

namespace NganHangDeThi.ViewModels.Subjects.Factories;

public class SubjectEditViewModelFactory(
    IMonHocRepository monHocRepository,
    IKhoaRepository khoaRepository
) : ISubjectEditViewModelFactory
{
    private readonly IMonHocRepository _monHocRepository = monHocRepository;
    private readonly IKhoaRepository _khoaRepository = khoaRepository;

    public SubjectEditViewModel Create(MonHoc? existing)
    {
        return new SubjectEditViewModel(
            _monHocRepository, 
            _khoaRepository,
            existing);
    }
}
