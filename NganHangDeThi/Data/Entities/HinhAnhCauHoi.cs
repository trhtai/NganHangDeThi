using NganHangDeThi.Data.Entities.Enums;
using NganHangDeThi.Data.Entities.Interfaces;

namespace NganHangDeThi.Data.Entities;

/// <summary>
/// Ảnh trong câu hỏi/phương án trả lời/đoạn dữ liệu chung. Chỉ gắn với đúng 1 trong 3 khoá ngoại
/// bên dưới (còn lại để null). Cho phép nhiều ảnh trong cùng 1 vị trí (khớp quyết định F2).
/// Chỉ chấp nhận ảnh có Wrap Text = "In line with text" — nếu không, câu hỏi bị từ chối import
/// (xem <see cref="LoiImportCauHoi"/>) thay vì cố tự xử lý.
/// </summary>
public class HinhAnhCauHoi : IEntity<int>
{
    public int Id { get; set; }

    public string DuongDanFile { get; set; } = string.Empty;

    public ViTriHinhAnh ViTri { get; set; }

    public int ThuTu { get; set; }

    // Navigation properties (Khóa ngoại & Quan hệ các bảng) — chỉ đúng 1 trong 3 có giá trị.
    public int? CauHoiId { get; set; }
    public CauHoi? CauHoi { get; set; }

    public int? PhuongAnTraLoiId { get; set; }
    public PhuongAnTraLoi? PhuongAnTraLoi { get; set; }

    public int? NhomCauHoiId { get; set; }
    public NhomCauHoi? NhomCauHoi { get; set; }
}