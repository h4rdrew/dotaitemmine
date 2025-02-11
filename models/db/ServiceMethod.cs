using Simple.DatabaseWrapper.Attributes;

namespace dotaitemmine.models.db;

public class ServiceMethod
{
    [PrimaryKey, AutoIncrement]
    public ServiceType ServiceType { get; set; }
}

public enum ServiceType
{
    UNKNOW = 0,
    STEAM = 1,
    DMARKET = 2,
}
