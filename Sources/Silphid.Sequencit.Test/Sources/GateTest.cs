using NUnit.Framework;
using Silphid.Sequencit;
using UniRx;
// ReSharper disable AccessToDisposedClosure

[TestFixture]
public class GateTest : SequenceableTestBase
{
    [Test]
    public void BooleanGateTest()
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
            s.Add(CreateDelay(10));
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

    [Test]
    public void ReturnedDisposableGateTest()
    {
        var disposable = new SerialDisposable();
        
        Sequence.Start(s =>
        {
            s.AddAction(() => _value = 1);
            disposable.Disposable = s.AddDisposableGate();
            s.AddAction(() => _value = 2);
        });

        Assert.That(_value, Is.EqualTo(1));

        disposable.Dispose();
        Assert.That(_value, Is.EqualTo(2));
    }

    [Test]
    public void LambdaDisposableGateTest()
    {
        var disposable = new SerialDisposable();
        
        Sequence.Start(s =>
        {
            s.AddAction(() => _value = 1);
            s.AddDisposableGate(x => disposable.Disposable = x);
            s.AddAction(() => _value = 2);
        });

        Assert.That(_value, Is.EqualTo(1));

        disposable.Dispose();
        Assert.That(_value, Is.EqualTo(2));
    }
}
