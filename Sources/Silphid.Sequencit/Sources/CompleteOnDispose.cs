using System;
using UniRx;
using Rx = UniRx;

namespace Silphid.Sequencit
{
    public class CompleteOnDispose : Rx.IObservable<Unit>, IDisposable
    {
        public static CompleteOnDispose Create(Action<CompleteOnDispose> action)
        {
            var completion = new CompleteOnDispose();
            action(completion);
            return completion;
        }

        private Subject<Unit> _subject;

        public IDisposable Subscribe(Rx.IObserver<Unit> observer)
        {
            if (_subject == null)
                _subject = new Subject<Unit>();

            return _subject.Subscribe(observer);
        }

        public void Dispose()
        {
            if (_subject != null)
            {
                _subject.OnNext(Unit.Default);
                _subject.OnCompleted();
                _subject = null;
            }
        }
    }
}