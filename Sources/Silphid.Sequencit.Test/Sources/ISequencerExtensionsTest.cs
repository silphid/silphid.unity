using System;
using NUnit.Framework;
using Silphid.Sequencit;
using UniRx;

[TestFixture]
public class ISequencerExtensionsTest : SequencingTestBase
{
    [Test]
    public void Add_FuncObservableUnit()
    {
        Sequence.Start(s =>
        {
            s.AddAction(() => _value = 123);
            s.Add(CreateDelay(100));
            s.AddAction(() => _value = 456);
        });

        Assert.That(_value, Is.EqualTo(123));

        _scheduler.AdvanceTo(100);
        Assert.That(_value, Is.EqualTo(456));
    }

    [Test]
    public void Add_FuncObservableUnit_FuncEvaluationIsDeferred()
    {
        Sequence.Start(s =>
        {
            s.Add(CreateDelay(100));
            s.Add(() =>
            {
                _value = 111;
                return CreateDelay(100);
            });
        });

        Assert.That(_value, Is.EqualTo(0));

        _scheduler.AdvanceTo(100);
        Assert.That(_value, Is.EqualTo(111));
    }

    [Test]
    public void Add_ObservableT_WaitsForEntireCompletion()
    {
        Sequence.Start(s =>
        {
            s.AddAction(() => _value = 123);
            s.Add(Observable.Interval(TimeSpan.FromTicks(100), _scheduler).Take(2));
            s.AddAction(() => _value = 456);
        });

        Assert.That(_value, Is.EqualTo(123));

        _scheduler.AdvanceTo(100);
        Assert.That(_value, Is.EqualTo(123));

        _scheduler.AdvanceTo(200);
        Assert.That(_value, Is.EqualTo(456));
    }

    [Test]
    public void Add_FuncObservableT_WaitsForEntireCompletion()
    {
        Sequence.Start(s =>
        {
            s.AddAction(() => _value = 123);
            s.Add(() => Observable.Interval(TimeSpan.FromTicks(100), _scheduler).Take(2));
            s.AddAction(() => _value = 456);
        });

        Assert.That(_value, Is.EqualTo(123));

        _scheduler.AdvanceTo(100);
        Assert.That(_value, Is.EqualTo(123));

        _scheduler.AdvanceTo(200);
        Assert.That(_value, Is.EqualTo(456));
    }

    [Test]
    public void Add_FuncObservableT_FuncEvaluationIsDeferred()
    {
        Sequence.Start(s =>
        {
            s.Add(CreateDelay(100));
            s.Add(() =>
            {
                _value = 111;
                return Observable.Interval(TimeSpan.FromTicks(100), _scheduler).Take(2);
            });
        });

        Assert.That(_value, Is.EqualTo(0));

        _scheduler.AdvanceTo(100);
        Assert.That(_value, Is.EqualTo(111));
    }

    [Test]
    public void AddAction_IsInstant()
    {
        Sequence.Start(s =>
        {
            s.AddAction(() => _value = 123);
            s.AddAction(() => _value = 456);
        });

        Assert.That(_value, Is.EqualTo(456));
    }

    [Test]
    public void AddInterval_Seconds()
    {
        Sequence.Start(s =>
        {
            s.AddAction(() => _value = 123);
            s.AddDelay(5f, _scheduler);
            s.AddAction(() => _value = 456);
        });

        Assert.That(_value, Is.EqualTo(123));

        _scheduler.AdvanceTo(TimeSpan.FromSeconds(5f).Ticks);
        Assert.That(_value, Is.EqualTo(456));
    }

    [Test]
    public void AddInterval_TimeSpan()
    {
        Sequence.Start(s =>
        {
            s.AddAction(() => _value = 123);
            s.AddDelay(TimeSpan.FromTicks(100), _scheduler);
            s.AddAction(() => _value = 456);
        });

        Assert.That(_value, Is.EqualTo(123));

        _scheduler.AdvanceTo(100);
        Assert.That(_value, Is.EqualTo(456));
    }
}
