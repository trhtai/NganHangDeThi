using System.ComponentModel;

namespace NganHangDeThi.Common.Enum;

public enum LoaiCauHoi
{
    [Description("Trắc nghiệm 1 đáp án")]
    TracNghiemMotDapAn = 1,

    [Description("Tự luận")]
    TuLuan = 2,

    [Description("Trắc nghiệm nhiều đáp án")]
    TracNghiemNhieuDapAn = 3,

    //[Description("Đúng sai")]
    //DungSai = 4,

    [Description("Điền khuyết")]
    DienKhuyet = 5,

    [Description("Chùm - TN 1 đáp án")]
    ChumTracNghiemMotDapAn = 6,

    [Description("Chùm - Tự luận")]
    ChumTuLuan = 7,

    [Description("Chùm - TN nhiều đáp án")]
    ChumTracNghiemNhieuDapAn = 8
}
