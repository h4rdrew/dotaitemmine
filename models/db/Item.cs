using Simple.DatabaseWrapper.Attributes;

namespace dotaitemmine.models.db;

public class Item
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }
    public int ItemId { get; set; }
    public string Name { get; set; }
}

