using System;
using System.Collections.Generic;

namespace UniRx.Completables.Operators
{
    internal class ObserveOnCompletable : OperatorCompletableBase
    {
        private readonly ICompletable source;
        private readonly IScheduler scheduler;

        public ObserveOnCompletable(ICompletable source, IScheduler scheduler)
            : base(source.IsRequiredSubscribeOnCurrentThread())
        {
            this.source = source;
            this.scheduler = scheduler;
        }

        protected override IDisposable SubscribeCore(ICompletableObserver observer, IDisposable cancel)
        {
            var queueing = scheduler as ISchedulerQueueing;
            if (queueing == null)
                return new ObserveOnObserver(this, observer, cancel).Run();

            return new ScheduledObserveOnObserver(this, queueing, observer, cancel).Run();
        }

        private class ObserveOnObserver : OperatorCompletableObserverBase
        {
            private class SchedulableAction : IDisposable
            {
                public Notification<Unit> data;
                public LinkedListNode<SchedulableAction> node;
                public IDisposable schedule;

                public void Dispose()
                {
                    schedule?.Dispose();
                    schedule = null;
                    node.List?.Remove(node);
                }

                public bool IsScheduled
                {
                    get { return schedule != null; }
                }
            }

            private readonly ObserveOnCompletable parent;
            private readonly LinkedList<SchedulableAction> actions = new LinkedList<SchedulableAction>();
            private bool isDisposed;

            public ObserveOnObserver(ObserveOnCompletable parent, ICompletableObserver observer, IDisposable cancel)
                : base(observer, cancel)
            {
                this.parent = parent;
            }

            public IDisposable Run()
            {
                isDisposed = false;

                var sourceDisposable = parent.source.Subscribe(this);

                return StableCompositeDisposable.Create(
                    sourceDisposable,
                    Disposable.Create(
                        () =>
                        {
                            lock (actions)
                            {
                                isDisposed = true;

                                while (actions.Count > 0)
                                {
                                    // Dispose will both cancel the action (if not already running)
                                    // and remove it from 'actions'
                                    actions.First.Value.Dispose();
                                }
                            }
                        }));
            }

            public override void OnError(Exception error)
            {
                QueueAction(new OnErrorNotification<Unit>(error));
            }

            public override void OnCompleted()
            {
                QueueAction(new OnCompletedNotification<Unit>());
            }

            private void QueueAction(Notification<Unit> data)
            {
                var action = new SchedulableAction { data = data };
                lock (actions)
                {
                    if (isDisposed)
                        return;

                    action.node = actions.AddLast(action);
                    ProcessNext();
                }
            }

            private void ProcessNext()
            {
                lock (actions)
                {
                    if (actions.Count == 0 || isDisposed)
                        return;

                    var action = actions.First.Value;

                    if (action.IsScheduled)
                        return;

                    action.schedule = parent.scheduler.Schedule(
                        () =>
                        {
                            try
                            {
                                switch (action.data.Kind)
                                {
                                    case NotificationKind.OnError:
                                        observer.OnError(action.data.Exception);
                                        break;
                                    case NotificationKind.OnCompleted:
                                        observer.OnCompleted();
                                        break;
                                }
                            }
                            finally
                            {
                                lock (actions)
                                {
                                    action.Dispose();
                                }

                                if (action.data.Kind == NotificationKind.OnNext)
                                    ProcessNext();
                                else
                                    Dispose();
                            }
                        });
                }
            }
        }

        private class ScheduledObserveOnObserver : OperatorCompletableObserverBase
        {
            private readonly ObserveOnCompletable parent;
            private readonly ISchedulerQueueing scheduler;
            private readonly BooleanDisposable isDisposed;

            public ScheduledObserveOnObserver(ObserveOnCompletable parent,
                                              ISchedulerQueueing scheduler,
                                              ICompletableObserver observer,
                                              IDisposable cancel)
                : base(observer, cancel)
            {
                this.parent = parent;
                this.scheduler = scheduler;
                isDisposed = new BooleanDisposable();
            }

            public IDisposable Run()
            {
                var sourceDisposable = parent.source.Subscribe(this);
                return StableCompositeDisposable.Create(sourceDisposable, isDisposed);
            }

            public override void OnError(Exception error)
            {
                scheduler.ScheduleQueueing(
                    isDisposed,
                    error,
                    ex =>
                    {
                        try
                        {
                            observer.OnError(ex);
                        }
                        finally
                        {
                            Dispose();
                        }
                    });
            }

            public override void OnCompleted()
            {
                scheduler.ScheduleQueueing(
                    isDisposed,
                    Unit.Default,
                    _ =>
                    {
                        try
                        {
                            observer.OnCompleted();
                        }
                        finally
                        {
                            Dispose();
                        }
                    });
            }
        }
    }
}