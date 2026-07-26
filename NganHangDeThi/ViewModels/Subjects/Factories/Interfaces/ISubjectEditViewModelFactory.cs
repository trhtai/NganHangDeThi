using NganHangDeThi.Data.Entities;

namespace NganHangDeThi.ViewModels.Subjects.Factories.Interfaces;

public interface ISubjectEditViewModelFactory
{
    SubjectEditViewModel Create(MonHoc? existing);
}
