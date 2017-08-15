using NUnit.Framework;
using Silphid.Sequencit;
using UniRx;

[TestFixture]
public class LiveSequenceTest : SequencingTestBase
{
    private LiveSequence _liveSequence;

    [SetUp]
    public override void Setup()
    {
        base.Setup();
        _liveSequence = new LiveSequence();
    }

    [Test]
    public void SequencerIsNotStartedInitially()
    {
        Assert.That(_value, Is.EqualTo(0));

        _liveSequence.AddAction(() => _value = 123);
        Assert.That(_value, Is.EqualTo(0));
    }

    [Test]
    public void Start_ExecutesAllInstantActionsInstantly()
    {
        _liveSequence.AddAction(() => _value = 1);
        _liveSequence.AddAction(() => _value = 2);
        _liveSequence.AddAction(() => _value = 3);

        Assert.That(_value, Is.EqualTo(0));

        _liveSequence.Start();
        Assert.That(_value, Is.EqualTo(3));
    }

    [Test]
    public void Static_Create_DoesNotStartSequencer()
    {
        LiveSequence.Create(s => s.AddAction(() => _value = 123));
        Assert.That(_value, Is.EqualTo(0));
    }

    [Test]
    public void Static_Start_DoesStartSequencer()
    {
        LiveSequence.Start(s => s.AddAction(() => _value = 123));
        Assert.That(_value, Is.EqualTo(123));
    }

    [Test]
    public void Stop_DisposesCurrentlyExecutingObservable()
    {
        var seq = LiveSequence.Start(s =>
        {
            s.AddAction(() => _value = 1);
            s.Add(CreateDelay(10).DoOnCompleted(() => _value = 2));
            s.Add(() =>
            {
                _value = 3;
                return CreateDelay(10)
                    .DoOnCompleted(() => _value = 4)
                    .DoOnCancel(() => _value = 100);
            });
            s.Add(CreateDelay(10).DoOnCompleted(() => _value = 5));
            s.AddAction(() => _value = 6);
        });

        Assert.That(_value, Is.EqualTo(1));

        _scheduler.AdvanceTo(10);
        Assert.That(_value, Is.EqualTo(3));

        seq.Stop();
        Assert.That(_value, Is.EqualTo(100));

        _scheduler.AdvanceTo(1000);
        Assert.That(_value, Is.EqualTo(100));
    }

    [Test]
    public void Start_AfterStop_ExecutionResumesInstantlyWithNextItem()
    {
        var seq = LiveSequence.Start(s =>
        {
            s.Add(CreateDelay(10).DoOnCompleted(() => _value = 1));
            s.Add(() =>
            {
                _value = 2;
                return CreateDelay(10).DoOnCancel(() => _value = 3);
            });
            s.Add(() =>
            {
                _value = 4;
                return CreateDelay(10);
            });
            s.AddAction(() => _value = 5);
        });

        Assert.That(_value, Is.EqualTo(0));

        _scheduler.AdvanceTo(10);
        Assert.That(_value, Is.EqualTo(2));

        seq.Stop();
        Assert.That(_value, Is.EqualTo(3));

        _scheduler.AdvanceTo(1000);
        Assert.That(_value, Is.EqualTo(3));

        seq.Start();
        Assert.That(_value, Is.EqualTo(4));

        _scheduler.AdvanceBy(10);
        Assert.That(_value, Is.EqualTo(5));
    }

    [Test]
    public void Add_AddedActionIsStartedImmediatelyWhenQueueIsEmpty()
    {
        _liveSequence.Add(CreateDelay(10).DoOnCompleted(() => _value = 1));
        _liveSequence.Start();
        _scheduler.AdvanceTo(10);
        Assert.That(_value, Is.EqualTo(1));

        _liveSequence.Add(() =>
        {
            _value = 2;
            return CreateDelay(10).DoOnCompleted(() => _value = 3);
        });
        Assert.That(_value, Is.EqualTo(2));
    }

    [Test]
    public void Add_AddedActionIsNotStartedWhenSequencerStopped()
    {
        _liveSequence.Start();
        _liveSequence.Add(CreateDelay(10).DoOnCompleted(() => _value = 1));
        _scheduler.AdvanceTo(10);
        Assert.That(_value, Is.EqualTo(1));

        _liveSequence.Stop();
        _liveSequence.AddAction(() => _value = 2);
        Assert.That(_value, Is.EqualTo(1));
    }

    [Test]
    public void SkipBefore()
    {
        var obs = CreateDelay(10);
        
        _liveSequence.Start();
        _liveSequence.Add(Observable.Never<Unit>());
        _liveSequence.AddAction(() => _value = 1);
        _liveSequence.Add(obs);
        _liveSequence.AddAction(() => _value = 2);

        Assert.That(_value, Is.EqualTo(0));
        
        _liveSequence.SkipBefore(obs);
        Assert.That(_value, Is.EqualTo(0));
        
        _scheduler.AdvanceBy(9);
        Assert.That(_value, Is.EqualTo(1));
        
        _scheduler.AdvanceBy(1);
        Assert.That(_value, Is.EqualTo(2));
    }

    [Test]
    public void SkipAfter()
    {
        var obs = CreateDelay(10);
        
        _liveSequence.Start();
        _liveSequence.Add(Observable.Never<Unit>());
        _liveSequence.AddAction(() => _value = 1);
        _liveSequence.Add(obs);
        _liveSequence.AddAction(() => _value = 2);

        Assert.That(_value, Is.EqualTo(0));
        
        _liveSequence.SkipAfter(obs);
        Assert.That(_value, Is.EqualTo(2));
    }

    [Test]
    public void TruncateBefore()
    {
        var obs = Sequence.Create(seq =>
        {
            seq.AddAction(() => _value = 100);
            _liveSequence.Add(CreateDelay(10));
            seq.AddAction(() => _value = 200);
        });
        
        _liveSequence.Start();
        _liveSequence.Add(CreateDelay(10));
        _liveSequence.AddAction(() => _value = 1);
        _liveSequence.Add(obs);
        _liveSequence.AddAction(() => _value = 2);

        Assert.That(_value, Is.EqualTo(0));
        
        _liveSequence.SkipBefore(obs);
        Assert.That(_value, Is.EqualTo(0));
        
        _scheduler.AdvanceBy(9);
        Assert.That(_value, Is.EqualTo(1));
        
        _scheduler.AdvanceBy(1);
        Assert.That(_value, Is.EqualTo(2));
    }

    [Test]
    public void AddMarker()
    {
        _liveSequence.Start();
        _liveSequence.Add(Observable.Never<Unit>());
        _liveSequence.AddAction(() => _value = 1);
        var marker = _liveSequence.AddMarker();
        _liveSequence.AddAction(() => _value = 2);

        Assert.That(_value, Is.EqualTo(0));
        _liveSequence.SkipBefore(marker);
        Assert.That(_value, Is.EqualTo(2));
    }

    [Test]
    public void Clear()
    {
        _liveSequence.Start();
        _liveSequence.Add(Observable.Never<Unit>());
        _liveSequence.AddAction(() => _value = 1);

        Assert.That(_value, Is.EqualTo(0));
        
        _liveSequence.Clear();
        _liveSequence.AddAction(() => _value = 2);
        Assert.That(_value, Is.EqualTo(2));
    }
}
