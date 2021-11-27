namespace Silphid.Showzup.Resolving
{
    public interface IScoreEvaluator
    {
        int? GetVariantScore(VariantSet requestedVariants,
                             VariantSet candidateVariants,
                             VariantSet candidateImplicitVariants);

        int? GetTypeScore(TypeModel requestedType, TypeModel candidateType);
    }
}