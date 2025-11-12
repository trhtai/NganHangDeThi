using NganHangDeThi.Common.Interfaces;
using System.ComponentModel.DataAnnotations;

namespace NganHangDeThi.Data.Entity;

public class MonHoc : IEntity<int>
{
    [Key]
    public int Id { get; set; }
    public string TenMon { get; set; } = string.Empty;

    public ICollection<Chuong> DsChuong { get; set; } = [];
}
