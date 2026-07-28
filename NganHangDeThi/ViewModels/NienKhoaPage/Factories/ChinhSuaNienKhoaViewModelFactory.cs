using NganHangDeThi.Data.Entities;
using NganHangDeThi.Data.Repositories.Interfaces;
using NganHangDeThi.ViewModels.NienKhoaPage.Factories.Interfaces;

namespace NganHangDeThi.ViewModels.NienKhoaPage.Factories;

public class ChinhSuaNienKhoaViewModelFactory(
    INienKhoaRepository nienKhoaRepository    
) : IChinhSuaNienKhoaViewModelFactory
{
    private readonly INienKhoaRepository _nienKhoaRepository = nienKhoaRepository;

    public ChinhSuaNienKhoaViewModel Create(NienKhoa? existing)
    {
        return new ChinhSuaNienKhoaViewModel(_nienKhoaRepository, existing);
    }
}
