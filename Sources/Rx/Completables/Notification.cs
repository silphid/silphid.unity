using System;
using System.Diagnostics;
using System.Globalization;

namespace UniRx
{
    /// <summary>
    /// Represents an OnError notification to an observer.
    /// </summary>
#if !NO_DEBUGGER_ATTRIBUTES
    [DebuggerDisplay("OnError({Exception})")]
#endif
#if !NO_SERIALIZABLE
    [Serializable]
#endif
    internal sealed class OnErrorNotification<T> : Notification<T>
    {
        Exception exception;

        /// <summary>
        /// Constructs a notification of an exception.
        /// </summary>
        public OnErrorNotification(Exception exception)
        {
            this.exception = exception;
        }

        /// <summary>
        /// Throws the exception.
        /// </summary>
        public override T Value
        {
            get { throw exception; }
        }

        /// <summary>
        /// Returns the exception.
        /// </summary>
        public override Exception Exception
        {
            get { return exception; }
        }

        /// <summary>
        /// Returns false.
        /// </summary>
        public override bool HasValue
        {
            get { return false; }
        }

        /// <summary>
        /// Returns NotificationKind.OnError.
        /// </summary>
        public override NotificationKind Kind
        {
            get { return NotificationKind.OnError; }
        }

        /// <summary>
        /// Returns the hash code for this instance.
        /// </summary>
        public override int GetHashCode()
        {
            return Exception.GetHashCode();
        }

        /// <summary>
        /// Indicates whether this instance and other are equal.
        /// </summary>
        public override bool Equals(Notification<T> other)
        {
            if (Object.ReferenceEquals(this, other))
                return true;
            if (Object.ReferenceEquals(other, null))
                return false;
            if (other.Kind != NotificationKind.OnError)
                return false;
            return Object.Equals(Exception, other.Exception);
        }

        /// <summary>
        /// Returns a string representation of this instance.
        /// </summary>
        public override string ToString()
        {
            return String.Format(
                CultureInfo.CurrentCulture,
                "OnError({0})",
                Exception.GetType()
                         .FullName);
        }

        /// <summary>
        /// Invokes the observer's method corresponding to the notification.
        /// </summary>
        /// <param name="observer">Observer to invoke the notification on.</param>
        public override void Accept(IObserver<T> observer)
        {
            if (observer == null)
                throw new ArgumentNullException("observer");

            observer.OnError(Exception);
        }

        /// <summary>
        /// Invokes the observer's method corresponding to the notification and returns the produced result.
        /// </summary>
        /// <param name="observer">Observer to invoke the notification on.</param>
        /// <returns>Result produced by the observation.</returns>
        public override TResult Accept<TResult>(IObserver<T, TResult> observer)
        {
            if (observer == null)
                throw new ArgumentNullException("observer");

            return observer.OnError(Exception);
        }

        /// <summary>
        /// Invokes the delegate corresponding to the notification.
        /// </summary>
        /// <param name="onNext">Delegate to invoke for an OnNext notification.</param>
        /// <param name="onError">Delegate to invoke for an OnError notification.</param>
        /// <param name="onCompleted">Delegate to invoke for an OnCompleted notification.</param>
        public override void Accept(Action<T> onNext, Action<Exception> onError, Action onCompleted)
        {
            if (onNext == null)
                throw new ArgumentNullException("onNext");
            if (onError == null)
                throw new ArgumentNullException("onError");
            if (onCompleted == null)
                throw new ArgumentNullException("onCompleted");

            onError(Exception);
        }

        /// <summary>
        /// Invokes the delegate corresponding to the notification and returns the produced result.
        /// </summary>
        /// <param name="onNext">Delegate to invoke for an OnNext notification.</param>
        /// <param name="onError">Delegate to invoke for an OnError notification.</param>
        /// <param name="onCompleted">Delegate to invoke for an OnCompleted notification.</param>
        /// <returns>Result produced by the observation.</returns>
        public override TResult Accept<TResult>(Func<T, TResult> onNext,
                                                Func<Exception, TResult> onError,
                                                Func<TResult> onCompleted)
        {
            if (onNext == null)
                throw new ArgumentNullException("onNext");
            if (onError == null)
                throw new ArgumentNullException("onError");
            if (onCompleted == null)
                throw new ArgumentNullException("onCompleted");

            return onError(Exception);
        }
    }

