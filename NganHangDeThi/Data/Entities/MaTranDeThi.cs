using NganHangDeThi.Data.Entities.Interfaces;

namespace NganHangDeThi.Data.Entities;

/// <summary>
/// Ma trận đề thi: quy định số lượng câu hỏi cần rút theo từng (Chương, Mức độ, Loại câu hỏi).
/// VD: 3 câu dễ + 5 câu trung bình + 2 câu khó thuộc Chương 1.
/// </summary>
public class MaTranDeThi : IEntity<int>, IAuditable
{
    public int Id { get; set; }

    public string TenMaTran { get; set; } = string.Empty;

    // Audit (Theo dõi lịch sử).
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    // Navigation properties (Khóa ngoại & Quan hệ các bảng).
    public int MonHocId { get; set; }
    public MonHoc MonHoc { get; set; } = null!;

    public ICollection<MaTranChiTiet> DanhSachChiTiet { get; set; } = [];
    public ICollection<DeThi> DanhSachDeThi { get; set; } = [];
}