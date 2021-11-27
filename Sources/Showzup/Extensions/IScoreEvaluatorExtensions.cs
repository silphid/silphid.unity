using log4net;
using Silphid.Showzup.Resolving;

namespace Silphid.Showzup
{
    public static class IScoreEvaluatorExtensions
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(ScoreEvaluator));

        public static int? GetScore(this IScoreEvaluator This,
                                    TypeModel candidateType,
                                    VariantSet candidateVariants,
                                    VariantSet candidateImplicitVariants,
                                    TypeModel requestedType,
                                    VariantSet requestedVariants)
        {
            var score = This.GetVariantScore(requestedVariants, candidateVariants, candidateImplicitVariants) +
                        This.GetTypeScore(requestedType, candidateType);

            if (Log.IsDebugEnabled)
                Log.Debug($"Final score {score}");

            return score;
        }
    }
}