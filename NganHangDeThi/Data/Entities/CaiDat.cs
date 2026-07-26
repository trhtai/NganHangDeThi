using NganHangDeThi.Data.Entities.Interfaces;

namespace NganHangDeThi.Data.Entities;

public class CaiDat : IEntity<int>
{
    public int Id { get; set; }

    // Có hiển thị dialog để user xác nhận thoát hay không?
    public bool HienThiXacNhanThoat { get; set; } = true;

    // Cho phép điều chỉnh định dạng ngày giờ
    public string DinhDangNgayGio { get; set; } = string.Empty;
}
