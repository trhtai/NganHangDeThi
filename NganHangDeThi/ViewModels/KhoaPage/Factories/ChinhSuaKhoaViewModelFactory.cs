using NganHangDeThi.Data.Entities;
using NganHangDeThi.Data.Repositories.Interfaces;
using NganHangDeThi.ViewModels.KhoaPage.Factories.Interfaces;

namespace NganHangDeThi.ViewModels.KhoaPage.Factories;

public class ChinhSuaKhoaViewModelFactory(
    IKhoaRepository khoaRepository
) : IChinhSuaKhoaViewModelFactory
{
    private readonly IKhoaRepository _khoaRepository = khoaRepository;

    public ChinhSuaKhoaViewModel Create(Khoa? existing)
    {
        return new ChinhSuaKhoaViewModel(_khoaRepository, existing);
    }
}
