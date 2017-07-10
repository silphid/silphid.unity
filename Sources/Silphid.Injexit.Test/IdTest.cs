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
            [Inject(Id = "Service1")]
            [UsedImplicitly]
            internal Service Service1;
            
            [Inject(Id = "Service2")]
            [UsedImplicitly]
            public Service Service2;
        }
        
        private class ConstructorInjectionWithId
        {
            public Service Service3 { get; private set; }

            [Inject]
            [UsedImplicitly]
            public void Inject([Inject(Id = "Service3")] Service service3)
            {
                Service3 = service3;
            }
        }

        private IContainer _fixture;
        private readonly Service _service1 = new Service();
        private readonly Service _service2 = new Service();
        private readonly Service _service3 = new Service();

        [SetUp]
        public void SetUp()
        {
            _fixture = new Container();
            
            _fixture.BindInstance(_service1).WithId("Service1");
            _fixture.BindInstance(_service2).WithId("Service2");
            _fixture.BindInstance(_service3).WithId("Service3");
            _fixture.BindSelf<FieldInjectionWithId>();
            _fixture.BindSelf<ConstructorInjectionWithId>();
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
            
            Assert.That(instance.Service3, Is.SameAs(_service3));
        }
    }
}