using NUnit.Framework;
using Silphid.Showzup.Layout;
using Silphid.Tests;
using UniRx;

namespace Silphid.Showzup.Test.Layout
{
    [TestFixture]
    public class ReactivePropertyThreeWayBindingTest
    {
        private ReactiveProperty<float> _target;
        private ReactiveProperty<float> _dependentSource;
        private ReactiveProperty<float> _independentSource;
        private CompositeDisposable _disposables;

        private const float TargetInitial = 1;
        private const float DependentSourceInitial = 2;
        private const float IndependentSourceInitial = 3;
        private const float NewValue1 = 5;
        private const float NewValue2 = 6;
        private const float NewValue3 = 7;

        [SetUp]
        public void SetUp()
        {
            _target = new ReactiveProperty<float>(TargetInitial);
            _dependentSource = new ReactiveProperty<float>(DependentSourceInitial);
            _independentSource = new ReactiveProperty<float>(IndependentSourceInitial);
            _disposables = new CompositeDisposable();
        }

        [TearDown]
        public void TearDown()
        {
            _disposables.Dispose();
        }

        private void SetUpBinding()
        {
            _disposables.Add(
                _target.BindThreeWayTo(
                    _dependentSource,
                    _independentSource,
                    (variable, independent) => variable + independent,
                    (target, independent) => target - independent));
        }

        [Test]
        public void InitialSourceValuesPropagateToTargetUponBinding()
        {
            SetUpBinding();

            _target.Value.Is(DependentSourceInitial + IndependentSourceInitial);
            _dependentSource.Value.Is(DependentSourceInitial);
            _independentSource.Value.Is(IndependentSourceInitial);
        }

        [Test]
        public void ChangeToDependentSourcePropagatesToTargetButNotToIndependentSource()
        {
            SetUpBinding();

            _dependentSource.Value = NewValue1;

            _target.Value.Is(NewValue1 + IndependentSourceInitial);
            _dependentSource.Value.Is(NewValue1);
            _independentSource.Value.Is(IndependentSourceInitial);
        }

        [Test]
        public void ChangeToIndependentSourcePropagatesToTargetButNotToDependentSource()
        {
            SetUpBinding();

            _independentSource.Value = NewValue1;

            _target.Value.Is(DependentSourceInitial + NewValue1);
            _dependentSource.Value.Is(DependentSourceInitial);
            _independentSource.Value.Is(NewValue1);
        }

        [Test]
        public void ChangeToTargetPropagatesToDependentSourceButNotToIndependentSource()
        {
            SetUpBinding();

            _target.Value = NewValue1;

            _target.Value.Is(NewValue1);
            _dependentSource.Value.Is(NewValue1 - IndependentSourceInitial);
            _independentSource.Value.Is(IndependentSourceInitial);
        }

        [Test]
        public void DisposingDisconnectsBindingInBothDirections()
        {
            SetUpBinding();

            _disposables.Dispose();

            _target.Value = NewValue1;
            
            _target.Value.Is(NewValue1);
            _dependentSource.Value.Is(DependentSourceInitial);
            _independentSource.Value.Is(IndependentSourceInitial);

            _dependentSource.Value = NewValue2;
            
            _target.Value.Is(NewValue1);
            _dependentSource.Value.Is(NewValue2);
            _independentSource.Value.Is(IndependentSourceInitial);

            _independentSource.Value = NewValue3;
            
            _target.Value.Is(NewValue1);
            _dependentSource.Value.Is(NewValue2);
            _independentSource.Value.Is(NewValue3);
        }
    }
}