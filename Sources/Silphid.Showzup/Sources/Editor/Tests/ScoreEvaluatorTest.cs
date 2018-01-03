using NUnit.Framework;

namespace Silphid.Showzup.Test
{
    [TestFixture]
    public class ScoreEvaluatorTest
    {
        public class Temper : Variant<Temper>
        {
            public static readonly Temper Good = new Temper();
            public static readonly Temper Bad = new Temper();
        }

        public class Speed : Variant<Speed>
        {
            public static readonly Speed Slow = new Speed();
            public static readonly Speed Fast = new Speed();
        }

        private class Animal {}
        private class Dog : Animal {}
        
        private readonly ScoreEvaluator _fixture = new ScoreEvaluator();

        [Test]
        public void ExactType_MatchesWithMaximumScore()
        {
            var score = _fixture.GetTypeScore(typeof(Animal), typeof(Animal));
            
            Assert.That(score, Is.EqualTo(ScoreEvaluator.TypeScore));
        }

        [Test]
        public void DerivedType_MatchesWithLowerScore()
        {
            var score = _fixture.GetTypeScore(typeof(Dog), typeof(Animal));
            
            Assert.That(score, Is.EqualTo(ScoreEvaluator.TypeScore - ScoreEvaluator.InheritanceDepthPenality));
        }

        [Test]
        public void ParentType_DoesNotMatchAtAll()
        {
            var score = _fixture.GetTypeScore(typeof(Animal), typeof(Dog));
            
            Assert.That(score, Is.Null);
        }

        private readonly VariantSet Empty = VariantSet.Empty;
        private readonly VariantSet Good = new VariantSet(Temper.Good);
        private readonly VariantSet Bad = new VariantSet(Temper.Bad);
        private readonly VariantSet Slow = new VariantSet(Speed.Slow);
        private readonly VariantSet Fast = new VariantSet(Speed.Fast);
        private readonly VariantSet GoodAndFast = new VariantSet(Temper.Good, Speed.Fast);
        private readonly VariantSet GoodAndSlow = new VariantSet(Temper.Good, Speed.Slow);

        [Test]
        public void ExplicitVariantMatched()
        {
            var score = _fixture.GetVariantScore(Good, Good, Empty);
            Assert.That(score, Is.EqualTo(ScoreEvaluator.ExplicitVariantScore));
        }

        [Test]
        public void ImplicitVariantMatched()
        {
            var score = _fixture.GetVariantScore(Good, Empty, Good);
            Assert.That(score, Is.EqualTo(ScoreEvaluator.ImplicitVariantScore));
        }

        [Test]
        public void FallbackVariantMatched()
        {
            var score = _fixture.GetVariantScore(Good, Empty, Empty);
            Assert.That(score, Is.EqualTo(ScoreEvaluator.FallbackVariantScore));
        }

        [Test]
        public void FallbackVariantMatched_WithExcessCandidateVariant()
        {
            var score = _fixture.GetVariantScore(Good, Fast, Empty);
            Assert.That(score, Is.EqualTo(ScoreEvaluator.FallbackVariantScore - ScoreEvaluator.ExcessVariantPenality));
        }
        
        [Test]
        public void FallbackIsPrivilegedWhenCandidateHasExcessVariants()
        {
            var scoreFallback = _fixture.GetVariantScore(Empty, Empty, Empty);
            var scoreExcess = _fixture.GetVariantScore(Empty, Slow, Empty);
            
            Assert.That(scoreFallback, Is.GreaterThan(scoreExcess));

            scoreFallback = _fixture.GetVariantScore(Good, Empty, Empty);
            scoreExcess = _fixture.GetVariantScore(Good, Slow, Empty);
            var scoreMatchAndExcess = _fixture.GetVariantScore(Good, GoodAndSlow, Empty);
            
            Assert.That(scoreFallback, Is.GreaterThan(scoreExcess));
            Assert.That(scoreMatchAndExcess, Is.GreaterThan(scoreFallback));
        }

        [Test]
        public void TwoExplicitVariantsMatched()
        {
            var score = _fixture.GetVariantScore(GoodAndFast, GoodAndFast, Empty);
            Assert.That(score, Is.EqualTo(2 * ScoreEvaluator.ExplicitVariantScore));
        }

        [Test]
        public void IncorrectVariant_Fails()
        {
            var score = _fixture.GetVariantScore(Good, Bad, Empty);
            Assert.That(score, Is.Null);
        }

        [Test]
        public void OneIncorrectVariantAndOneMatched_Fails()
        {
            var score = _fixture.GetVariantScore(GoodAndFast, GoodAndSlow, Empty);
            Assert.That(score, Is.Null);
        }

        [Test]
        public void NoImplicitVariantIsBetterThanIncorrectImplicitVariant()
        {
            var noImplicitScore = _fixture.GetVariantScore(Good, Empty, Empty);
            var incorrectImplicitScore = _fixture.GetVariantScore(Good, Empty, Bad);
            Assert.That(noImplicitScore, Is.GreaterThan(incorrectImplicitScore));
        }
    }
}