using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Silphid.Extensions;
// ReSharper disable UnusedMember.Local
// ReSharper disable UnassignedGetOnlyAutoProperty
// ReSharper disable ClassNeverInstantiated.Local
// ReSharper disable UnusedAutoPropertyAccessor.Local
// ReSharper disable MemberCanBePrivate.Local
#pragma warning disable 649
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
            [Inject] public void InjectablePublicMethod(object obj, [Optional] string str, object obj2 = null) {}
        }
    
        private class ClassWithIdentifiedInjectableMembers
        {
            [Inject] public string InjectableField;
            [Inject] public string InjectableProperty { get; set; }
        
            [Inject] public void InjectableMethod(object obj, string str) {}
        }
    
        private class ClassWithNonWritableInjectableProperty
        {
            [Inject] public string InjectablePropertyWithNoSetter { get; }
        }
        
        private class ClassWithInjectableInternalField
        {
            public int Value => _value;
            
            [Inject] internal int _value;
        }
        
        private class ClassWithInheritedInjectableInternalField : ClassWithInjectableInternalField
        {
        }
        
        private class ClassWithInjectableInternalMethod
        {
            [Inject]
            internal void Inject(int value)
            {
            }
        }
        
        private class ClassWithInheritedInjectableInternalMethod : ClassWithInjectableInternalMethod
        {
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
            AssertMember<string>(typeInfo.Constructor.Parameters[0], "str", "Str");
        }

        [Test]
        public void ClassWithSomeInjectableMembers_ReturnsOnlyInjectables()
        {
            var typeInfo = GetTypeInfo<ClassWithSomeInjectableMembers>();

            Assert.That(typeInfo.FieldsAndProperties.Length, Is.EqualTo(4));
            AssertContainsMember<string>(typeInfo.FieldsAndProperties, "_injectablePrivateField", "InjectablePrivateField");
            AssertContainsMember<string>(typeInfo.FieldsAndProperties, "InjectablePublicField", "InjectablePublicField");
            AssertContainsMember<string>(typeInfo.FieldsAndProperties, "InjectablePrivateProperty", "InjectablePrivateProperty");
            AssertContainsMember<string>(typeInfo.FieldsAndProperties, "InjectablePublicProperty", "InjectablePublicProperty");
            
            Assert.That(typeInfo.Methods.Length, Is.EqualTo(2));
            
            var privateMethod = GetMethod(typeInfo.Methods, "InjectablePrivateMethod");
            Assert.That(privateMethod.Parameters.Length, Is.EqualTo(2));
            AssertMember<object>(privateMethod.Parameters[0], "obj", "Obj");
            AssertMember<string>(privateMethod.Parameters[1], "str", "Str");

            var publicMethod = GetMethod(typeInfo.Methods, "InjectablePublicMethod");
            Assert.That(publicMethod.Parameters.Length, Is.EqualTo(2));
            AssertMember<object>(publicMethod.Parameters[0], "obj", "Obj");
            AssertMember<string>(publicMethod.Parameters[1], "str", "Str");
        }

        [Test]
        public void ClassWithOptionalInjectableMembers_OptionalsAreCorrectlyDetected()
        {
            var typeInfo = GetTypeInfo<ClassWithOptionalInjectableMembers>();

            Assert.That(typeInfo.FieldsAndProperties.Length, Is.EqualTo(4));
            AssertContainsMember<string>(typeInfo.FieldsAndProperties, "_injectablePrivateField", "InjectablePrivateField", true);
            AssertContainsMember<string>(typeInfo.FieldsAndProperties, "InjectablePublicField", "InjectablePublicField", true);
            AssertContainsMember<string>(typeInfo.FieldsAndProperties, "InjectablePrivateProperty", "InjectablePrivateProperty", true);
            AssertContainsMember<string>(typeInfo.FieldsAndProperties, "InjectablePublicProperty", "InjectablePublicProperty", true);
            
            Assert.That(typeInfo.Methods.Length, Is.EqualTo(2));
            
            var privateMethod = GetMethod(typeInfo.Methods, "InjectablePrivateMethod");
            Assert.That(privateMethod.Parameters.Length, Is.EqualTo(2));
            AssertMember<object>(privateMethod.Parameters[0], "obj", "Obj");
            AssertMember<string>(privateMethod.Parameters[1], "str", "Str", true);

            var publicMethod = GetMethod(typeInfo.Methods, "InjectablePublicMethod");
            Assert.That(publicMethod.Parameters.Length, Is.EqualTo(3));
            AssertMember<object>(publicMethod.Parameters[0], "obj", "Obj");
            AssertMember<string>(publicMethod.Parameters[1], "str", "Str", true);
            AssertMember<object>(publicMethod.Parameters[2], "obj2", "Obj2", true);
        }

        [Test]
        public void ClassWithIdentifiedInjectableMembers_IdsAreCorrectlyDetected()
        {
            var typeInfo = GetTypeInfo<ClassWithIdentifiedInjectableMembers>();

            Assert.That(typeInfo.FieldsAndProperties.Length, Is.EqualTo(2));
            AssertContainsMember<string>(typeInfo.FieldsAndProperties, "InjectableField", "InjectableField");
            AssertContainsMember<string>(typeInfo.FieldsAndProperties, "InjectableProperty", "InjectableProperty");
            
            Assert.That(typeInfo.Methods.Length, Is.EqualTo(1));
            var method = GetMethod(typeInfo.Methods, "InjectableMethod");
            Assert.That(method.Parameters.Length, Is.EqualTo(2));
            AssertMember<object>(method.Parameters[0], "obj", "Obj");
            AssertMember<string>(method.Parameters[1], "str", "Str");
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
        
        [Test]
        public void ClassWithInheritedInjectableInternalField_InheritedFieldShouldBeDiscovered()
        {
            var typeInfo = GetTypeInfo<ClassWithInheritedInjectableInternalField>();

            Assert.That(typeInfo.FieldsAndProperties.Length, Is.EqualTo(1));
            AssertMember<int>(typeInfo.FieldsAndProperties[0], "_value", "Value");
        }
        
        [Test]
        public void ClassWithInheritedInjectableInternalMethod_InheritedMethodShouldBeDiscovered()
        {
            var typeInfo = GetTypeInfo<ClassWithInheritedInjectableInternalMethod>();

            Assert.That(typeInfo.Methods.Length, Is.EqualTo(1));
            var method = GetMethod(typeInfo.Methods, "Inject");
            Assert.That(method.Parameters.Length, Is.EqualTo(1));
            AssertMember<int>(method.Parameters[0], "value", "Value");
        }

        private InjectMethodInfo GetMethod(IEnumerable<InjectMethodInfo> methods, string name) =>
            methods.Single(x => x.Name == name);

        private void AssertContainsMember<T>(IEnumerable<InjectMemberInfo> members, string name, string canonicalName, bool isOptional = false)
        {
            var member = members.Single(x => x.Name == name);
            AssertMember<T>(member, name, canonicalName, isOptional);
        }

        private void AssertMember<T>(InjectMemberInfo member, string name, string canonicalName, bool isOptional = false)
        {
            Assert.That(member.Name, Is.EqualTo(name));
            Assert.That(member.Type, Is.EqualTo(typeof(T)));
            Assert.That(member.IsOptional, Is.EqualTo(isOptional));
            Assert.That(member.CanonicalName, Is.EqualTo(canonicalName));
        }

        private InjectTypeInfo GetTypeInfo<T>() =>
            _fixture.GetTypeInfo(typeof(T));
    }
}