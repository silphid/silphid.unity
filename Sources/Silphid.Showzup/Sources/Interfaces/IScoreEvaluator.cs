using System;

namespace Silphid.Showzup
{
    public interface IScoreEvaluator
    {
        int? GetVariantScore(VariantSet candidateVariants, VariantSet requestedVariants);
        int? GetTypeScore(Type candidateType, Type requestedType);
    }
}