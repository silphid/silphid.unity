using System;
using System.Linq;
using Silphid.Extensions;

namespace Silphid.Showzup
{
    public class ScoreEvaluator : IScoreEvaluator
    {
        private const int ZeroScore = 0;
        private const int ExactMatchScore = 100;
        private const int InheritanceScorePenality = 5;
        private const int FallbackScore = 10;

        public int? GetVariantScore(VariantSet candidateVariants, VariantSet requestedVariants)
        {
            int score = ZeroScore;
            
            foreach (var requestedVariant in requestedVariants)
            {
                // Matches exact variant?
                if (candidateVariants.Contains(requestedVariant))
                    score += ExactMatchScore;
                
                // Matches another variant in same group? (fail!)
                else if (candidateVariants.Any(x => x.Group == requestedVariant.Group))
                    return null;
                
                // No variant specified for that group (is a fallback)
                else
                    score += FallbackScore;
            }

            return score;
        }

        public int? GetTypeScore(Type candidateType, Type requestedType)
        {
            var score = ExactMatchScore;
            var type = candidateType;
            while (type != null)
            {
                if (type == requestedType)
                    return score;

                score -= InheritanceScorePenality;
                type = type.GetBaseType();
            }

            return null;
        }
    }
}