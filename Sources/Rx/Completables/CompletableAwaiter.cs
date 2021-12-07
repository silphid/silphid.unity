#if (NET_4_6 || NET_STANDARD_2_0)

using System;
using System.Threading;

namespace UniRx
{
    public static partial class Completable
    {
        /// <summary>
        /// Gets an awaiter that returns the last value of the observable sequence or throws an exception if the sequence is empty.
        /// This operation subscribes to the observable sequence, making it hot.
        /// </summary>
        /// <param name="source">Source sequence to await.</param>
        public static AsyncCompletableSubject GetAwaiter(this ICompletable source)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            
            return RunAsync(source, CancellationToken.None);
        }

        /// <summary>
        /// Gets an awaiter that returns the last value of the observable sequence or throws an exception if the sequence is empty.
        /// This operation subscribes to the observable sequence, making it hot.
        /// </summary>
        /// <param name="source">Source sequence to await.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        public static AsyncCompletableSubject GetAwaiter(this ICompletable source, CancellationToken cancellationToken)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));

            return RunAsync(source, cancellationToken);
        }

        public static AsyncCompletableSubject RunAsync(ICompletable source, CancellationToken cancellationToken)
        {
            var s = new AsyncCompletableSubject();
            if (cancellationToken.IsCancellationRequested)
            {
                return Cancel(s, cancellationToken);
            }

            var d = source.Subscribe(s);
            if (cancellationToken.CanBeCanceled)
            {
                RegisterCancelation(s, d, cancellationToken);
            }

            return s;
        }

        public static AsyncCompletableSubject Cancel(AsyncCompletableSubject subject, CancellationToken cancellationToken)
        {
            subject.OnError(new OperationCanceledException(cancellationToken));
            return subject;
        }

        public static void RegisterCancelation(AsyncCompletableSubject subject, IDisposable subscription, CancellationToken token)
        {
            //
            // Separate method used to avoid heap allocation of closure when no cancellation is needed,
            // e.g. when CancellationToken.None is provided to the RunAsync overloads.
            //

            var ctr = token.Register(() =>
            {
                subscription.Dispose();
                Cancel(subject, token);
            });

            //
            // No null-check for ctr is needed:
            //
            // - CancellationTokenRegistration is a struct
            // - Registration will succeed 99% of the time, no warranting an attempt to avoid spurious Subscribe calls
            //
            subject.Subscribe(_ => ctr.Dispose(), ctr.Dispose);
        }
    }
}

#endif