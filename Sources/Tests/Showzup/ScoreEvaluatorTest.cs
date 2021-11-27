using NUnit.Framework;
using Silphid.Showzup.Resolving;

namespace Silphid.Showzup.Test
{
    [TestFixture]
    public class ScoreEvaluatorTest
    {
        public class Temper : Variant<Temper>
        {
            public static readonly Temper Good = new Temper(nameof(Good));
            public static readonly Temper Bad = new Temper(nameof(Bad));
            public static readonly Temper Average = new Temper(nameof(Average), Good);

            public Temper(string name, Temper fallback = null)
                : base(name, fallback) {}
        }

        public class Speed : Variant<Speed>
        {
            public static readonly Speed Slow = Add(nameof(Slow));
            public static readonly Speed Fast = Add(nameof(Fast));

            protected static Speed Add(string name) =>
                new Speed(name);

            public Speed(string name)
                : base(name) {}
        }

        private class Animal : IAnimal {}

        private class Dog : Animal, IDog {}

        private interface IAnimal {}

        private interface IDog : IAnimal {}

        private readonly ScoreEvaluator _fixture = new ScoreEvaluator();

        private readonly TypeModelCollection _typeModelCollection = new TypeModelCollection();

        [Test]
        public void ExactType_MatchesWithMaximumScore()
        {
            var score = _fixture.GetTypeScore(
                _typeModelCollection.GetModelFromType(typeof(Animal)),
                _typeModelCollection.GetModelFromType(typeof(Animal)));

            Assert.That(score, Is.EqualTo(ScoreEvaluator.TypeScore));
        }

        [Test]
        public void DerivedType_MatchesWithLowerScore()
        {
            var score = _fixture.GetTypeScore(
                _typeModelCollection.GetModelFromType(typeof(Dog)),
                _typeModelCollection.GetModelFromType(typeof(Animal)));

            Assert.That(score, Is.EqualTo(ScoreEvaluator.TypeScore - ScoreEvaluator.InheritanceDepthPenalty));
        }

        [Test]
        public void ParentType_DoesNotMatchAtAll()
        {
            var score = _fixture.GetTypeScore(
                _typeModelCollection.GetModelFromType(typeof(Animal)),
                _typeModelCollection.GetModelFromType(typeof(Dog)));

            Assert.That(score, Is.Null);
        }

        [Test]
        public void InterfaceType_MatchesWithLowerScore()
        {
            var matchTypeScore = _fixture.GetTypeScore(
                _typeModelCollection.GetModelFromType(typeof(Dog)),
                _typeModelCollection.GetModelFromType(typeof(Dog)));
            var interfaceTypeScore = _fixture.GetTypeScore(
                _typeModelCollection.GetModelFromType(typeof(Dog)),
                _typeModelCollection.GetModelFromType(typeof(IAnimal)));

            Assert.That(matchTypeScore, Is.GreaterThan(interfaceTypeScore));
        }

        [Test]
        public void ParentInterfaceType_MatchesWithLowerScore()
        {
            var interfaceTypeScore = _fixture.GetTypeScore(
                _typeModelCollection.GetModelFromType(typeof(Dog)),
                _typeModelCollection.GetModelFromType(typeof(IDog)));
            var parentInterfaceTypeScore = _fixture.GetTypeScore(
                _typeModelCollection.GetModelFromType(typeof(Dog)),
                _typeModelCollection.GetModelFromType(typeof(IAnimal)));

            Assert.That(interfaceTypeScore, Is.GreaterThan(parentInterfaceTypeScore));
        }

        private readonly VariantSet Empty = VariantSet.Empty;
        private readonly VariantSet Good = new VariantSet(Temper.Good);
        private readonly VariantSet Bad = new VariantSet(Temper.Bad);
        private readonly VariantSet Average = new VariantSet(Temper.Average);
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
            Assert.That(score, Is.EqualTo(ScoreEvaluator.FallbackVariantScore - ScoreEvaluator.ExcessVariantPenalty));
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

        [Test]
        public void VariantWithFallbackMatched()
        {
            var actualExplicitFallbackScore = _fixture.GetVariantScore(Average, Good, Empty);
            var actualImplicitFallbackScore = _fixture.GetVariantScore(Average, Empty, Good);

            Assert.That(actualExplicitFallbackScore, Is.EqualTo(ScoreEvaluator.ExplicitFallbackVariantScore));
            Assert.That(actualImplicitFallbackScore, Is.EqualTo(ScoreEvaluator.ImplicitFallbackVariantScore));
        }

