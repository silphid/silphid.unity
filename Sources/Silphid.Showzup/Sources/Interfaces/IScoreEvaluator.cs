using System;

namespace Silphid.Showzup
{
    public interface IScoreEvaluator
    {
        int? GetVariantScore(VariantSet requestedVariants, VariantSet candidateVariants, VariantSet candidateImplicitVariants);
        int? GetTypeScore(Type requestedType, Type candidateType);
    }
}