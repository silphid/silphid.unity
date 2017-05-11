using System;

namespace Silphid.Showzup
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = false)]
    public class AssetAttribute : Attribute
    {
        public Uri Uri { get; }
        public string[] Variants { get; }

        public AssetAttribute(string uri, params string[] variants)
        {
            Uri = new Uri(uri);
            Variants = variants;
        }
    }
}