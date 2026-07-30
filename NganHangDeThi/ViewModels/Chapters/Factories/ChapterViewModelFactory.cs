using NganHangDeThi.Data.Entities;
using NganHangDeThi.Data.Repositories.Interfaces;
using NganHangDeThi.Services.Interfaces;
using NganHangDeThi.ViewModels.Chapters.Factories.Interfaces;

namespace NganHangDeThi.ViewModels.Chapters.Factories;

public class ChapterViewModelFactory(
    IChuongRepository chuongRepository,
    IChapterEditViewModelFactory chapterEditViewModelFactory,
    IToastService toast,
    IConfirmService confirm
) : IChapterViewModelFactory
{
    private readonly IChuongRepository _chuongRepository = chuongRepository;
    private readonly IChapterEditViewModelFactory _chapterEditViewModelFactory = chapterEditViewModelFactory;
    private readonly IToastService _toast = toast;
    private readonly IConfirmService _confirm = confirm;

    public ChapterViewModel Create(MonHoc monHoc)
    {
        return new ChapterViewModel(
            monHoc,
            _chuongRepository,
            _chapterEditViewModelFactory,
            _toast,
            _confirm);
    }
}