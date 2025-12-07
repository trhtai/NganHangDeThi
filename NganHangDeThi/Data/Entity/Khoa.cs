using NganHangDeThi.Common.Interfaces;
using System.ComponentModel.DataAnnotations;

namespace NganHangDeThi.Data.Entity;

public class Khoa : IEntity<int>
{
    [Key]
    public int Id { get; set; }
    public string MaKhoa { get; set; } = string.Empty; // VD: CNTT
    public string TenKhoa { get; set; } = string.Empty; // VD: Công nghệ thông tin

    // Quan hệ: Một khoa có nhiều lớp
    public ICollection<LopHoc> DsLopHoc { get; set; } = new List<LopHoc>();

    // Quan hệ: Một khoa quản lý nhiều môn học
    public ICollection<MonHoc> DsMonHoc { get; set; } = new List<MonHoc>();
}