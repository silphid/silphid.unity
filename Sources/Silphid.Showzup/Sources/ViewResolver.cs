using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Silphid.Extensions;
using Zenject;

namespace Silphid.Showzup
{
    public class ViewResolver : IViewResolver //, IInitializable
    {
        private class Candidate
        {
            public ViewInfo ViewInfo { get; }
            private int VariantScore { get; }
            private int TypeScore { get; }

            public Candidate(ViewInfo viewInfo, int variantScore, int typeScore)
            {
                ViewInfo = viewInfo;
                VariantScore = variantScore;
                TypeScore = typeScore;
            }

            public override string ToString() =>
                $"{ViewInfo} (VariantScore: {VariantScore}, TypeScore: {TypeScore})";
        }

        private const int ZeroScore = 0;
        private const int MediumScore = 50;
        private const int HighScore = 100;

        private readonly Manifest _manifest;
        private readonly VariantSet _globalVariants;

        public ViewResolver(Manifest manifest, [InjectOptional] VariantSet globalVariants = null)
        {
            _manifest = manifest;
            _globalVariants = globalVariants ?? VariantSet.Empty;
        }

//        public void Initialize()
//        {
//            _viewInfos.AddRange(
//                from viewType in GetAllViewTypes()
//                let viewModelType = GetViewModelType(viewType)
//                let viewVariants = viewType.GetAttributes<VariantAttribute>().Select(x => x.Variant).ToArray()
//                from assetAttribute in viewType.GetAttributes<AssetAttribute>()
//                select new ViewInfo
//                {
//                    ViewModelType = viewModelType,
//                    ViewType = viewType,
//                    Uri = assetAttribute.Uri,
//                    Variants = viewVariants.Concat(assetAttribute.Variants)
//                });
//        }

        private Type GetViewModelType(Type viewType) =>
            viewType
                .SelfAndAncestors()
                .FirstOrDefault(x => x.IsGenericType() && x.GetGenericTypeDefinition() == typeof(View<>))
                ?.GetGenericArguments()
                .FirstOrDefault();

        private IEnumerable<Type> GetAllViewTypes() =>
            from candidateType in _viewsAssembly.GetTypes()
            where typeof(IView).IsAssignableFrom(candidateType) && !candidateType.IsAbstract()
            select candidateType;

        public ViewInfo Resolve(object input, Options options = null)
        {
            if (input == null)
            {
//                Debug.Log("#Views# Using null view for null input");
                return ViewInfo.Null;
            }

            if (input is IView)
            {
//                Debug.Log("#Views# Using input itself as view");
                return new ViewInfo
                {
                    View = (IView)input,
                    ViewType = input.GetType()
                };
            }

            if (input is Type)
            {
                var type = (Type) input;
                if (!type.IsAssignableTo<IView>())
                    throw new NotSupportedException($"Input type {type} does not implement IView");

                return ResolveFromViewType(type, options);
            }

            var viewInfo = ResolveFromViewModelType(input.GetType(), options);
            viewInfo.ViewModel = input;
            return viewInfo;
        }

        private ViewInfo ResolveFromViewModelType(Type viewModelType, Options options = null) =>
            ResolveInternal(viewModelType, "view model", viewInfo => GetTypeScore(viewModelType, viewInfo.ViewModelType), options);

        private ViewInfo ResolveFromViewType(Type viewType, Options options = null) =>
            ResolveInternal(viewType, "view", viewInfo => GetTypeScore(viewType, viewInfo.ViewType), options);

        private ViewInfo ResolveInternal(Type type, string kind, Func<ViewInfo, int> getTypeSpecificity, Options options)
        {
            var variants = options.GetVariants().UnionWith(_globalVariants);

//            Debug.Log($"#Views# Resolving view for {type} and variants {variants.ToDelimitedString(";")}");

            var candidates = (
                    from viewInfo in _viewInfos
                    let variantScore = GetVariantScore(viewInfo.ViewType.Name, variants, viewInfo.Variants)
                    let viewModelScore = getTypeSpecificity(viewInfo)
                    where viewModelScore != ZeroScore
                    orderby variantScore descending, viewModelScore descending
                    select new Candidate(viewInfo, variantScore, viewModelScore))
                .ToList();

            var resolved = candidates.FirstOrDefault();
            if (resolved == null)
                throw new InvalidOperationException($"Failed to resolve view info for {kind} type {type}");

//            Debug.Log($"#Views# Resolved: {resolved}");

//            if (candidates.Count > 1)
//                Debug.Log($"#Views# Other candidates:{Environment.NewLine}" +
//                          $"{candidates.Skip(1).ToDelimitedString(Environment.NewLine)}");

            return resolved.ViewInfo;
        }

        private static int GetVariantScore(string viewTypeName, VariantSet requestedVariants, VariantSet candidateVariants)
        {
            throw new NotImplementedException();
//            var requiredVariants = requestedVariants.Where(x => x.EndsWith("!")).ToList();
//            var matchedRequiredCount = requiredVariants.Count(candidateVariants.Contains);
////            Debug.Log($"#Views# {viewTypeName} Required variants: {requiredVariants.Count}  Matched required: {matchedRequiredCount}");
//            if (matchedRequiredCount < requiredVariants.Count)
//                return ZeroScore;
//
//            return requestedVariants.Count(candidateVariants.Contains) * HighScore +
//                   (candidateVariants.Contains(DefaultCategory) ? MediumScore : ZeroScore);
        }

        private static int GetTypeScore(Type requestedType, Type candidateType)
        {
            var score = HighScore;
            var type = candidateType;
            while (type != null)
            {
                if (type == requestedType)
                    return score;

                score--;
                type = type.GetBaseType();
            }

            return ZeroScore;
        }
    }
}