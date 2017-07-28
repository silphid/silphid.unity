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

        gate.Value = true;
        Assert.That(_value, Is.EqualTo(3));

        gate.Value = false;
        _scheduler.AdvanceTo(10);
        Assert.That(_value, Is.EqualTo(4));

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
