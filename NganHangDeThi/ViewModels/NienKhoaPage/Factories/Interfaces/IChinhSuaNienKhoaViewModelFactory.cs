using NganHangDeThi.Data.Entities;

namespace NganHangDeThi.ViewModels.NienKhoaPage.Factories.Interfaces;

public interface IChinhSuaNienKhoaViewModelFactory
{
    ChinhSuaNienKhoaViewModel Create(NienKhoa? existing);
}
