using NUnit.Framework;
#pragma warning disable 649

namespace Silphid.Injexit.Test
{
    [TestFixture]
    public class InjectorTest
    {
        private class BaseClass
        {
            public int Value => _value;
            
            [Inject] private int _value;
        }
        
        private class SubClass : BaseClass
        {
        }
        
        private Container _fixture;

        [SetUp]
        public void SetUp()
        {
            _fixture = new Container(new Reflector());
        }

        [Test]
        public void InjectingSubClass_ShouldInjectPrivateFieldInBaseClass()
        {
            _fixture.BindInstance(123);
            
            var instance = new SubClass();
            _fixture.Inject(instance);
            
            Assert.That(instance.Value, Is.EqualTo(123));
        }
    }
}