using NganHangDeThi.Data.Entities;

namespace NganHangDeThi.ViewModels.Chapters.Factories.Interfaces;

public interface IChapterEditViewModelFactory
{
    ChapterEditViewModel Create(int monHocId, Chuong? existing);
}
