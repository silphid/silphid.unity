using JetBrains.Annotations;
using NUnit.Framework;

namespace Silphid.Injexit.Test
{
    [TestFixture]
    public class Id
    {
        private class Service
        {
        }
        
        private class FieldInjectionWithId
        {
            [Inject]
            [UsedImplicitly]
            internal Service Service1;
            
            [Inject]
            [UsedImplicitly]
            public Service Service2;
        }
        
        private class ConstructorInjectionWithId
        {
            public Service Service1 { get; private set; }
            public Service Service2 { get; private set; }

            [Inject]
            [UsedImplicitly]
            public void Inject(Service service1, Service service2)
            {
                Service1 = service1;
                Service2 = service2;
            }
        }

        private IContainer _fixture;
        private readonly Service _service1 = new Service();
        private readonly Service _service2 = new Service();

        [SetUp]
        public void SetUp()
        {
            _fixture = new Container(new Reflector());
            
            _fixture.BindInstance(_service1).Named("Service1");
            _fixture.BindInstance(_service2).Named("Service2");
            _fixture.BindToSelf<FieldInjectionWithId>();
            _fixture.BindToSelf<ConstructorInjectionWithId>();
        }
        
        [Test]
        public void Field_injection_with_id()
        {
            var instance = _fixture.Resolve<FieldInjectionWithId>();
            
            Assert.That(instance.Service1, Is.SameAs(_service1));
            Assert.That(instance.Service2, Is.SameAs(_service2));
        }
        
        [Test]
        public void Constructor_injection_with_id()
        {
            var instance = _fixture.Resolve<ConstructorInjectionWithId>();
            
            Assert.That(instance.Service1, Is.SameAs(_service1));
            Assert.That(instance.Service2, Is.SameAs(_service2));
        }
    }
}