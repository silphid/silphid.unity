using NUnit.Framework;

namespace Silphid.Showzup.Test
{
    [TestFixture]
    public class ScoreEvaluatorTest
    {
        public class Temper : Variant<Temper>
        {
            public static readonly Temper Good = Create();
            public static readonly Temper Bad = Create();
        }

        private class Animal {}
        private class Dog : Animal {}
        
        private readonly ScoreEvaluator _fixture = new ScoreEvaluator();

        [Test]
        public void ExactType_MatchesWithMaximumScore()
        {
            var score = _fixture.GetTypeScore(typeof(Animal), typeof(Animal));
            
            Assert.That(score, Is.EqualTo(ScoreEvaluator.MatchedVariantScore));
        }

        [Test]
        public void DerivedType_MatchesWithLowerScore()
        {
            var score = _fixture.GetTypeScore(typeof(Animal), typeof(Dog));
            
            Assert.That(score, Is.EqualTo(ScoreEvaluator.MatchedTypeScore - ScoreEvaluator.TypeInheritanceDepthScorePenality));
        }

        [Test]
        public void ParentType_DoesNotMatchAtAll()
        {
            var score = _fixture.GetTypeScore(typeof(Dog), typeof(Animal));
            
            Assert.That(score, Is.Null);
        }

        [Test]
        public void MatchedVariant()
        {
            var score = _fixture.GetVariantScore();
            
            Assert.That(score, Is.Null);
        }
    }
}