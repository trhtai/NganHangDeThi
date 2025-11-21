namespace NganHangDeThi.Models;

//public record CauTraLoiRaw(string NoiDung, bool LaDapAnDung, byte ViTriGoc, bool DaoViTri, string? HinhAnh = null);

public class CauTraLoiRaw
{
    public string NoiDung { get; set; }
    public bool LaDapAnDung { get; set; }
    public byte ViTriGoc { get; set; }
    public bool DaoViTri { get; set; }
    public string? HinhAnh { get; set; }

    public CauTraLoiRaw(string noiDung, bool laDapAnDung, byte viTriGoc, bool daoViTri, string? hinhAnh = null)
    {
        NoiDung = noiDung;
        LaDapAnDung = laDapAnDung;
        ViTriGoc = viTriGoc;
        DaoViTri = daoViTri;
        HinhAnh = hinhAnh;
    }
}
