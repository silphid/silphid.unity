using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Silphid.Showzup.Injection;

namespace Silphid.Showzup.Test
{
    [TestFixture]
    public class ContainerTest
    {
        private interface IBeing {}

        private class Being : IBeing
        {
            private bool Equals(Being other) =>
                GetType() == other.GetType();

            public override bool Equals(object obj)
            {
                if (ReferenceEquals(null, obj)) return false;
                return ReferenceEquals(this, obj) || Equals((Being) obj);
            }

            public override int GetHashCode() =>
                GetType().GetHashCode();
        }
        
        private class Human : Being {}
        private class Man : Human {}
        private class Woman : Human {}
        private class Dog : Being {}

        private readonly Dog _dog = new Dog();
        
        [Test]
        public void BindAsList()
        {
            var container = new Container();
            container.BindAsList<IBeing, Man>();
            container.BindSingleAsList<IBeing, Woman>();
            container.BindInstanceAsList<IBeing>(_dog);

            var list1 = container.ResolveInstance<List<IBeing>>();
            var man1 = list1.OfType<Man>().FirstOrDefault();
            var woman1 = list1.OfType<Woman>().FirstOrDefault();
            var dog1 = list1.OfType<Dog>().FirstOrDefault();
            
            Assert.That(list1, Is.EquivalentTo(new IBeing[]{ new Man(), new Woman(), _dog }));
            Assert.That(dog1, Is.SameAs(_dog));
            
            // Try same resolve a second time
            var list2 = container.ResolveInstance<List<IBeing>>();
            var man2 = list2.OfType<Man>().FirstOrDefault();
            var woman2 = list2.OfType<Woman>().FirstOrDefault();
            var dog2 = list2.OfType<Dog>().FirstOrDefault();

            // Should be same woman and same dog
            Assert.That(woman2, Is.SameAs(woman1));
            Assert.That(dog2, Is.SameAs(dog1));

            // Should be different man
            Assert.That(man2, Is.Not.SameAs(man1));
        }
    }
}