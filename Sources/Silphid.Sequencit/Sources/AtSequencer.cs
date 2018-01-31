using System;
using UniRx;

namespace Silphid.Sequencit
{
    internal class AtSequencer : ISequencer
    {
        private readonly ISequencer _innerSequencer;
        private readonly float _delay;

        public AtSequencer(ISequencer innerSequencer, float delay)
        {
            if (!(innerSequencer is Parallel))
                throw new NotSupportedException("At() extension method can only be used on Parallel sequencers");
            
            _innerSequencer = innerSequencer;
            _delay = delay;
        }

        public IDisposable Subscribe(ICompletableObserver observer) =>
            _innerSequencer.Subscribe();

        public object Add(ICompletable completable) =>
            _innerSequencer.AddSequence(seq =>
            {
                seq.AddDelay(_delay);
                seq.Add(completable);
            });

        public object Add(Func<ICompletable> selector) =>
            _innerSequencer.AddSequence(seq =>
            {
                seq.AddDelay(_delay);
                seq.Add(selector);
            });
    }
}