    /// <summary>
    /// Represents an OnCompleted notification to an observer.
    /// </summary>
    [DebuggerDisplay("OnCompleted()")]
    [Serializable]
    internal sealed class OnCompletedNotification<T> : Notification<T>
    {
        /// <summary>
        /// Constructs a notification of the end of a sequence.
        /// </summary>
        public OnCompletedNotification() {}

        /// <summary>
        /// Throws an InvalidOperationException.
        /// </summary>
        public override T Value
        {
            get { throw new InvalidOperationException("No Value"); }
        }

        /// <summary>
        /// Returns null.
        /// </summary>
        public override Exception Exception
        {
            get { return null; }
        }

        /// <summary>
        /// Returns false.
        /// </summary>
        public override bool HasValue
        {
            get { return false; }
        }

        /// <summary>
        /// Returns NotificationKind.OnCompleted.
        /// </summary>
        public override NotificationKind Kind
        {
            get { return NotificationKind.OnCompleted; }
        }

        /// <summary>
        /// Returns the hash code for this instance.
        /// </summary>
        public override int GetHashCode()
        {
            return typeof(T).GetHashCode() ^ 8510;
        }

        /// <summary>
        /// Indicates whether this instance and other are equal.
        /// </summary>
        public override bool Equals(Notification<T> other)
        {
            if (Object.ReferenceEquals(this, other))
                return true;
            if (Object.ReferenceEquals(other, null))
                return false;
            return other.Kind == NotificationKind.OnCompleted;
        }

        /// <summary>
        /// Returns a string representation of this instance.
        /// </summary>
        public override string ToString()
        {
            return "OnCompleted()";
        }

        /// <summary>
        /// Invokes the observer's method corresponding to the notification.
        /// </summary>
        /// <param name="observer">Observer to invoke the notification on.</param>
        public override void Accept(IObserver<T> observer)
        {
            if (observer == null)
                throw new ArgumentNullException("observer");

            observer.OnCompleted();
        }

        /// <summary>
        /// Invokes the observer's method corresponding to the notification and returns the produced result.
        /// </summary>
        /// <param name="observer">Observer to invoke the notification on.</param>
        /// <returns>Result produced by the observation.</returns>
        public override TResult Accept<TResult>(IObserver<T, TResult> observer)
        {
            if (observer == null)
                throw new ArgumentNullException("observer");

            return observer.OnCompleted();
        }

        /// <summary>
        /// Invokes the delegate corresponding to the notification.
        /// </summary>
        /// <param name="onNext">Delegate to invoke for an OnNext notification.</param>
        /// <param name="onError">Delegate to invoke for an OnError notification.</param>
        /// <param name="onCompleted">Delegate to invoke for an OnCompleted notification.</param>
        public override void Accept(Action<T> onNext, Action<Exception> onError, Action onCompleted)
        {
            if (onNext == null)
                throw new ArgumentNullException("onNext");
            if (onError == null)
                throw new ArgumentNullException("onError");
            if (onCompleted == null)
                throw new ArgumentNullException("onCompleted");

            onCompleted();
        }

        /// <summary>
        /// Invokes the delegate corresponding to the notification and returns the produced result.
        /// </summary>
        /// <param name="onNext">Delegate to invoke for an OnNext notification.</param>
        /// <param name="onError">Delegate to invoke for an OnError notification.</param>
        /// <param name="onCompleted">Delegate to invoke for an OnCompleted notification.</param>
        /// <returns>Result produced by the observation.</returns>
        public override TResult Accept<TResult>(Func<T, TResult> onNext,
                                                Func<Exception, TResult> onError,
                                                Func<TResult> onCompleted)
        {
            if (onNext == null)
                throw new ArgumentNullException("onNext");
            if (onError == null)
                throw new ArgumentNullException("onError");
            if (onCompleted == null)
                throw new ArgumentNullException("onCompleted");

            return onCompleted();
        }
    }
}