using System.Collections.Generic;

namespace dotaitemmine.models;

public class ConfigJson
{
    public List<string> Items { get; set; } = [];
    public string DbPath { get; set; } = String.Empty;
    public List<int> ItemIds { get; set; } = [];
}
