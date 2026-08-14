using NganHangDeThi.Data.Entities.Enums;
using NganHangDeThi.Data.Entities.Interfaces;

namespace NganHangDeThi.Data.Entities;

/// <summary>
/// 1 lượt import file docx do phòng khảo thí thực hiện. Hỗ trợ import từng phần: câu đúng được lưu
/// vào CauHoi/NhomCauHoi, câu lỗi ghi vào <see cref="LoiImportCauHoi"/> để sửa và import lại riêng.
/// </summary>
public class FileImport : IEntity<int>, IAuditable
{
    public int Id { get; set; }

    public string TenFileGoc { get; set; } = string.Empty;

    /// <summary>Đường dẫn lưu lại file docx gốc, phục vụ đối chiếu/tải lại khi cần.</summary>
    public string DuongDanLuuTru { get; set; } = string.Empty;

    public TrangThaiImport TrangThai { get; set; }

    public int TongSoCauNhanDien { get; set; }
    public int SoCauThanhCong { get; set; }
    public int SoCauLoi { get; set; }

    /// <summary>Tên/tài khoản nhân viên phòng khảo thí thực hiện import.</summary>
    public string NguoiImport { get; set; } = string.Empty;

    // Audit (Theo dõi lịch sử).
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    // Navigation properties (Khóa ngoại & Quan hệ các bảng).
    // Môn/Chương do người dùng chọn tay trên UI trước khi import (không suy luận từ tên file).
    public int MonHocId { get; set; }
    public MonHoc MonHoc { get; set; } = null!;

    public int ChuongId { get; set; }
    public Chuong Chuong { get; set; } = null!;

    public ICollection<LoiImportCauHoi> DanhSachLoi { get; set; } = [];
    public ICollection<CauHoi> DanhSachCauHoiDaLuu { get; set; } = [];
    public ICollection<NhomCauHoi> DanhSachNhomDaLuu { get; set; } = [];
}