using System.Linq;
using System.Text;
using Silphid.Loadzup;
using UnityEngine;

public class TextureConverter : IConverter
{
    private readonly string[] _imageMediaTypes =
    {
        "image/png",
        "image/jpeg",
        "application/octet-stream"
    };

    public bool Supports<T>(byte[] bytes, ContentType contentType) =>
        _imageMediaTypes.Contains(contentType.MediaType);

    public T Convert<T>(byte[] bytes, ContentType contentType, Encoding encoding)
    {
        var texture = new Texture2D(2, 2, TextureFormat.RGBA32, false, false);
        texture.LoadImage(bytes);
        return (T) (object) texture;
    }
}