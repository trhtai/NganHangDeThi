using NganHangDeThi.Data.Entities;

namespace NganHangDeThi.ViewModels.Chapters.Factories.Interfaces;

public interface IChapterViewModelFactory
{
    ChapterViewModel Create(MonHoc monHoc);
}
