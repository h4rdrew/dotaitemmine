using Simple.DatabaseWrapper.Attributes;

namespace dotaitemmine.models.db;

public class Data
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }
    public int ItemId { get; set; }
    public decimal Price { get; set; }
    public Guid CaptureId { get; set; }
}

