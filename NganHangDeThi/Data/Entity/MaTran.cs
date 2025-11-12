using NganHangDeThi.Common.Interfaces;
using System.ComponentModel.DataAnnotations;

namespace NganHangDeThi.Data.Entity;

public class MaTran : IEntity<int>
{
    [Key]
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public DateTime ThoiGianCapNhatGanNhat { get; set; } = DateTime.UtcNow;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<DeThi> DsDeThi { get; set; } = [];
    public ICollection<ChiTietMaTran> DsChiTietMaTran { get; set; } = [];
}
