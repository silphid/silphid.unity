using System;
using NUnit.Framework;
using Silphid.Extensions.UniRx.Schedulers;
using UniRx;

public abstract class SequencingTestBase
{
    protected int _value;
    protected TestScheduler _scheduler;
    protected IObservable<Unit> CreateDelay(int ticks) =>
        Observable.ReturnUnit().Delay(TimeSpan.FromTicks(ticks), _scheduler);

    [SetUp]
    public virtual void Setup()
    {
        _value = 0;
        _scheduler = new TestScheduler();
    }

    [Test]
    public void CreateDelay_EnsureTestHelperBehavesAsExpected()
    {
        CreateDelay(10).DoOnCompleted(() => _value = 123).ObserveOn(_scheduler).Subscribe();

        Assert.That(_value, Is.EqualTo(0));

        _scheduler.AdvanceTo(5);
        Assert.That(_value, Is.EqualTo(0));

        _scheduler.AdvanceTo(9);
        Assert.That(_value, Is.EqualTo(0));

        _scheduler.AdvanceBy(10);
        Assert.That(_value, Is.EqualTo(123));
    }
}
