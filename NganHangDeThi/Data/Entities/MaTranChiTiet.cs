using NganHangDeThi.Data.Entities.Enums;
using NganHangDeThi.Data.Entities.Interfaces;

namespace NganHangDeThi.Data.Entities;

/// <summary>
/// 1 dòng cấu hình trong ma trận đề thi: số lượng câu hỏi cần rút từ (Chương, Mức độ), có thể lọc
/// thêm theo Loại câu hỏi nếu môn học có cả trắc nghiệm lẫn tự luận trong cùng đề.
/// </summary>
public class MaTranChiTiet : IEntity<int>
{
    public int Id { get; set; }

    public int SoLuongCau { get; set; }

    /// <summary>null = không phân biệt loại câu hỏi (lấy cả trắc nghiệm lẫn tự luận).</summary>
    public LoaiCauHoi? LoaiCauHoi { get; set; }

    // Navigation properties (Khóa ngoại & Quan hệ các bảng).
    public int MaTranDeThiId { get; set; }
    public MaTranDeThi MaTranDeThi { get; set; } = null!;

    public int ChuongId { get; set; }
    public Chuong Chuong { get; set; } = null!;

    public int MucDoCauHoiId { get; set; }
    public MucDoCauHoi MucDoCauHoi { get; set; } = null!;
}