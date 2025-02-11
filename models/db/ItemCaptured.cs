using Simple.DatabaseWrapper.Attributes;

namespace dotaitemmine.models.db;

public class ItemCaptured
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }
    public Guid CaptureId { get; set; }
    public ServiceType ServiceType { get; set; }
    public DateTime DateTime { get; set; }
    public decimal ExchangeRate { get; set; }
}

