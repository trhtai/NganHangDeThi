using NganHangDeThi.Data.Entities;

namespace NganHangDeThi.ViewModels.Semesters.Factories.Interfaces;

public interface ISemesterViewModelFactory
{
    SemesterViewModel Create(NienKhoa nienKhoa);
}