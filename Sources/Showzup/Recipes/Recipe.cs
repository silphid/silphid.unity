using System;
using System.Collections.Generic;

namespace Silphid.Showzup.Recipes
{
    public struct Recipe
    {
        public static Recipe Null => new Recipe();

        public object Model { get; set; }
        public Type ModelType { get; set; }
        public IViewModel ViewModel { get; set; }
        public Type ViewModelType { get; set; }
        public IView View { get; set; }
        public Type ViewType { get; set; }
        public Uri PrefabUri { get; set; }
        public VariantSet Variants { get; set; }
        public IDictionary<Type, object> Parameters { get; set; }
        public IOptions Options { get; set; }

        public override string ToString() =>
            $"{ModelType?.Name} > {ViewModelType?.Name} > {ViewType?.Name} > {PrefabUri} ({Variants})";
    }
}