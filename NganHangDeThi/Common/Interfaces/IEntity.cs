namespace NganHangDeThi.Common.Interfaces;

public interface IEntity<Tkey>
{
    Tkey Id { get; set; }
}
