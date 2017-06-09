public class Photographer
{
    public string Id { get; }
    public string Name { get; }
    public string Location { get; }
    public string Website { get; }

    public Photographer(string id, string name, string location, string website)
    {
        Id = id;
        Name = name;
        Location = location;
        Website = website;
    }
}