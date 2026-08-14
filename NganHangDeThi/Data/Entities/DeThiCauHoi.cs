using NganHangDeThi.Data.Entities.Interfaces;

namespace NganHangDeThi.Data.Entities;

/// <summary>
/// Liên kết 1 câu hỏi cụ thể vào 1 đề thi, kèm thứ tự hiển thị và thứ tự phương án đã trộn
/// (áp dụng logic hoán vị dựa trên cờ PhuongAnTraLoi.KhongHoanVi — xem quyết định mục I3).
/// </summary>
public class DeThiCauHoi : IEntity<int>
{
    public int Id { get; set; }

    /// <summary>Số thứ tự câu trong đề, VD "Câu 1", "Câu 2"...</summary>
    public int ThuTuTrongDe { get; set; }

    /// <summary>
    /// Thứ tự các PhuongAnTraLoi.Id sau khi trộn, lưu dạng chuỗi phân tách dấu phẩy (VD "12,10,13,11").
    /// null nếu là câu tự luận.
    /// </summary>
    public string? ThuTuPhuongAnDaTron { get; set; }

    // Navigation properties (Khóa ngoại & Quan hệ các bảng).
    public int DeThiId { get; set; }
    public DeThi DeThi { get; set; } = null!;

    public int CauHoiId { get; set; }
    public CauHoi CauHoi { get; set; } = null!;
}