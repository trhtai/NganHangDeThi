using NganHangDeThi.Common.Enum;
using NganHangDeThi.Common.Interfaces;
using System.ComponentModel.DataAnnotations;

namespace NganHangDeThi.Data.Entity;

public class LopHoc : IEntity<int>
{
    [Key]
    public int Id { get; set; }
    public string MaLop { get; set; } = string.Empty; // DH20TIN01.
    public DateOnly NgayBatDau { get; set; }
    public DateOnly NgayKetThuc { get; set; }
    public TrangThaiLopHoc TrangThai { get; set; } = TrangThaiLopHoc.DangHoc;
    public string NamHoc { get; set; } = string.Empty; // 2020-2024.
    public string GVCN { get; set; } = string.Empty; // Huynh Vo Huu Tri.
    public DateTime CreatedAt { get; set; }

    public int KhoaId { get; set; } // Nullable để tránh lỗi dữ liệu cũ chưa có Khoa
    public Khoa Khoa { get; set; }

    public ICollection<MonHocThuocLop> DsMonHoc { get; set; } = [];
    public ICollection<DeThi> DsDeThi { get; set; } = [];
}