        [Test]
        public void VariantWithFallbackAndExcessMatched()
        {
            var actualExplicitFallbackScore = _fixture.GetVariantScore(Average, GoodAndFast, Empty);

            Assert.That(
                actualExplicitFallbackScore,
                Is.EqualTo(ScoreEvaluator.ExplicitFallbackVariantScore - ScoreEvaluator.ExcessVariantPenalty));
        }

        [Test]
        public void VariantWithExplicitFallbackScoresLowerThanExplicitScore()
        {
            var explicitFallbackScore = _fixture.GetVariantScore(Average, Good, Empty);
            var explicitScore = _fixture.GetVariantScore(Average, Average, Empty);

            Assert.That(explicitFallbackScore, Is.LessThan(explicitScore));
        }

        [Test]
        public void VariantWithExplicitFallbackScoresLowerThanImplicitScore()
        {
            var explicitFallbackScore = _fixture.GetVariantScore(Average, Good, Empty);
            var implicitScore = _fixture.GetVariantScore(Average, Empty, Average);

            Assert.That(explicitFallbackScore, Is.LessThan(implicitScore));
        }

        [Test]
        public void VariantWithImplicitFallbackScoresLowerThanExplicitScore()
        {
            var implicitFallbackScore = _fixture.GetVariantScore(Average, Empty, Good);
            var explicitScore = _fixture.GetVariantScore(Average, Average, Empty);

            Assert.That(implicitFallbackScore, Is.LessThan(explicitScore));
        }

        [Test]
        public void VariantWithImplicitFallbackScoresLowerThanImplicitScore()
        {
            var implicitFallbackScore = _fixture.GetVariantScore(Average, Empty, Good);
            var implicitScore = _fixture.GetVariantScore(Average, Empty, Average);

            Assert.That(implicitFallbackScore, Is.LessThan(implicitScore));
        }

        [Test]
        public void InterfaceScore_Of_Parent_Interface_Is_Lower()
        {
            var actualInterface = _fixture.GetTypeScore(
                _typeModelCollection.GetModelFromType(typeof(A)),
                _typeModelCollection.GetModelFromType(typeof(IA)));

            var actualParentInterface = _fixture.GetTypeScore(
                _typeModelCollection.GetModelFromType(typeof(A)),
                _typeModelCollection.GetModelFromType(typeof(IB)));

            Assert.That(actualInterface, Is.Not.Null);
            Assert.That(actualParentInterface, Is.Not.Null);
            Assert.Greater(actualInterface, actualParentInterface);
        }

        [Test]
        public void InterfaceScore_Of_Further_Interface_Is_Lower()
        {
            var actualInterface = _fixture.GetTypeScore(
                _typeModelCollection.GetModelFromType(typeof(A)),
                _typeModelCollection.GetModelFromType(typeof(IA)));

            var actual2NdLevelInterface = _fixture.GetTypeScore(
                _typeModelCollection.GetModelFromType(typeof(A)),
                _typeModelCollection.GetModelFromType(typeof(I)));

            Assert.That(actualInterface, Is.Not.Null);
            Assert.That(actual2NdLevelInterface, Is.Not.Null);
            Assert.Greater(actualInterface, actual2NdLevelInterface);
        }

        [Test]
        public void InterfaceScore_Uses_Closest_Interface_In_Tree()
        {
            /*  IF
             *  |
             *  IE     (IF, IG)
             *  |         | 
             *  I1        IA
             *  |         |
             *       A
             *
             *  IG and IF should have the same score, meaning it should take the IF from A -> IA -> IF instead of
             *  A -> I1 -> IE -> IF
             */

            var actualIf = _fixture.GetTypeScore(
                _typeModelCollection.GetModelFromType(typeof(A)),
                _typeModelCollection.GetModelFromType(typeof(IF)));

            var actualIg = _fixture.GetTypeScore(
                _typeModelCollection.GetModelFromType(typeof(A)),
                _typeModelCollection.GetModelFromType(typeof(IG)));

            Assert.That(actualIf, Is.Not.Null);
            Assert.That(actualIg, Is.Not.Null);
            Assert.AreEqual(actualIf, actualIg);
        }

        [Test]
        public void InterfaceScore_Of_Not_Implemented_Interface_Is_Null()
        {
            var actual = _fixture.GetTypeScore(
                _typeModelCollection.GetModelFromType(typeof(A)),
                _typeModelCollection.GetModelFromType(typeof(IC)));

            Assert.That(actual, Is.Null);
        }

        private class A : B, I1, IA {}

        private class B : IB {}

        private interface IA : I, IF, IG {}

        private interface I {}

        private interface IB {}

        private interface IC {}

        private interface I1 : IE {}

        private interface IE : IF {}

        private interface IF {}

        private interface IG {}
    }
}