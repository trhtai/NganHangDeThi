using NganHangDeThi.Data.Entities;
using NganHangDeThi.Data.Repositories.Interfaces;
using NganHangDeThi.Services.Interfaces;
using NganHangDeThi.ViewModels.Subjects.Factories.Interfaces;

namespace NganHangDeThi.ViewModels.Subjects.Factories;

public class SubjectEditViewModelFactory(
    IMonHocRepository monHocRepository,
    IKhoaRepository khoaRepository,
    IDateTimeService dateTime
    ) : ISubjectEditViewModelFactory
{
    private readonly IMonHocRepository _monHocRepository = monHocRepository;
    private readonly IKhoaRepository _khoaRepository = khoaRepository;
    private readonly IDateTimeService _dateTime = dateTime;

    public SubjectEditViewModel Create(MonHoc? existing)
    {
        return new SubjectEditViewModel(
            _monHocRepository, 
            _khoaRepository, 
            _dateTime, existing);
    }
}
