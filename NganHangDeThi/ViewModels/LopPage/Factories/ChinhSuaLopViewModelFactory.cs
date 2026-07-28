using NganHangDeThi.Data.Entities;
using NganHangDeThi.Data.Repositories.Interfaces;
using NganHangDeThi.ViewModels.LopPage.Factories.Interfaces;

namespace NganHangDeThi.ViewModels.LopPage.Factories;

public class ChinhSuaLopViewModelFactory(
    ILopRepository lopRepository,
    IKhoaRepository khoaRepository
) : IChinhSuaLopViewModelFactory
{
    private readonly ILopRepository _lopRepository = lopRepository;
    private readonly IKhoaRepository _khoaRepository = khoaRepository;

    public ChinhSuaLopViewModel Create(Lop? existing)
    {
        return new ChinhSuaLopViewModel(
            _lopRepository,
            _khoaRepository,
            existing);
    }
}
