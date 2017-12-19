using NUnit.Framework;
using UniRx;

namespace Silphid.Sequencit.Test
{
    [TestFixture]
    public class GateTest : SequencingTestBase
    {
        [Test]
        public void AddGate()
        {
            // Start in already paused state
            var gate = new ReactiveProperty<bool>(false);
        
            Sequence.Start(s =>
            {
                s.AddAction(() => _value = 1);
                s.AddGate(gate);
                s.AddAction(() => _value = 2);
                s.AddGate(gate);
                s.AddAction(() => _value = 3);
                s.Add(CreateTimer(10));
                s.AddAction(() => _value = 4);
                s.AddGate(gate);
                s.AddAction(() => _value = 5);
            });

            Assert.That(_value, Is.EqualTo(1));

            // Resume
            gate.Value = true;
            Assert.That(_value, Is.EqualTo(3));

            // Pause (during virtual delay)
            gate.Value = false;
        
            // Let virtual delay elapse
            _scheduler.AdvanceTo(10);
            Assert.That(_value, Is.EqualTo(4));

            // Resume
            gate.Value = true;
            Assert.That(_value, Is.EqualTo(5));
        }
    }
}