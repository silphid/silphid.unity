using System;

namespace Silphid.Showzup.Resolving
{
    public class Solution
    {
        public Solution(Guid id,
                        TypeModel model,
                        TypeModel viewModel,
                        TypeModel view,
                        Uri prefab,
                        VariantSet prefabVariants)
        {
            Id = id;
            Model = model;
            ViewModel = viewModel;
            View = view;
            Prefab = prefab;
            PrefabVariants = prefabVariants;
        }

        public Solution(Guid id, TypeModel viewModel, TypeModel view, Uri prefab, VariantSet prefabVariants)
        {
            Id = id;
            ViewModel = viewModel;
            View = view;
            Prefab = prefab;
            PrefabVariants = prefabVariants;
        }

        public Guid Id { get; }
        public TypeModel Model { get; }
        public TypeModel ViewModel { get; }
        public TypeModel View { get; }
        public Uri Prefab { get; }
        public VariantSet PrefabVariants { get; }

        public override string ToString()
        {
            return $"{nameof(Id)}: {Id}, {nameof(Model)}: {Model}, {nameof(ViewModel)}: {ViewModel}, " +
                   $"{nameof(View)}: {View}, {nameof(Prefab)}: {Prefab}, {nameof(PrefabVariants)}: {PrefabVariants}";
        }
    }
}