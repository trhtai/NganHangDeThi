using System.ComponentModel;

namespace NganHangDeThi.Common.Enum;

public enum LoaiCauHoi
{
    [Description("Trắc nghiệm")]
    TracNghiemMotDapAn = 1,       // Câu hỏi trắc nghiệm 1 đáp án đúng (SingleChoice)

    [Description("Tự luận")]
    TuLuan = 2                    // Câu hỏi tự luận (Essay)
}
