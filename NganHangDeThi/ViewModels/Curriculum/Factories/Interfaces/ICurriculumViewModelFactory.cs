using NganHangDeThi.Data.Entities;

namespace NganHangDeThi.ViewModels.Curriculum.Factories.Interfaces;

public interface ICurriculumViewModelFactory
{
    CurriculumViewModel Create(Lop lop);
}