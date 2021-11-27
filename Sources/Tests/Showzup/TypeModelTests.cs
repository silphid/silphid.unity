using System.IO;
using System.Linq;
using NUnit.Framework;

namespace Silphid.Showzup.Test
{
    [TestFixture]
    public class TypeModelTests
    {
        private const string FileName = "test.yaml";
        private string FilePath => Path.Combine(Path.GetTempPath(), FileName); 
        
        [Test]
        public void TypeModel_Has_Name()
        {
            var sut = new TypeModelCollection();

            var actual = sut.GetModelFromType(typeof(A));

            Assert.That(actual.Name, Contains.Substring("Silphid.Showzup.Test.TypeModelTests+A"));
        }
        
        [Test]
        public void TypeModel_Has_Interfaces()
        {
            var sut = new TypeModelCollection();

            var actual = sut.GetModelFromType(typeof(A));

            Assert.That(
                actual.InterfaceNames.Any(i => i.Contains("Silphid.Showzup.Test.TypeModelTests+IA")),
                "Silphid.Showzup.Test.TypeModelTests+IA");
            Assert.That(
                actual.InterfaceNames.Any(i => i.Contains("Silphid.Showzup.Test.TypeModelTests+IE")),
                "Silphid.Showzup.Test.TypeModelTests+IE");
            Assert.That(actual.InterfaceNames, Has.No.Member("Silphid.Showzup.Test.TypeModelTests+IF"));
        }
        
        [Test]
        public void ClassModel_Has_Parent()
        {
            var sut = new TypeModelCollection();

            var actual = sut.GetModelFromType(typeof(A));

            Assert.That(actual is ClassModel);
            
            var classModel = actual as ClassModel;
            Assert.That(classModel.ParentName, Contains.Substring("Silphid.Showzup.Test.TypeModelTests+B"));
        }

        [Test]
        public void ClassModel_Parent_Has_Interfaces()
        {
            var sut = new TypeModelCollection();

            var actual = sut.GetModelFromType(typeof(A));
            
            Assert.That(actual is ClassModel);

            var classModel = actual as ClassModel;
            Assert.That(
                classModel.Parent.InterfaceNames.Any(i => i.Contains("Silphid.Showzup.Test.TypeModelTests+IB")),
                "Silphid.Showzup.Test.TypeModelTests+IB");
            Assert.That(
                classModel.Parent.InterfaceNames.Any(i => i.Contains("Silphid.Showzup.Test.TypeModelTests+ID")),
                "Silphid.Showzup.Test.TypeModelTests+ID");
            Assert.That(
                classModel.Parent.InterfaceNames.Any(i => i.Contains("Silphid.Showzup.Test.TypeModelTests+IF")),
                "Silphid.Showzup.Test.TypeModelTests+IF");
        }

        [Test]
        public void TypeModelCollection_Can_Be_RoundTripped()
        {
            var sut = new TypeModelCollection();

            sut.Save(FilePath);
            TypeModelCollection.Load(FilePath);

            Assert.That(File.Exists(FilePath), "File exists");
        }

        [TearDown]
        public void TearDown()
        {
            if(File.Exists(FilePath))
                File.Delete(FilePath);
        }
        
        private class A : B, IA, IE {}

        private class B : IB, ID, IF {}

        private interface IA {}

        private interface IB {}

        private interface IC {}

        private interface ID {}

        private interface IE : IC {}

        private interface IF {}
    }
}