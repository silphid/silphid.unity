using NUnit.Framework;

namespace Silphid.Showzup.Test
{
    [TestFixture]
    public class ScoreEvaluatorTest
    {
        public class Display : Variant<Display>
        {
            public static readonly Display Page = Create();
            public static readonly Display Background = Create();
            public static readonly Display Panel = Create();
            public static readonly Display Tile = Create();
            public static readonly Display Thumbnail = Create();
            public static readonly Display Item = Create();
        }

        private class ParentClass {}
        private class ChildClass : ParentClass {}
        
        private readonly ScoreEvaluator _fixture = new ScoreEvaluator();

        [Test]
        public void ExactType_MatchesWithMaximumScore()
        {
            var score = _fixture.GetTypeScore(typeof(ParentClass), typeof(ParentClass));
            
            Assert.That(score, Is.EqualTo(ScoreEvaluator.ExactMatchScore));
        }

        [Test]
        public void DerivedType_MatchesWithLowerScore()
        {
            var score = _fixture.GetTypeScore(typeof(ParentClass), typeof(ChildClass));
            
            Assert.That(score, Is.EqualTo(ScoreEvaluator.ExactMatchScore - ScoreEvaluator.InheritanceScorePenality));
        }

        [Test]
        public void ParentType_DoesNotMatchAtAll()
        {
            var score = _fixture.GetTypeScore(typeof(ChildClass), typeof(ParentClass));
            
            Assert.That(score, Is.Null);
        }
    }
}