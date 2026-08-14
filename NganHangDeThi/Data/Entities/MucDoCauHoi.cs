using NganHangDeThi.Data.Entities.Interfaces;

namespace NganHangDeThi.Data.Entities;

/// <summary>
/// Lookup mức độ câu hỏi. Seed sẵn 3 dòng theo tài liệu: MaSo=2 (Dễ), 3 (Trung bình), 4 (Khó).
/// Thiết kế dạng bảng (không hard-code enum) để khách hàng thêm mức độ mới trong tương lai
/// mà không cần đổi schema/code.
/// </summary>
public class MucDoCauHoi : IEntity<int>
{
    public int Id { get; set; }

    /// <summary>Giá trị X trong tag [&lt;O D=`X`&gt;] của tài liệu gốc (2, 3, 4, ...).</summary>
    public int MaSo { get; set; }

    public string TenMucDo { get; set; } = string.Empty; // "Dễ", "Trung bình", "Khó"

    public int ThuTu { get; set; }

    // Navigation properties
    public ICollection<CauHoi> DanhSachCauHoi { get; set; } = [];
}