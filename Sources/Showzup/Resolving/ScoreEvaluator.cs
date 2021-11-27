using System.Collections.Generic;
using System.Linq;
using log4net;

namespace Silphid.Showzup.Resolving
{
    public class ScoreEvaluator : IScoreEvaluator
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(ScoreEvaluator));

        public const int ZeroScore = 0;
        public const int ExplicitVariantScore = 150;
        public const int ImplicitVariantScore = 90;
        public const int ExplicitFallbackVariantScore = 80;
        public const int ImplicitFallbackVariantScore = 50;
        public const int IncorrectImplicitVariantPenalty = 5;
        public const int TypeScore = 180;
        public const int InheritanceDepthPenalty = 10;
        public const int InterfacePenalty = 60;
        public const int FallbackVariantScore = 50;
        public const int ExcessVariantPenalty = 5;

        public int? GetVariantScore(VariantSet requestedVariants,
                                    VariantSet candidateVariants,
                                    VariantSet candidateImplicitVariants)
        {
            if (Log.IsDebugEnabled)
                Log.Debug(
                    "Calculating Variant Score " + $"\n requestedVariants: {requestedVariants} " +
                    $"\n candidateVariants: {candidateVariants} " +
                    $"\n candidateImplicitVariants: {candidateImplicitVariants}");

            var score = ZeroScore;
            var excessVariants = new HashSet<IVariant>(candidateVariants);

            foreach (var requestedVariant in requestedVariants)
            {
                // Matches exact variant?
                if (candidateVariants.Contains(requestedVariant))
                {
                    score += ExplicitVariantScore;
                    excessVariants.Remove(requestedVariant);

                    if (Log.IsDebugEnabled)
                        Log.Debug(
                            $"Contains explicit variant {requestedVariant}; " +
                            $"New score: {score} (+{ExplicitVariantScore})");
                }

                else if (candidateVariants.Contains(requestedVariant.Fallback))
                {
                    score += ExplicitFallbackVariantScore;
                    excessVariants.Remove(requestedVariant.Fallback);

                    if (Log.IsDebugEnabled)
                        Log.Debug(
                            $"Contains explicit variant fallback {requestedVariant}; " +
                            $"New score: {score} (+{ExplicitFallbackVariantScore})");
                }

                // Matches another variant in same group? (fail!)
                else if (candidateVariants.Any(x => x.Group == requestedVariant.Group))
                {
                    Log.Debug("Matches another explicit variant from the same group. Score is null");

                    return null;
                }

                // Matches implicit variant?
                else if (candidateImplicitVariants.Contains(requestedVariant))
                {
                    score += ImplicitVariantScore;
                    if (Log.IsDebugEnabled)
                        Log.Debug(
                            $"Contains implicit variant {requestedVariant}; " +
                            $"New score: {score} (+{ImplicitVariantScore})");
                }

                else if (candidateImplicitVariants.Contains(requestedVariant.Fallback))
                {
                    score += ImplicitFallbackVariantScore;
                    if (Log.IsDebugEnabled)
                        Log.Debug(
                            $"Contains implicit fallback  variant {requestedVariant};" +
                            $"New score: {score} (+{ImplicitFallbackVariantScore})");
                }

                // Matches another implicit variant in same group?
                else if (candidateImplicitVariants.Any(x => x.Group == requestedVariant.Group))
                {
                    score -= IncorrectImplicitVariantPenalty;

                    if (Log.IsDebugEnabled)
                        Log.Debug(
                            $"Contains another implicit variant from group {requestedVariant.Group.Name};" +
                            $"New score: {score} (-{IncorrectImplicitVariantPenalty})");
                }

                // No variant specified for that group (is a fallback)
                else
                {
                    score += FallbackVariantScore;

                    if (Log.IsDebugEnabled)
                        Log.Debug(
                            $"No variant specified for group {requestedVariant.Group.Name};" +
                            $"New score: {score} (+{FallbackVariantScore})");
                }
            }

            score -= excessVariants.Count * ExcessVariantPenalty;

            if (Log.IsDebugEnabled && excessVariants.Count > 0)
                Log.Debug(
                    "Has excess explicit variants; " +
                    $"New score:: {score} (-{excessVariants.Count}*{ExcessVariantPenalty})");

            if (Log.IsDebugEnabled)
                Log.Debug("Final Variant score: " + score);

            return score;
        }

        public int? GetTypeScore(TypeModel requestedType, TypeModel candidateType)
        {
            return candidateType is InterfaceModel
                       ? GetInterfaceScore(candidateType, requestedType)
                       : GetClassScore(candidateType, (ClassModel) requestedType);
        }

        private static int? GetClassScore(TypeModel candidateClass, ClassModel requestedType)
        {
            Log.Debug("Calculating Class Score");

            var score = TypeScore;
            var type = requestedType;
            while (type.Type != typeof(object))
            {
                if (type == candidateClass)
                {
                    if (Log.IsDebugEnabled)
                        Log.Debug($"Final Class score for {candidateClass.Name}: " + score);
                    return score;
                }

                if (Log.IsDebugEnabled)
                    Log.Debug(
                        $"Class score decreased from {score} to {score - InheritanceDepthPenalty} for {candidateClass.Name} ({type.Name})");

                score -= InheritanceDepthPenalty;
                type = type.Parent;
            }

            return null;
        }

        private static int? GetInterfaceScore(TypeModel candidateInterface, TypeModel requestedType)
        {
            Log.Debug("Calculating Interface Score");
            const int score = TypeScore - InterfacePenalty;

            var inheritanceDepth = GetInheritanceDepth(candidateInterface, requestedType);

            if (Log.IsDebugEnabled)
            {
                if (inheritanceDepth > 0)
                    Log.Debug(
                        "Interface has inheritance depth. " +
                        $"Decreasing score by (-{InheritanceDepthPenalty} * {inheritanceDepth})");

                Log.Debug(
                    $"Final Interface score for {candidateInterface.Name}: " +
                    $"{score - InheritanceDepthPenalty * inheritanceDepth}");
            }

            return score - InheritanceDepthPenalty * inheritanceDepth;
        }

        private static int? GetInheritanceDepth(TypeModel candidateInterface, TypeModel requestedType)
        {
            var queue = new Queue<(TypeModel, int)>();
            queue.Enqueue((requestedType, 0));

            // Breadth-first search
            while (queue.Any())
            {
                var (typeModel, depth) = queue.Dequeue();

                if (typeModel.Interfaces != null)
                {
                    if (typeModel.Interfaces.Contains(candidateInterface))
                        return depth;

                    foreach (var interfaceModel in typeModel.Interfaces)
                        queue.Enqueue((interfaceModel, depth + 1));
                }

                if (typeModel is ClassModel classModel && classModel.Parent != null)
                    queue.Enqueue((classModel.Parent, depth + 1));
            }

            return null;
        }
    }
}