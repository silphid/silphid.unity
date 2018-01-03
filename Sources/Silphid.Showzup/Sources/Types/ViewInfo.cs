using System;
using System.Collections.Generic;

namespace Silphid.Showzup
{
    public struct ViewInfo
    {
        public static ViewInfo Null => new ViewInfo();

        public object Model { get; set; }
        public Type ModelType { get; set; }
        public IViewModel ViewModel { get; set; }
        public Type ViewModelType { get; set; }
        public IView View { get; set; }
        public Type ViewType { get; set; }
        public Uri PrefabUri { get; set; }
        public VariantSet Variants { get; set; }
        public IDictionary<Type, object> Parameters { get; set; }

        public override string ToString() =>
            $"{ModelType?.Name} > {ViewModelType?.Name} > {ViewType?.Name} > {PrefabUri} ({Variants})";
    }
}