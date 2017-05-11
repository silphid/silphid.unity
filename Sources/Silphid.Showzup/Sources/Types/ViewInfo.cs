using System;
using System.Collections.Generic;
using Silphid.Extensions;

namespace Silphid.Showzup
{
    public struct ViewInfo
    {
        public static ViewInfo Null => new ViewInfo();

        public Type ViewModelType { get; set; }
        public object ViewModel { get; set; }
        public Type ViewType { get; set; }
        public IView View { get; set; }
        public Uri Uri { get; set; }
        public IEnumerable<string> Variants { get; set; }

        public override string ToString()
        {
            return $"{ViewModelType} => {ViewType} ({Variants.ToDelimitedString(";")}) {Uri}";
        }
    }
}