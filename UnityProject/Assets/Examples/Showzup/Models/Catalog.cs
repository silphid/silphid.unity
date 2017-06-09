using System.Collections.Generic;

public class Catalog
{
    public List<Photo> Photos { get; } = new List<Photo>();

    public Catalog()
    {
        Photos.AddRange(new []
        {
            new Photo("235177", new Photographer("erico-marcelino", "Erico Marcelino", "Sydney, Australia", "")), 
            new Photo("2554",   new Photographer("jeff-sheldon", "Jeff Sheldon", "Downingtown, PA", "ugmonk.com/gather")), 
            new Photo("260835", new Photographer("jonatan-pie", "Jonatan Pie", "Iceland", "")),
            new Photo("134977", new Photographer("lobostudio-hamburg", "LoboStudio Hamburg", "Hamburg", "lobostudio.de")), 
            new Photo("229776", new Photographer("maarten-deckers", "Maarten Deckers", "Leuven, Belgium", "maartendeckers.com")), 
            new Photo("274716", new Photographer("martin-jernberg", "Martin Jernberg", "Boston, MA", "martinjphoto.com")),
            new Photo("194690", new Photographer("patrick-baum", "Patrick Baum", "Frankfurt, Germany", "instagram.com/gecko81de")),
            new Photo("214954", new Photographer("paul-morris", "Paul Morris", "Manchester, UK", "flickr.com/photos/62438406@n00")), 
            new Photo("234486", new Photographer("vivek-kumar", "Vivek Kumar", "India", "vivekkumar.me"))
        });
    }
}