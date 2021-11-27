using NUnit.Framework;
using Silphid.Showzup.Layout;
using Silphid.Tests;
using UniRx;

namespace Silphid.Showzup.Test.Layout
{
    [TestFixture]
    public class ReactivePropertyTwoWayBindingTest
    {
        private ReactiveProperty<float> _prop1;
        private ReactiveProperty<float> _prop2;
        private CompositeDisposable _disposables;

        private const float InitialValue1 = 1;
        private const float InitialValue2 = 2;
        private const float Offset = 3;
        private const float NewValue1 = 4;
        private const float NewValue2 = 5;

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
            _disposables.Add(_prop1.BindTwoWayTo(_prop2, Offset));
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

            _prop2.Value = NewValue1;

            _prop1.Value.Is(NewValue1 + Offset);
            _prop2.Value.Is(NewValue1);
        }

        [Test]
        public void ChangePropagatesFromTargetToSource()
        {
            SetUpBinding();

            _prop1.Value = NewValue1;

            _prop1.Value.Is(NewValue1);
            _prop2.Value.Is(NewValue1 - Offset);
        }

        [Test]
        public void DisposingDisconnectsBindingInBothDirections()
        {
            SetUpBinding();

            _disposables.Dispose();

            _prop1.Value = NewValue1;
            _prop1.Value.Is(NewValue1);
            _prop2.Value.Is(InitialValue2);

            _prop2.Value = NewValue2;
            _prop2.Value.Is(NewValue2);
            _prop1.Value.Is(NewValue1);
        }
    }
}