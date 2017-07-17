using System.Text;
using Silphid.Loadzup;
using UnityEngine;

public class JsonConverter : IConverter
{
    public bool Supports<T>(byte[] bytes, ContentType contentType) =>
        contentType.MediaType == KnownMediaType.ApplicationJson;

    public T Convert<T>(byte[] bytes, ContentType contentType, Encoding encoding)
    {
        return JsonUtility.FromJson<T>(encoding.GetString(bytes));
    }
}