
using Silphid.Tests;

namespace UniRx.Tests
{
    public class TestSchedulerTest
    {
        private int _value;
        private TestScheduler _scheduler;

        public void Setup()
        {
            _value = 0;
            _scheduler = new TestScheduler();

            _scheduler.Schedule(10, () => _value = 1);
            _scheduler.Schedule(20, () => _value = 2);
            _scheduler.Schedule(30, () => _value = 3);
            _scheduler.Schedule(30, () => _value = 4);
            _scheduler.Schedule(50, () => _value = 5);
            _scheduler.Schedule(60, () => _value = 6);
        }

        public void InitialState()
        {
            _value.Is(0);
        }

        public void AdvanceTo_BeforeFirstScheduledAction()
        {
            _scheduler.AdvanceTo(5);
            _value.Is(0);
        }

        public void AdvanceTo_ExactlyAtScheduledAction()
        {
            _scheduler.AdvanceTo(20);
            _value.Is(2);
        }

        public void AdvanceTo_BetweenScheduledActions()
        {
            _scheduler.AdvanceTo(15);
            _value.Is(1);
        }

        public void AdvanceTo_ExactlyAtManyScheduledActionAtSameMoment_RespectScheduledOrder()
        {
            _scheduler.AdvanceTo(30);
            _value.Is(4);
        }

        public void Start_ExecutesEverything()
        {
            _scheduler.Start();
            _value.Is(6);
            _scheduler.Clock.Is(60);
        }

        public void Start_ExecutesEverythingButDoesNotExecuteSubsequentlyScheduledActionsUntilNextStart()
        {
            _scheduler.Start();
            _value.Is(6);

            _scheduler.Schedule(70, () => _value = 7);
            _value.Is(6);

            _scheduler.Start();
            _value.Is(7);
        }

        public void AdvanceTo_UpdatesClock()
        {
            _scheduler.AdvanceTo(35);
            _scheduler.Clock.Is(35);
        }
    }
}