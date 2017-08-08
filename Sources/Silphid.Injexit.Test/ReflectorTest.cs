using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Silphid.Extensions;
// ReSharper disable UnusedMember.Local
// ReSharper disable UnassignedGetOnlyAutoProperty
// ReSharper disable ClassNeverInstantiated.Local
#pragma warning disable 169

namespace Silphid.Injexit.Test
{
    [TestFixture]
    public class ReflectorTest
    {
        private class ClassWithMultipleUnmarkedConstructors
        {
            public ClassWithMultipleUnmarkedConstructors() {}
            public ClassWithMultipleUnmarkedConstructors(string str) {}
        }

        private class ClassWithMultipleMarkedConstructors
        {
            [Inject] public ClassWithMultipleMarkedConstructors() {}
            [Inject] public ClassWithMultipleMarkedConstructors(string str) {}
        }
    
        private class ClassWithMultipleConstructorsAndOnlyOneMarked
        {
            public ClassWithMultipleConstructorsAndOnlyOneMarked() {}
            [Inject] public ClassWithMultipleConstructorsAndOnlyOneMarked(string str) {}
        }
    
        private class ClassWithSomeInjectableMembers
        {
            [Inject] private string _injectablePrivateField;
            [Inject] public string InjectablePublicField;
            private string _regularField;
        
            [Inject] private string InjectablePrivateProperty { get; set; }
            [Inject] public string InjectablePublicProperty { get; set; }
            private string RegularPrivateProperty { get; set; }
            public string RegularPublicProperty { get; set; }
        
            [Inject] private void InjectablePrivateMethod(object obj, string str) {}
            [Inject] public void InjectablePublicMethod(object obj, string str) {}
            private void RegularPrivateMethod() {}
            public void RegularPublicMethod() {}
        }
    
        private class ClassWithOptionalInjectableMembers
        {
            [Inject] [Optional] private string _injectablePrivateField;
            [Inject] [Optional] public string InjectablePublicField;
            [Inject] [Optional] private string InjectablePrivateProperty { get; set; }
            [Inject] [Optional] public string InjectablePublicProperty { get; set; }
        
            [Inject] private void InjectablePrivateMethod(object obj, [Optional] string str) {}
            [Inject] public void InjectablePublicMethod(object obj, [Optional] string str) {}
        }
    
        private class ClassWithIdentifiedInjectableMembers
        {
            [Inject] [Id("FieldId")] public string InjectableField;
            [Inject] [Id("PropertyId")] public string InjectableProperty { get; set; }
        
            [Inject] public void InjectableMethod(object obj, [Id("ParameterId")] string str) {}
        }
    
        private class ClassWithNonWritableInjectableProperty
        {
            [Inject] public string InjectablePropertyWithNoSetter { get; }
        }

        private Reflector _fixture;

        [SetUp]
        public void SetUp()
        {
            _fixture = new Reflector();
        }

        [Test]
        public void ClassWithMultipleUnmarkedConstructors_MissingInjectAttributeException()
        {
            var typeInfo = GetTypeInfo<ClassWithMultipleUnmarkedConstructors>();
            
            Assert.That(
                typeInfo.Constructor.ConstructorException.Message,
                Is.EqualTo($"Type {typeInfo.Type.Name} has multiple constructors, but none of them is marked with an [Inject] attribute to make it the default."));
        }

        [Test]
        public void ClassWithMultipleMarkedConstructors_TooManyInjectAttributesException()
        {
            var typeInfo = GetTypeInfo<ClassWithMultipleMarkedConstructors>();
            
            Assert.That(
                typeInfo.Constructor.ConstructorException.Message,
                Is.EqualTo($"Type {typeInfo.Type.Name} has multiple constructors marked with an [Inject] attribute."));
        }

        [Test]
        public void ClassWithMultipleConstructorsAndOnlyOneMarked_DetectsProperConstructor()
        {
            var typeInfo = GetTypeInfo<ClassWithMultipleConstructorsAndOnlyOneMarked>();
            
            Assert.That(typeInfo.Constructor.ConstructorException, Is.Null);
            Assert.That(typeInfo.Constructor.Constructor.GetAttribute<InjectAttribute>(), Is.Not.Null);
            Assert.That(typeInfo.Constructor.Parameters.Length, Is.EqualTo(1));
            AssertMember<string>(typeInfo.Constructor.Parameters[0], "str");
        }

