using NganHangDeThi.Data.Entities;
using NganHangDeThi.Data.Repositories.Interfaces;
using NganHangDeThi.ViewModels.Chapters.Factories.Interfaces;

namespace NganHangDeThi.ViewModels.Chapters.Factories;

public class ChapterEditViewModelFactory(
    IChuongRepository chuongRepository
) : IChapterEditViewModelFactory
{
    private readonly IChuongRepository _chuongRepository = chuongRepository;

    public ChapterEditViewModel Create(int monHocId, Chuong? existing)
    {
        return new ChapterEditViewModel(_chuongRepository, monHocId, existing);
    }
}