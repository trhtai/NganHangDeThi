using NganHangDeThi.Data.Entities;

namespace NganHangDeThi.ViewModels.KhoaPage.Factories.Interfaces;

public interface IChinhSuaKhoaViewModelFactory
{
    ChinhSuaKhoaViewModel Create(Khoa? existing);
}
