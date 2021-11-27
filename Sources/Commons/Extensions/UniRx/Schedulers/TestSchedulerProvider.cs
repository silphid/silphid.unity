using UniRx;

namespace Silphid.Extensions.UniRx.Schedulers
{
    public class TestSchedulerProvider : ISchedulerProvider
    {
        private TestScheduler _currentThread;
        private TestScheduler _immediate;
        private TestScheduler _mainThread;
        private TestScheduler _mainThreadEndOfFrame;
        private TestScheduler _mainThreadFixedUpdate;
        private TestScheduler _mainThreadIgnoreTimeScale;
        private TestScheduler _threadPool;

        public TestScheduler CurrentThread => _currentThread ?? (_currentThread = new TestScheduler());
        public TestScheduler Immediate => _immediate ?? (_immediate = new TestScheduler());
        public TestScheduler MainThread => _mainThread ?? (_mainThread = new TestScheduler());

        public TestScheduler MainThreadEndOfFrame =>
            _mainThreadEndOfFrame ?? (_mainThreadEndOfFrame = new TestScheduler());

        public TestScheduler MainThreadFixedUpdate =>
            _mainThreadFixedUpdate ?? (_mainThreadFixedUpdate = new TestScheduler());

        public TestScheduler MainThreadIgnoreTimeScale =>
            _mainThreadIgnoreTimeScale ?? (_mainThreadIgnoreTimeScale = new TestScheduler());

        public TestScheduler ThreadPool => _threadPool ?? (_threadPool = new TestScheduler());

        IScheduler ISchedulerProvider.CurrentThread => CurrentThread;
        IScheduler ISchedulerProvider.Immediate => Immediate;
        IScheduler ISchedulerProvider.MainThread => MainThread;
        IScheduler ISchedulerProvider.MainThreadEndOfFrame => MainThreadEndOfFrame;
        IScheduler ISchedulerProvider.MainThreadFixedUpdate => MainThreadFixedUpdate;
        IScheduler ISchedulerProvider.MainThreadIgnoreTimeScale => MainThreadIgnoreTimeScale;
        IScheduler ISchedulerProvider.ThreadPool => ThreadPool;
    }
}