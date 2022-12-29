namespace Permify.AspNetCore.Model;

public class Subject
{
    public Subject(string id, string type, string relation = "")
    {
        Id = id;
        Type = type;
        Relation = relation;
    }

    public string Id { get; init; }
    public string Type { get; init; }
    public string? Relation { get; set; }
}