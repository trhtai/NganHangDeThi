using NganHangDeThi.Data.Entities;

namespace NganHangDeThi.ViewModels.Semesters.Factories.Interfaces;

public interface ISemesterEditViewModelFactory
{
    SemesterEditViewModel Create(int nienKhoaId, HocKy? existing);
}