using System;

namespace Silphid.Showzup
{
    public interface IScoreEvaluator
    {
        int? GetVariantScore(VariantSet candidateVariants, VariantSet candidateImplicitVariants, VariantSet requestedVariants);
        int? GetTypeScore(Type candidateType, Type requestedType);
    }
}