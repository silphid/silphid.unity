using System;
using System.Collections.Generic;
using System.Linq;
using log4net;
using Silphid.Extensions;
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
        
        private static readonly ILog Log = LogManager.GetLogger(typeof(ViewResolver));

        public ViewResolver(IManifest manifest, IVariantProvider variantProvider, IScoreEvaluator scoreEvaluator)
        {
            _manifest = manifest;
            _variantProvider = variantProvider;
            _scoreEvaluator = scoreEvaluator;
            
            ValidateManifest();
        }

        private void ValidateManifest()
        {
            if (_manifest.ModelsToViewModels.Any(x => x.Source == null || x.Target == null || x.Variants == null) ||
                _manifest.ViewModelsToViews.Any(x => x.Source == null || x.Target == null || x.Variants == null) ||
                _manifest.ViewsToPrefabs.Any(x => x.Source == null || x.Target == null || x.Variants == null))
                throw new InvalidOperationException("Manifest needs to be rebuilt.");
        }

        public ViewInfo Resolve(object input, Options options = null)
        {
            if (Log.IsDebugEnabled)
                Log.Debug($"Resolving input: {input}");
            
            if (input == null)
            {
                if (Log.IsDebugEnabled)
                    Log.Debug("Resolved null input to null View.");
                
                return ViewInfo.Null;
            }

            var requestedVariants = GetRequestedVariants(options);
            if (input is Type)
            {
                var type = (Type) input;
                if (Log.IsDebugEnabled)
                    Log.Debug($"Resolving type: {type}");
                
                if (type.IsAssignableTo<IView>())
                    return ResolveFromViewType(type, requestedVariants);

                if (type.IsAssignableTo<IViewModel>())
                    return ResolveFromViewModelType(type, requestedVariants);
                
                throw new ArgumentException("Only types implementing IView can be passed as input");
            }
            
            var viewInfo = ResolveFromInstance(input, requestedVariants);
            viewInfo.Parameters = options?.Parameters?.ToArray() ?? new object[]{};
            return viewInfo;
        }

        private VariantSet GetRequestedVariants(Options options)
        {
            var requestedVariants = options.GetVariantsOrDefault().UnionWith(_variantProvider.GlobalVariants.Value);
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
            if (Log.IsDebugEnabled)
                Log.Debug($"Resolving model: {model}");

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
            try
            {
                return ResolveFromModel(viewModel, requestedVariants);
            }
            catch (InvalidOperationException)
            {
            }

            if (Log.IsDebugEnabled)
                Log.Debug($"Resolving viewModel: {viewModel}");

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
            if (Log.IsDebugEnabled)
                Log.Debug($"Resolving view: {view}");

            var viewType = view.GetType();
            var prefabUri = ResolvePrefabFromViewType(viewType, requestedVariants);
            
            return new ViewInfo
            {
                View = view,
                ViewType = viewType,
                PrefabUri = prefabUri
            };
        }

        private ViewInfo ResolveFromViewModelType(Type viewModelType, VariantSet requestedVariants)
        {
            if (Log.IsDebugEnabled)
                Log.Debug($"Resolving viewModelType: {viewModelType}");

            var viewType = ResolveTargetType(viewModelType, "ViewModel", "View", _manifest.ViewModelsToViews, requestedVariants);
            var prefabUri = ResolvePrefabFromViewType(viewType, requestedVariants);
            
            return new ViewInfo
            {
                ViewModelType = viewModelType,
                ViewType = viewType,
                PrefabUri = prefabUri
            };
        }

        private ViewInfo ResolveFromViewType(Type viewType, VariantSet requestedVariants)
        {
            if (Log.IsDebugEnabled)
                Log.Debug($"Resolving viewType: {viewType}");

            var prefabUri = ResolvePrefabFromViewType(viewType, requestedVariants);
            
            return new ViewInfo
            {
                ViewType = viewType,
                PrefabUri = prefabUri
            };
        }

        private Type ResolveTargetType(Type type, string sourceKind, string targetKind, List<TypeToTypeMapping> mappings, VariantSet requestedVariants)
        {
            if (type == null)
                throw new ArgumentNullException(nameof(type));
            
            var candidates = mappings
                .Where(x => type.IsAssignableTo(x.Source))
                .ToList();
            
            var resolved = candidates
                .Select(candidate => new CandidateMapping<TypeToTypeMapping>(
                    candidate,
                    _scoreEvaluator.GetScore(candidate.Source, candidate.Variants, candidate.ImplicitVariants, type, requestedVariants)))
                .Where(candidate => candidate.Score.HasValue)
                .WithMax(candidate => candidate.Score.Value)
                ?.Mapping;

            if (resolved == null)
                throw new InvalidOperationException($"Failed to resolve {sourceKind} {type} to some {targetKind} (Variants: {requestedVariants})");

            if (Log.IsDebugEnabled)
                Log.Debug($"Resolved {sourceKind} {type} to {targetKind} {resolved.Target} (Variants: {resolved.Variants})");

            if (candidates.Count > 1 && Log.IsDebugEnabled)
                Log.Debug($"Other candidates were:{Environment.NewLine}" +
                          $"{candidates.Except(resolved).JoinAsString(Environment.NewLine)}");
            
            return resolved.Target;
        }

        private Uri ResolvePrefabFromViewType(Type viewType, VariantSet requestedVariants)
        {
            var candidates = _manifest.ViewsToPrefabs
                .Where(x => viewType == x.Source)
                .ToList();
            
            var resolved = candidates
                .Select(candidate => new CandidateMapping<TypeToUriMapping>(candidate,
                    _scoreEvaluator.GetVariantScore(requestedVariants, candidate.Variants, VariantSet.Empty)))
                .Where(candidate => candidate.Score.HasValue)
                .WithMax(candidate => candidate.Score.Value)
                ?.Mapping;

            if (resolved == null)
                throw new InvalidOperationException($"Failed to resolve View {viewType} to some Prefab (Variants: {requestedVariants})");

            if (Log.IsDebugEnabled)
                Log.Debug($"Resolved View {viewType} to Prefab {resolved.Target} (Variants: {resolved.Variants})");

            if (candidates.Count > 1 && Log.IsDebugEnabled)
                Log.Debug($"Other candidates were:{Environment.NewLine}" +
                          $"{candidates.Except(resolved).JoinAsString(Environment.NewLine)}");
            
            return resolved.Target;
        }
    }
}