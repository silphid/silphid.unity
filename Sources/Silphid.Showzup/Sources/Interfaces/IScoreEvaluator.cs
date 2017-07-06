using System;

namespace Silphid.Showzup
{
    public interface IScoreEvaluator
    {
        float? GetVariantScore(VariantSet candidateVariants, VariantSet requestedVariants);
        float? GetTypeScore(Type candidateType, Type requestedType);
    }
}