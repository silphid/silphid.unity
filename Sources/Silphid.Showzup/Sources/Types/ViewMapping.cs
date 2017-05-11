using System;
using System.Collections.Generic;
using Silphid.Extensions;

namespace Silphid.Showzup
{
    public class ViewMapping
    {
        public Type ViewModelType { get; }
        public Type ViewType { get; }
        public Uri Uri { get; }
        public IEnumerable<string> Variants { get; }

        public ViewMapping(Type viewModelType, Type viewType, Uri uri, IEnumerable<string> variants)
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