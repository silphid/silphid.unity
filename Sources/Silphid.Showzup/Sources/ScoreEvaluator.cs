using System;
using System.Collections.Generic;
using System.Linq;
using Silphid.Extensions;
// ReSharper disable PossibleMultipleEnumeration

namespace Silphid.Showzup
{
    public class ScoreEvaluator : IScoreEvaluator
    {
        public const int ZeroScore = 0;
        public const int ExplicitVariantScore = 100;
        public const int ImplicitVariantScore = 90;
        public const int IncorrectImplicitVariantPenality = 5;
        public const int TypeScore = 80;
        public const int InheritanceDepthPenality = 5;
        public const int FallbackVariantScore = 50;
        public const int ExcessVariantPenality = 5;

        public int? GetVariantScore(VariantSet requestedVariants, VariantSet candidateVariants, VariantSet candidateImplicitVariants)
        {
            var score = ZeroScore;
            var excessVariants = new HashSet<IVariant>(candidateVariants);
            
            foreach (var requestedVariant in requestedVariants)
            {
                // Matches exact variant?
                if (candidateVariants.Contains(requestedVariant))
                {
                    score += ExplicitVariantScore;
                    excessVariants.Remove(requestedVariant);
                }
                
                // Matches another variant in same group? (fail!)
                else if (candidateVariants.Any(x => x.Group == requestedVariant.Group))
                    return null;

                // Matches implicit variant?
                else if (candidateImplicitVariants.Contains(requestedVariant))
                    score += ImplicitVariantScore;
                
                // Matches another implicit variant in same group?
                else if (candidateImplicitVariants.Any(x => x.Group == requestedVariant.Group))
                    score -= IncorrectImplicitVariantPenality;
                
                // No variant specified for that group (is a fallback)
                else
                    score += FallbackVariantScore;
            }

            score -= excessVariants.Count * ExcessVariantPenality;

            return score;
        }

        public int? GetTypeScore(Type requestedType, Type candidateType)
        {
            return candidateType.IsInterface
                ? GetInterfaceScore(candidateType, requestedType)
                : GetClassScore(candidateType, requestedType);
        }

        private static int? GetClassScore(Type candidateClass, Type requestedType)
        {
            var score = TypeScore;
            var type = requestedType;
            while (type != null)
            {
                if (type == candidateClass)
                    return score;

                score -= InheritanceDepthPenality;
                type = type.GetBaseType();
            }

            return null;
        }

        private static int? GetInterfaceScore(Type candidateInterface, Type requestedType)
        {
            var score = TypeScore;
            IEnumerable<Type> interfaces = requestedType.GetInterfaces().ToList();
            
            while (interfaces.Any())
            {
                if (interfaces.Contains(candidateInterface))
                    return score;

                score -= InheritanceDepthPenality;
                interfaces = interfaces.SelectMany(x => x.GetInterfaces());
            }

            return null;
        }
    }
}