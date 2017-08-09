using System;
using System.Collections.Generic;
using System.Linq;
using Silphid.Extensions;
using UnityEngine;
using Rx = UniRx;

namespace Silphid.Showzup
{
    public class ViewResolver : IViewResolver
    {
        private class CandidateMapping<TMapping>
        {
            public TMapping Mapping { get; }
            public float? Score { get; }

            public CandidateMapping(TMapping mapping, float? score)
            {
                Mapping = mapping;
                Score = score;
            }
        }
        
        private readonly IManifest _manifest;
        private readonly IVariantProvider _variantProvider;
        private readonly IScoreEvaluator _scoreEvaluator;
        private readonly ILogger _logger;

        public ViewResolver(IManifest manifest, IVariantProvider variantProvider, IScoreEvaluator scoreEvaluator, ILogger logger = null)
        {
            _manifest = manifest;
            _variantProvider = variantProvider;
            _scoreEvaluator = scoreEvaluator;
            _logger = logger;
        }

        public ViewInfo Resolve(object input, Options options = null)
        {
            if (input == null)
            {
                _logger?.Log("#Views# Resolved null input to null View.");
                return ViewInfo.Null;
            }

            var requestedVariants = GetRequestedVariants(options);
            if (input is Type)
            {
                var type = (Type) input;
                if (type.IsAssignableTo<IView>())
                    return ResolveFromViewType(type, requestedVariants);
                
                throw new ArgumentException("Only types implementing IView can be passed as input");
            }
            
            var viewInfo = ResolveFromInstance(input, requestedVariants);
            viewInfo.Parameters = options?.Parameters ?? Enumerable.Empty<object>();
            return viewInfo;
        }

        private VariantSet GetRequestedVariants(Options options)
        {
            var requestedVariants = options.GetVariants().UnionWith(_variantProvider.GlobalVariants.Value);
            if (requestedVariants.Distinct(x => x.Group).Count() != requestedVariants.Count())
                throw new InvalidOperationException($"Cannot request more than one variant per group: {requestedVariants}");
            
            return requestedVariants;
        }

        private ViewInfo ResolveFromInstance(object instance, VariantSet requestedVariants)
        {
            if (instance is IView)
                return ResolveFromView((IView) instance, requestedVariants);

            if (instance is IViewModel)
                return ResolveFromViewModel((IViewModel) instance, requestedVariants);
            
            return ResolveFromModel(instance, requestedVariants);
        }

        private ViewInfo ResolveFromModel(object model, VariantSet requestedVariants)
        {
            var modelType = model.GetType();
            var viewModelType = ResolveTargetType(modelType, "Model", "ViewModel", _manifest.ModelsToViewModels, requestedVariants);
            var viewType = ResolveTargetType(viewModelType, "ViewModel", "View", _manifest.ViewModelsToViews, requestedVariants);
            var prefabUri = ResolvePrefabFromViewType(viewType, requestedVariants);

            return new ViewInfo
            {
                Model = model,
                ModelType = modelType,
                ViewModelType = viewModelType,
                ViewType = viewType,
                PrefabUri = prefabUri
            };
        }

        private ViewInfo ResolveFromViewModel(IViewModel viewModel, VariantSet requestedVariants)
        {
            var viewModelType = viewModel.GetType();
            var viewType = ResolveTargetType(viewModelType, "ViewModel", "View", _manifest.ViewModelsToViews, requestedVariants);
            var prefabUri = ResolvePrefabFromViewType(viewType, requestedVariants);
            
            return new ViewInfo
            {
                ViewModel = viewModel,
                ViewModelType = viewModelType,
                ViewType = viewType,
                PrefabUri = prefabUri
            };
        }

        private ViewInfo ResolveFromView(IView view, VariantSet requestedVariants)
        {
            var viewType = view.GetType();
            var prefabUri = ResolvePrefabFromViewType(viewType, requestedVariants);
            
            return new ViewInfo
            {
                View = view,
                ViewType = viewType,
                PrefabUri = prefabUri
            };
        }

        private ViewInfo ResolveFromViewType(Type viewType, VariantSet requestedVariants)
        {
            var prefabUri = ResolvePrefabFromViewType(viewType, requestedVariants);
            
            return new ViewInfo
            {
                ViewType = viewType,
                PrefabUri = prefabUri
            };
        }

        private Type ResolveTargetType(Type type, string sourceKind, string targetKind, List<TypeToTypeMapping> mappings, VariantSet requestedVariants)
        {
            var candidates = mappings
                .Where(x => type.IsAssignableTo(x.Source))
                .ToList();
            
            var resolved = candidates
                .Select(candidate => new CandidateMapping<TypeToTypeMapping>(
                    candidate,
                    GetScore(candidate.Source, candidate.Variants, candidate.ImplicitVariants, type, requestedVariants)))
                .Where(candidate => candidate.Score.HasValue)
                .WithMax(candidate => candidate.Score.Value)
                ?.Mapping;

            if (resolved == null)
                throw new InvalidOperationException($"Failed to resolve {sourceKind} {type} to some {targetKind} (Variants: {requestedVariants})");

            _logger?.Log($"#Views# Resolved {sourceKind} {type} to {targetKind} {resolved.Target} (Variants: {resolved.Variants})");

            if (candidates.Count > 1)
                _logger?.Log($"#Views# Other candidates were:{Environment.NewLine}" +
                          $"{candidates.Except(resolved).ToDelimitedString(Environment.NewLine)}");
            
            return resolved.Target;
        }

        private Uri ResolvePrefabFromViewType(Type viewType, VariantSet requestedVariants)
        {
            var candidates = _manifest.ViewsToPrefabs
                .Where(x => viewType == x.Source)
                .ToList();
            
            var resolved = candidates
                .Select(candidate => new CandidateMapping<TypeToUriMapping>(candidate,
                    _scoreEvaluator.GetVariantScore(candidate.Variants, null, requestedVariants)))
                .Where(candidate => candidate.Score.HasValue)
                .WithMax(candidate => candidate.Score.Value)
                ?.Mapping;

            if (resolved == null)
                throw new InvalidOperationException($"Failed to resolve View {viewType} to some Prefab (Variants: {requestedVariants})");

            _logger?.Log($"#Views# Resolved View {viewType} to Prefab {resolved.Target} (Variants: {resolved.Variants})");

            if (candidates.Count > 1)
                _logger?.Log($"#Views# Other candidates were:{Environment.NewLine}" +
                          $"{candidates.Except(resolved).ToDelimitedString(Environment.NewLine)}");
            
            return resolved.Target;
        }

        private float? GetScore(Type candidateType, VariantSet candidateVariants, VariantSet candidateImplicitVariants, Type requestedType, VariantSet requestedVariants) =>
            _scoreEvaluator.GetVariantScore(candidateVariants, candidateImplicitVariants, requestedVariants) +
            _scoreEvaluator.GetTypeScore(candidateType, requestedType);
    }
}