using NganHangDeThi.Data.Entities;

namespace NganHangDeThi.ViewModels.Curriculum.Factories.Interfaces;

public interface ICurriculumEditViewModelFactory
{
    CurriculumEditViewModel Create(int lopId, ChuongTrinhHoc? existing);
}