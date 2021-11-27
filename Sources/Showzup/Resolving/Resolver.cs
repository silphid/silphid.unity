using System;
using System.Collections.Generic;
using System.Linq;
using log4net;
using Silphid.Extensions;
using Rx = UniRx;

namespace Silphid.Showzup.Resolving
{
    public class Resolver : IResolver
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(Resolver));
        private static readonly ILog ScoreEvaluatorLog = LogManager.GetLogger(typeof(ScoreEvaluator));

        private readonly IManifest _manifest;
        private readonly IScoreEvaluator _scoreEvaluator;
        private readonly TypeModelCollection _typeModelCollection;

        public Resolver(IManifest manifest, IScoreEvaluator scoreEvaluator, TypeModelCollection typeModelCollection)
        {
            _manifest = manifest;
            _scoreEvaluator = scoreEvaluator;
            _typeModelCollection = typeModelCollection;

            ValidateManifest();
        }

        public Solution Resolve(Problem problem)
        {
            if (Log.IsDebugEnabled)
                Log.Debug($"Resolving type: {problem.Type}");

            if (problem.Type == null)
            {
                if (Log.IsDebugEnabled)
                    Log.Debug("Resolved null type to null View.");

                return null;
            }

            if (problem.Type.Type == typeof(Type))
                throw new NotSupportedException("Input cannot be of type Type");

            return ResolveFromProblem(problem);
        }

        private void ValidateManifest()
        {
            if (_manifest == null)
                throw new InvalidManifestException("Manifest is null");

            if (_manifest.ModelsToViewModels == null || _manifest.ViewModelsToViews == null ||
                _manifest.ViewsToPrefabs == null)
                throw new InvalidManifestException("Some manifest dictionary is null");

            if (_manifest.ModelsToViewModels.Any(x => x == null) || _manifest.ViewModelsToViews.Any(x => x == null) ||
                _manifest.ViewsToPrefabs.Any(x => x == null))
                throw new InvalidManifestException("Some manifest dictionary contains null values");

            var invalidModelToViewModel = _manifest.ModelsToViewModels.FirstOrDefault(x => !x.IsValid);
            if (invalidModelToViewModel != null)
                throw new InvalidMappingException(
                    invalidModelToViewModel,
                    "Invalid Model to ViewModel mapping, try rebuilding manifest.");

            var invalidViewModelToView = _manifest.ViewModelsToViews.FirstOrDefault(x => !x.IsValid);
            if (invalidViewModelToView != null)
                throw new InvalidMappingException(
                    invalidViewModelToView,
                    "Invalid ViewModel to View mapping, try rebuilding manifest.");

            var invalidViewToPrefab = _manifest.ViewsToPrefabs.FirstOrDefault(x => !x.IsValid);
            if (invalidViewToPrefab != null)
                throw new InvalidMappingException(
                    invalidViewToPrefab,
                    "Invalid View to Prefab mapping, try rebuilding manifest.");
        }

        private Solution ResolveFromProblem(Problem problem)
        {
            if (problem.Type.Type.GetInterfaces()
                       .Contains(typeof(IViewModel)))
                return ResolveFromViewModel(problem);

            return ResolveFromModel(problem);
        }

        private Solution ResolveFromModel(Problem problem)
        {
            if (Log.IsDebugEnabled)
                Log.Debug($"Resolving model: {problem.Type} (RequestedVariants: {problem.Variants})");

            var viewModelTypeMapping = ResolveViewModelFromModel(problem);
            var modelType = viewModelTypeMapping.Source;
            var viewModelType = viewModelTypeMapping.Target;
            var viewType = ResolveViewFromViewModel(new Problem(viewModelType, problem.Variants))
               .Target;
            var prefabMapping = ResolvePrefabFromViewType(
                new Problem(viewType, problem.Variants),
                viewType);

            return new Solution(
                Guid.NewGuid(),
                modelType,
                viewModelType,
                viewType,
                prefabMapping.Target,
                prefabMapping.Variants);
        }

        private Solution ResolveFromViewModel(Problem problem)
        {
            try
            {
                return ResolveFromModel(problem);
            }
            catch (ResolveException) {}

            if (Log.IsDebugEnabled)
                Log.Debug($"Resolving viewModel: {problem.Type} (RequestedVariants: {problem.Variants})");

            var viewModelType = problem.Type;
            var viewType = ResolveViewFromViewModel(problem)
               .Target;
            var prefabMapping = ResolvePrefabFromViewType(
                new Problem(viewType, problem.Variants),
                viewType);

            return new Solution(Guid.NewGuid(), viewModelType, viewType, prefabMapping.Target, prefabMapping.Variants);
        }

        private TypeModelToTypeModelMapping ResolveViewModelFromModel(Problem problem)
        {
            return ResolveTypeMapping(problem, "Model", "ViewModel", _manifest.ModelsToViewModels);
        }

        private TypeModelToTypeModelMapping ResolveViewFromViewModel(Problem problem)
        {
            return ResolveTypeMapping(problem, "ViewModel", "View", _manifest.ViewModelsToViews);
        }

        private TypeModelToTypeModelMapping ResolveTypeMapping(Problem problem,
                                                               string sourceKind,
                                                               string targetKind,
                                                               IEnumerable<TypeToTypeMapping> typeToTypeMappings)
        {
            if (problem.Type == null)
                throw new ArgumentNullException(nameof(problem.Type));

            var mappings = typeToTypeMappings.ToList();
            ThrowIfMappingsContainsNull(mappings);

            var candidates = mappings.Where(x => problem.Type.Type.IsAssignableTo(x.Source))
                                     .Where(x => sourceKind == "Model" || ViewHasPrefab(problem, x))
                                     .ToList();

            var candidatesWithScores = candidates.Select(
                                                      candidate =>
                                                      {
                                                          if (ScoreEvaluatorLog.IsDebugEnabled)
                                                              ScoreEvaluatorLog.Debug(
                                                                  $"Calculating {candidate.Target} score");

                                                          return new CandidateMapping<TypeToTypeMapping>(
                                                              candidate,
                                                              _scoreEvaluator.GetScore(
                                                                  _typeModelCollection.GetModelFromType(
                                                                      candidate.Source),
                                                                  candidate.Variants,
                                                                  candidate.ImplicitVariants,
                                                                  problem.Type,
                                                                  problem.Variants));
                                                      })
                                                 .ToArray();
            var resolved = candidatesWithScores.Where(candidate => candidate.Score.HasValue)

                                                // ReSharper disable once PossibleInvalidOperationException
                                               .WithMax(candidate => candidate.Score.Value);

            if (resolved == null)
                throw new ResolveException(
                    problem.Type.Type,
                    problem.Variants,
                    $"Failed to resolve {sourceKind} {problem.Type} to some {targetKind}");

            if (Log.IsDebugEnabled)
            {
                var message = $"Resolved {sourceKind} {problem.Type} to " +
                              $"{targetKind} {resolved.Mapping.Target} (Score: {resolved.Score} " +
                              $"Variants: {resolved.Mapping.Variants}\nImplicitVariants: {resolved.Mapping.ImplicitVariants})" +
                              (candidates.Count > 1
                                   ? $"\nOther candidates were:{Environment.NewLine}" + $@"{
                                             candidatesWithScores.Except(resolved)
                                                                 .OrderByDescending(x => x.Score)
                                                                 .JoinAsString(Environment.NewLine)
                                         }"
                                   : "");
                Log.Debug(message);
            }

            return new TypeModelToTypeModelMapping(
                _typeModelCollection.GetModelFromType(resolved.Mapping.Source),
                _typeModelCollection.GetModelFromType(resolved.Mapping.Target),
                problem.Variants);
        }

        private bool ViewHasPrefab(Problem problem, TypeToTypeMapping x)
        {
            return GetPrefabFromViewType(
                       new Problem(_typeModelCollection.GetModelFromType(x.Source), problem.Variants),
                       _typeModelCollection.GetModelFromType(x.Target)) != null;
        }

        private void ThrowIfMappingsContainsNull(IEnumerable<TypeToTypeMapping> mappings)
        {
            var invalidMappings = mappings.FirstOrDefault(x => x.Source == null || x.Target == null);
            if (invalidMappings != null)
                throw new InvalidManifestException(
                    $"Invalid CandidateMapping ({invalidMappings}), try rebuilding manifest.");
        }

        private ViewToPrefabMapping ResolvePrefabFromViewType(Problem problem, TypeModel viewType)
        {
            var resolved = GetPrefabFromViewType(problem, viewType);

            if (resolved == null)
                throw new ResolveException(
                    problem.Type,
                    problem.Variants,
                    $"Failed to resolve View {viewType} to some Prefab");

            return resolved.Mapping;
        }

        private CandidateMapping<ViewToPrefabMapping> GetPrefabFromViewType(Problem problem, TypeModel viewType)
        {
            var candidates = _manifest.ViewsToPrefabs.Where(x => viewType.Type == x.Source)
                                      .ToList();

            var candidatesWithScores = candidates.Select(
                                                      candidate =>
                                                      {
                                                          if (ScoreEvaluatorLog.IsDebugEnabled)
                                                              ScoreEvaluatorLog.Debug(
                                                                  $"Calculating {candidate.Target} score");

                                                          return new CandidateMapping<ViewToPrefabMapping>(
                                                              candidate,
                                                              _scoreEvaluator.GetVariantScore(
                                                                  problem.Variants,
                                                                  candidate.Variants,
                                                                  VariantSet.Empty));
                                                      })
                                                 .ToArray();

            var resolved = candidatesWithScores.Where(candidate => candidate.Score.HasValue)

                                                // ReSharper disable once PossibleInvalidOperationException
                                               .WithMax(candidate => candidate.Score.Value);

            if (Log.IsDebugEnabled && resolved != null)
            {
                var message = $"Resolved View {viewType} to Prefab " +
                              $"{resolved.Mapping.Target} (Variants: {resolved.Mapping.Variants})" +
                              (candidates.Count > 1
                                   ? $"\nOther candidates were:{Environment.NewLine}" + $@"{
                                             candidatesWithScores.Except(resolved)
                                                                 .OrderByDescending(x => x.Score)
                                                                 .JoinAsString(Environment.NewLine)
                                         }"
                                   : "");
                Log.Debug(message);
            }

            return resolved;
        }

        private class CandidateMapping<TMapping> where TMapping : Mapping
        {
            public CandidateMapping(TMapping mapping, float? score)
            {
                Mapping = mapping;
                Score = score;
            }

            public TMapping Mapping { get; }
            public float? Score { get; }

            public override string ToString() => $"{Mapping.TargetName} Score: {Score}";
        }
    }
}