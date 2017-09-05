using NUnit.Framework;
// ReSharper disable ClassNeverInstantiated.Local
// ReSharper disable ConvertToConstant.Local
// ReSharper disable FieldCanBeMadeReadOnly.Local
#pragma warning disable 169

namespace Silphid.Injexit.Test
{
    [TestFixture]
    public class OptionalTest
    {
        private class ClassWithOptionalText
        {
            [Inject] [Optional] public string Text = "OriginalValue";
        }
        
        private class ClassWithRequiredText
        {
            [Inject] public string Text;
        }
        
        private class ClassWithOptionalParameterInConstructor
        {
            public string Text { get; }
            
            public ClassWithOptionalParameterInConstructor([Optional] string text)
            {
                Text = text;
            }
        }
        
        private class ClassWithDefaultValueParameterInConstructor
        {
            public string Text { get; }
            
            public ClassWithDefaultValueParameterInConstructor(string text = "ParameterDefault")
            {
                Text = text;
            }
        }

        private IContainer _fixture;

        [SetUp]
        public void SetUp()
        {
            _fixture = new Container(new Reflector());
            _fixture.BindToSelf<ClassWithOptionalText>();
            _fixture.BindToSelf<ClassWithRequiredText>();
            _fixture.BindToSelf<ClassWithOptionalParameterInConstructor>();
            _fixture.BindToSelf<ClassWithDefaultValueParameterInConstructor>();
        }
        
        [Test]
        public void MissingBindingForOptionalText_ShouldLeaveOriginalValueAsIs()
        {
            var value = _fixture.Resolve<ClassWithOptionalText>();
            Assert.That(value.Text, Is.EqualTo("OriginalValue"));
        }
        
        [Test]
        public void MissingBindingForRequiredText_ShouldThrow()
        {
            Assert.Throws<UnresolvedDependencyException>(() =>
                _fixture.Resolve<ClassWithRequiredText>());
        }
        
        [Test]
        public void MissingBindingForOptionalParameter_ShouldInjectDefaultValueOfParameterType()
        {
            var value = _fixture.Resolve<ClassWithOptionalParameterInConstructor>();
            Assert.That(value.Text, Is.Null);
        }
        
        [Test]
        public void MissingBindingForDefaultValueParameter_ShouldInjectDefaultValueDefinedByParameter()
        {
            var value = _fixture.Resolve<ClassWithDefaultValueParameterInConstructor>();
            Assert.That(value.Text, Is.EqualTo("ParameterDefault"));
        }
    }
}