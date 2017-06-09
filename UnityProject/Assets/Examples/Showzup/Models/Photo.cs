public class Photo
{
    public string Id { get; }
    public Photographer Photographer { get; }

    public Photo(string id, Photographer photographer)
    {
        Id = id;
        Photographer = photographer;
    }
}