        [Test]
        public void ClassWithSomeInjectableMembers_ReturnsOnlyInjectables()
        {
            var typeInfo = GetTypeInfo<ClassWithSomeInjectableMembers>();

            Assert.That(typeInfo.FieldsAndProperties.Length, Is.EqualTo(4));
            AssertContainsMember<string>(typeInfo.FieldsAndProperties, "_injectablePrivateField");
            AssertContainsMember<string>(typeInfo.FieldsAndProperties, "InjectablePublicField");
            AssertContainsMember<string>(typeInfo.FieldsAndProperties, "InjectablePrivateProperty");
            AssertContainsMember<string>(typeInfo.FieldsAndProperties, "InjectablePublicProperty");
            
            Assert.That(typeInfo.Methods.Length, Is.EqualTo(2));
            
            var privateMethod = GetMethod(typeInfo.Methods, "InjectablePrivateMethod");
            Assert.That(privateMethod.Parameters.Length, Is.EqualTo(2));
            AssertMember<object>(privateMethod.Parameters[0], "obj");
            AssertMember<string>(privateMethod.Parameters[1], "str");

            var publicMethod = GetMethod(typeInfo.Methods, "InjectablePublicMethod");
            Assert.That(publicMethod.Parameters.Length, Is.EqualTo(2));
            AssertMember<object>(publicMethod.Parameters[0], "obj");
            AssertMember<string>(publicMethod.Parameters[1], "str");
        }

        [Test]
        public void ClassWithOptionalInjectableMembers_OptionalsAreCorrectlyDetected()
        {
            var typeInfo = GetTypeInfo<ClassWithOptionalInjectableMembers>();

            Assert.That(typeInfo.FieldsAndProperties.Length, Is.EqualTo(4));
            AssertContainsMember<string>(typeInfo.FieldsAndProperties, "_injectablePrivateField", true);
            AssertContainsMember<string>(typeInfo.FieldsAndProperties, "InjectablePublicField", true);
            AssertContainsMember<string>(typeInfo.FieldsAndProperties, "InjectablePrivateProperty", true);
            AssertContainsMember<string>(typeInfo.FieldsAndProperties, "InjectablePublicProperty", true);
            
            Assert.That(typeInfo.Methods.Length, Is.EqualTo(2));
            
            var privateMethod = GetMethod(typeInfo.Methods, "InjectablePrivateMethod");
            Assert.That(privateMethod.Parameters.Length, Is.EqualTo(2));
            AssertMember<object>(privateMethod.Parameters[0], "obj");
            AssertMember<string>(privateMethod.Parameters[1], "str", true);

            var publicMethod = GetMethod(typeInfo.Methods, "InjectablePublicMethod");
            Assert.That(publicMethod.Parameters.Length, Is.EqualTo(2));
            AssertMember<object>(publicMethod.Parameters[0], "obj");
            AssertMember<string>(publicMethod.Parameters[1], "str", true);
        }

        [Test]
        public void ClassWithIdentifiedInjectableMembers_IdsAreCorrectlyDetected()
        {
            var typeInfo = GetTypeInfo<ClassWithIdentifiedInjectableMembers>();

            Assert.That(typeInfo.FieldsAndProperties.Length, Is.EqualTo(2));
            AssertContainsMember<string>(typeInfo.FieldsAndProperties, "InjectableField", false, "FieldId");
            AssertContainsMember<string>(typeInfo.FieldsAndProperties, "InjectableProperty", false, "PropertyId");
            
            Assert.That(typeInfo.Methods.Length, Is.EqualTo(1));
            var method = GetMethod(typeInfo.Methods, "InjectableMethod");
            Assert.That(method.Parameters.Length, Is.EqualTo(2));
            AssertMember<object>(method.Parameters[0], "obj");
            AssertMember<string>(method.Parameters[1], "str", false, "ParameterId");
        }
        
        [Test]
        public void ClassWithNonWritableInjectableProperty_Throws()
        {
            var exception =
                Assert.Throws<InvalidOperationException>(() => GetTypeInfo<ClassWithNonWritableInjectableProperty>());

            const string className = nameof(ClassWithNonWritableInjectableProperty);
            const string propertyName = nameof(ClassWithNonWritableInjectableProperty.InjectablePropertyWithNoSetter);
            Assert.That(exception.Message, Is.EqualTo($"Property {className}.{propertyName} is marked with [Inject] attribute, but has not setter."));
        }

        private InjectMethodInfo GetMethod(IEnumerable<InjectMethodInfo> methods, string name) =>
            methods.Single(x => x.Name == name);

        private void AssertContainsMember<T>(IEnumerable<InjectMemberInfo> members, string name, bool isOptional = false, string id = null)
        {
            var member = members.Single(x => x.Name == name);
            AssertMember<T>(member, name, isOptional, id);
        }

        private void AssertMember<T>(InjectMemberInfo member, string name, bool isOptional = false, string id = null)
        {
            Assert.That(member.Name, Is.EqualTo(name));
            Assert.That(member.Type, Is.EqualTo(typeof(T)));
            Assert.That(member.IsOptional, Is.EqualTo(isOptional));
            Assert.That(member.Id, Is.EqualTo(id));
        }
        
        private InjectTypeInfo GetTypeInfo<T>() =>
            _fixture.GetTypeInfo(typeof(T));
    }
}