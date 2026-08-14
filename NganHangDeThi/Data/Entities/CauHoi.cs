using NganHangDeThi.Data.Entities.Enums;
using NganHangDeThi.Data.Entities.Interfaces;

namespace NganHangDeThi.Data.Entities;

/// <summary>
/// Câu hỏi trong ngân hàng câu hỏi thi. 1 câu chỉ thuộc đúng 1 <see cref="LoaiCauHoi"/>:
/// - TracNghiem: dùng navigation <see cref="DanhSachPhuongAn"/>.
/// - TuLuan: dùng navigation <see cref="DanhSachY"/> + <see cref="DiemToiDa"/>.
/// </summary>
public class CauHoi : IEntity<int>, IAuditable
{
    public int Id { get; set; }

    public LoaiCauHoi LoaiCauHoi { get; set; }

    /// <summary>Nội dung câu hỏi, rich text/HTML để giữ bold/italic/underline/sub-superscript.</summary>
    public string NoiDung { get; set; } = string.Empty;

    /// <summary>Chỉ dùng cho câu tự luận — số điểm ghi trong ngoặc ở đầu câu, VD "(2 điểm)".</summary>
    public decimal? DiemToiDa { get; set; }

    /// <summary>
    /// Nullable: null khi câu hỏi thuộc <see cref="NhomCauHoi"/> (theo quy định "không chèn mức độ" cho nhóm).
    /// </summary>
    public int? MucDoCauHoiId { get; set; }
    public MucDoCauHoi? MucDoCauHoi { get; set; }

    /// <summary>Nullable: chỉ có giá trị khi câu hỏi là câu con trong 1 nhóm dùng chung dữ liệu.</summary>
    public int? NhomCauHoiId { get; set; }
    public NhomCauHoi? NhomCauHoi { get; set; }

    /// <summary>Thứ tự &lt;n&gt; của câu con trong nhóm (khớp số thứ tự xuất hiện trong đoạn dữ liệu chung).</summary>
    public int? ThuTuTrongNhom { get; set; }

    /// <summary>Soft delete — phục vụ đối chiếu trùng lặp & audit, không xoá cứng để không vỡ tham chiếu từ DeThiCauHoi.</summary>
    public bool DaXoa { get; set; }

    // Audit (Theo dõi lịch sử).
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    // Dùng để tối ưu Search, Sort, Filter, đối chiếu trùng lặp (không dấu).
    public string NoiDungUnsign { get; set; } = string.Empty;

    // Navigation properties (Khóa ngoại & Quan hệ các bảng).
    public int ChuongId { get; set; }
    public Chuong Chuong { get; set; } = null!;

    public int? FileImportId { get; set; }
    public FileImport? FileImport { get; set; }

    public ICollection<PhuongAnTraLoi> DanhSachPhuongAn { get; set; } = [];
    public ICollection<CauHoiTuLuanY> DanhSachY { get; set; } = [];
    public ICollection<HinhAnhCauHoi> DanhSachHinhAnh { get; set; } = [];
}