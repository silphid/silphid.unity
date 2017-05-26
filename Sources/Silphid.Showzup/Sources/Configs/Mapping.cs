using System;
using System.Collections.Generic;
using Silphid.Extensions;

namespace Silphid.Showzup
{
    public class Mapping
    {
        public Type ViewModelType { get; }
        public Type ViewType { get; }
        public Uri Uri { get; }
        public List<string> Variants { get; }

        public Mapping(Type viewModelType, Type viewType, Uri uri, List<string> variants)
        {
            ViewModelType = viewModelType;
            ViewType = viewType;
            Uri = uri;
            Variants = variants;
        }

        public override string ToString()
        {
            return $"{ViewModelType} => {ViewType} ({Variants.ToDelimitedString(";")}) {Uri}";
        }
    }
}