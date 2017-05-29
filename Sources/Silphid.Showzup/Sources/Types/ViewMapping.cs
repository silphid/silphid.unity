using System;
using System.Collections.Generic;
using Silphid.Extensions;

namespace Silphid.Showzup
{
    public class ViewMapping
    {
        public Type ViewModelType { get; }
        public Type ViewType { get; }
        public Uri Resource { get; }
        public IEnumerable<string> Variants { get; }

        public ViewMapping(Type viewModelType, Type viewType, Uri resource, IEnumerable<string> variants)
        {
            ViewModelType = viewModelType;
            ViewType = viewType;
            Resource = resource;
            Variants = variants;
        }

        public override string ToString()
        {
            return $"{ViewModelType} => {ViewType} ({Variants.ToDelimitedString(";")}) {Resource}";
        }
    }
}