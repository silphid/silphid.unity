using NUnit.Framework;
using Silphid.Showzup.Layout;
using Silphid.Tests;
using UniRx;

namespace Silphid.Showzup.Test.Layout
{
    [TestFixture]
    public class ReactivePropertyOneWayBindingTest
    {
        private ReactiveProperty<float> _prop1;
        private ReactiveProperty<float> _prop2;
        private CompositeDisposable _disposables;

        private const float InitialValue1 = 1;
        private const float InitialValue2 = 2;
        private const float Offset = 3;
        private const float NewValue = 4;

        [SetUp]
        public void SetUp()
        {
            _prop1 = new ReactiveProperty<float>(InitialValue1);
            _prop2 = new ReactiveProperty<float>(InitialValue2);
            _disposables = new CompositeDisposable();
        }

        [TearDown]
        public void TearDown()
        {
            _disposables.Dispose();
        }

        private void SetUpBinding()
        {
            _disposables.Add(_prop1.BindTo(_prop2, Offset));
        }

        [Test]
        public void InitialSourceValuePropagatesToTargetUponBinding()
        {
            SetUpBinding();

            _prop1.Value.Is(InitialValue2 + Offset);
            _prop2.Value.Is(InitialValue2);
        }

        [Test]
        public void ChangePropagatesFromSourceToTarget()
        {
            SetUpBinding();

            _prop2.Value = NewValue;

            _prop1.Value.Is(NewValue + Offset);
            _prop2.Value.Is(NewValue);
        }

        [Test]
        public void ChangeDoesNotPropagateFromTargetToSource()
        {
            SetUpBinding();

            _prop1.Value = NewValue;

            _prop1.Value.Is(NewValue);
            _prop2.Value.Is(InitialValue2);
        }

        [Test]
        public void DisposingDisconnectsBinding()
        {
            SetUpBinding();

            _disposables.Dispose();
            _prop1.Value = NewValue;

            _prop2.Value.Is(InitialValue2);
        }
    }
}