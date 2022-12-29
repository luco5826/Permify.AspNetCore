namespace Permify.AspNetCore.Model;

public class Entity
{
    public Entity(string id, string type)
    {
        Id = id;
        Type = type;
    }

    public string Id { get; set; }
    public string Type { get; set; }
}