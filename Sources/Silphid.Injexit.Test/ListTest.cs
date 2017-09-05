using System;
using System.Collections.Generic;
using NUnit.Framework;
// ReSharper disable ClassNeverInstantiated.Local

namespace Silphid.Injexit.Test
{
    [TestFixture]
    public class ListTest
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

        private class ManWithDog : Man
        {
            public Dog Dog { get; }

            public ManWithDog(Dog dog) { Dog = dog; }
        }

        private readonly Dog _dog = new Dog();
        
        [Test]
        public void BindAsList_OrderIsRespected()
        {
            var container = new Container(new Reflector());
            container.Bind<IBeing, Man>().AsList();
            container.Bind<IBeing, Woman>().AsSingle().AsList();
            container.BindInstance<IBeing>(_dog).AsList();

            var list1 = container.Resolve<List<IBeing>>();
            var man1 = list1[0];
            var woman1 = list1[1];
            var dog1 = list1[2];
            
            Assert.That(list1, Is.EqualTo(new IBeing[]{ new Man(), new Woman(), _dog }));
            
            // Try same resolve a second time
            var list2 = container.Resolve<List<IBeing>>();
            var man2 = list2[0];
            var woman2 = list2[1];
            var dog2 = list2[2];

            // Should be same woman and same dog
            Assert.That(woman2, Is.SameAs(woman1));
            Assert.That(dog2, Is.SameAs(dog1));

            // Should be different man
            Assert.That(man2, Is.Not.SameAs(man1));
        }
        
        [Test]
        public void BindAsListWithNewSyntax()
        {
            var container = new Container(new Reflector());
            container.BindList<IBeing>(x =>
            {
                x.Add<Man>();
                x.Add<Woman>();
                x.AddInstance(_dog);
            });

            var list = container.Resolve<List<IBeing>>();
            Assert.That(list, Is.EqualTo(new IBeing[]{ new Man(), new Woman(), _dog }));
            Assert.That(list[2], Is.SameAs(_dog));
        }
        
        [Test]
        public void BindAsList_AddNullInstance_Throws()
        {
            var container = new Container(new Reflector());
            Assert.Throws<ArgumentNullException>(() =>
                container.BindList<IBeing>(x =>
                    x.AddInstance<Man>(null)));
        }
        
        [Test]
        public void BindManyTimeTheSameTypeAsList_OrderShouldBeRespected()
        {
            var container = new Container(new Reflector());
            container.Bind<Being, Man>().AsList();
            container.Bind<Being, Man>().AsList();
            container.Bind<Being, Woman>().AsList();
            container.Bind<Being, Man>().AsList();
            container.Bind<Being, Woman>().AsList();
            container.Bind<Being, Woman>().AsList();

            var list = container.Resolve<List<Being>>();
            
            Assert.That(list.Count, Is.EqualTo(6));
            Assert.That(list[0], Is.TypeOf<Man>());
            Assert.That(list[1], Is.TypeOf<Man>());
            Assert.That(list[2], Is.TypeOf<Woman>());
            Assert.That(list[3], Is.TypeOf<Man>());
            Assert.That(list[4], Is.TypeOf<Woman>());
            Assert.That(list[5], Is.TypeOf<Woman>());
        }
        
        [Test]
        public void BindTwoListsWithDifferentIdsWithNewSyntax()
        {
            var container = new Container(new Reflector());

            container
                .BindList<Being>(x =>
                {
                    x.Add<Man>();
                    x.Add<Man>();
                    x.Add<Woman>();
                })
                .WithId("List1");

            container
                .BindList<Being>(x =>
                {
                    x.Add<Woman>();
                    x.Add<Man>();
                })
                .WithId("List2");

            container
                .Bind<Being, Woman>()
                .AsList()
                .WithId("List2");
            
            var list1 = container.Resolve<List<Being>>("List1");
            Assert.That(list1.Count, Is.EqualTo(3));
            Assert.That(list1[0], Is.TypeOf<Man>());
            Assert.That(list1[1], Is.TypeOf<Man>());
            Assert.That(list1[2], Is.TypeOf<Woman>());

            var list2 = container.Resolve<List<Being>>("List2");
            Assert.That(list2.Count, Is.EqualTo(3));
            Assert.That(list2[0], Is.TypeOf<Woman>());
            Assert.That(list2[1], Is.TypeOf<Man>());
            Assert.That(list2[2], Is.TypeOf<Woman>());
        }
        
        [Test]
        public void BindManWithDogAsListUsingDogInstance()
        {
            var container = new Container(new Reflector());

            container
                .BindList<Being>(x =>
                {
                    x.Add<ManWithDog>().UsingInstance(_dog);
                    x.Add<Man>();
                });
            
            var list = container.Resolve<List<Being>>();
            
            Assert.That(list.Count, Is.EqualTo(2));
            Assert.That(list[0], Is.TypeOf<ManWithDog>());
            Assert.That(((ManWithDog)list[0]).Dog, Is.SameAs(_dog));
        }
    }
}