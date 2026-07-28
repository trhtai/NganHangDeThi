using NganHangDeThi.Data.Entities;

namespace NganHangDeThi.ViewModels.LopPage.Factories.Interfaces;

public interface IChinhSuaLopViewModelFactory
{
    ChinhSuaLopViewModel Create(Lop? existing);
}
