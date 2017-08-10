using System;

namespace Silphid.Showzup
{
    public static class IScoreEvaluatorExtensions
    {
        public static int? GetScore(this IScoreEvaluator This, Type candidateType, VariantSet candidateVariants, VariantSet candidateImplicitVariants, Type requestedType, VariantSet requestedVariants) =>
            This.GetVariantScore(requestedVariants, candidateVariants, candidateImplicitVariants) +
            This.GetTypeScore(requestedType, candidateType);
    }
}