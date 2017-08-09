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
        public const int MatchedVariantScore = 100;
        public const int MatchedImplicitVariantScore = 90;
        public const int MatchedTypeScore = 80;
        public const int TypeInheritanceDepthScorePenality = 5;
        public const int FallbackVariantScore = 10;

        public int? GetVariantScore(VariantSet candidateVariants, VariantSet candidateImplicitVariants, VariantSet requestedVariants)
        {
            var score = ZeroScore;
            
            foreach (var requestedVariant in requestedVariants)
            {
                // Matches exact variant?
                if (candidateVariants.Contains(requestedVariant))
                    score += MatchedVariantScore;
                
                // Matches another variant in same group? (fail!)
                else if (candidateVariants.Any(x => x.Group == requestedVariant.Group))
                    return null;

                // Matches implicit variant?
                else if (candidateImplicitVariants.Contains(requestedVariant))
                    score += MatchedImplicitVariantScore;
                
                // No variant specified for that group (is a fallback)
                else
                    score += FallbackVariantScore;
            }

            return score;
        }

        public int? GetTypeScore(Type candidateType, Type requestedType)
        {
            return candidateType.IsInterface
                ? GetInterfaceScore(candidateType, requestedType)
                : GetClassScore(candidateType, requestedType);
        }

        private static int? GetClassScore(Type candidateClass, Type requestedType)
        {
            var score = MatchedTypeScore;
            var type = requestedType;
            while (type != null)
            {
                if (type == candidateClass)
                    return score;

                score -= TypeInheritanceDepthScorePenality;
                type = type.GetBaseType();
            }

            return null;
        }

        private static int? GetInterfaceScore(Type candidateInterface, Type requestedType)
        {
            var score = MatchedTypeScore;
            IEnumerable<Type> interfaces = requestedType.GetInterfaces().ToList();
            
            while (interfaces.Any())
            {
                if (interfaces.Contains(candidateInterface))
                    return score;

                score -= TypeInheritanceDepthScorePenality;
                interfaces = interfaces.SelectMany(x => x.GetInterfaces());
            }

            return null;
        }
    }
}