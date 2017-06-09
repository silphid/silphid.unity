using System;
using Silphid.Extensions;

namespace Silphid.Showzup
{
    public struct ViewInfo
    {
        public static ViewInfo Null => new ViewInfo();

        public Type ModelType { get; set; }
        public Type ViewModelType { get; set; }
        public Type ViewType { get; set; }
        public Uri Uri { get; set; }
        public VariantSet Variants { get; set; }

        public override string ToString()
        {
            return $"{ViewModelType} => {ViewType} ({Variants.ToDelimitedString(";")}) {Uri}";
        }
    }
}