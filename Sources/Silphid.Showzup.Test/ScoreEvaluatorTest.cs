using NUnit.Framework;

namespace Silphid.Showzup.Test
{
    [TestFixture]
    public class ScoreEvaluatorTest
    {
        public class Display : Variant<Display>
        {
            public static readonly Display Page = new Display();
            public static readonly Display Background = new Display();
            public static readonly Display Panel = new Display();
            public static readonly Display Tile = new Display();
            public static readonly Display Thumbnail = new Display();
            public static readonly Display Item = new Display();
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