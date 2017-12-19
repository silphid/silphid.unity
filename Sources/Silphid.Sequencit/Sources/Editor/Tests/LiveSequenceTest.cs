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
        AssertValue(0);

        _liveSequence.AddAction(() => _value = 123);
        AssertValue(0);
    }

    [Test]
    public void Start_ExecutesAllInstantActionsInstantly()
    {
        _liveSequence.AddAction(() => _value = 1);
        _liveSequence.AddAction(() => _value = 2);
        _liveSequence.AddAction(() => _value = 3);

        AssertValue(0);

        _liveSequence.Subscribe();
        AssertValue(3);
    }

    [Test]
    public void Static_Create_DoesNotStartSequencer()
    {
        LiveSequence.Create(s => s.AddAction(() => _value = 123));
        AssertValue(0);
    }

    [Test]
    public void Static_Start_DoesStartSequencer()
    {
        LiveSequence.Start(s => s.AddAction(() => _value = 123));
        AssertValue(123);
    }

    [Test]
    public void Stop_DisposesCurrentlyExecutingObservable()
    {
        var disposable = LiveSequence.Start(s =>
        {
            s.AddAction(() => _value = 1);
            s.Add(CreateTimer(10).DoOnCompleted(() => _value = 2));
            s.Add(() =>
            {
                _value = 3;
                return CreateTimer(10)
                    .DoOnCompleted(() => _value = 4)
                    .DoOnCancel(() => _value = 100);
            });
            s.Add(CreateTimer(10).DoOnCompleted(() => _value = 5));
            s.AddAction(() => _value = 6);
        });

        AssertValue(1);

        _scheduler.AdvanceTo(10);
        AssertValue(3);

        disposable.Dispose();
        AssertValue(100);

        _scheduler.AdvanceTo(1000);
        AssertValue(100);
    }

    [Test]
    public void SubscribeAfterDisposeOfPreviousSubscription_ExecutionResumesInstantlyWithNextItem()
    {
        var seq = LiveSequence.Create(s =>
        {
            s.Add(CreateTimer(10).DoOnCompleted(() => _value = 1));
            s.Add(() =>
            {
                _value = 2;
                return CreateTimer(10).DoOnCancel(() => _value = 3);
            });
            s.Add(() =>
            {
                _value = 4;
                return CreateTimer(10);
            });
            s.AddAction(() => _value = 5);
        });
        var disposable = seq.Subscribe();

        AssertValue(0);

        _scheduler.AdvanceTo(10);
        AssertValue(2);

        disposable.Dispose();
        AssertValue(3);

        _scheduler.AdvanceTo(1000);
        AssertValue(3);

        seq.Subscribe();
        AssertValue(4);

        _scheduler.AdvanceBy(10);
        AssertValue(5);
    }

    [Test]
    public void Add_AddedActionIsStartedImmediatelyWhenQueueIsEmpty()
    {
        _liveSequence.Add(CreateTimer(10).DoOnCompleted(() => _value = 1));
        _liveSequence.Subscribe();
        _scheduler.AdvanceTo(10);
        AssertValue(1);

        _liveSequence.Add(() =>
        {
            _value = 2;
            return CreateTimer(10).DoOnCompleted(() => _value = 3);
        });
        AssertValue(2);
    }

    [Test]
    public void Add_AddedActionIsNotExecutedAfterSubscriptionDisposed()
    {
        var disposable = _liveSequence.Subscribe();
        _liveSequence.Add(CreateTimer(10).DoOnCompleted(() => _value = 1));
        _scheduler.AdvanceTo(10);
        AssertValue(1);

        disposable.Dispose();
        _liveSequence.AddAction(ShouldNotReachThisPoint);
    }

    [Test]
    public void SkipBefore()
    {
        var obs = CreateTimer(10);
        
        _liveSequence.Subscribe();
        _liveSequence.AddAction(() => _value = 1);
        _liveSequence.Add(Observable.Never<Unit>());
        _liveSequence.AddAction(ShouldNotReachThisPoint);
        _liveSequence.Add(obs);
        _liveSequence.AddAction(() => _value = 2);

        AssertValue(1);
        
        _liveSequence.SkipBefore(obs);
        AssertValue(1);
        
        _scheduler.AdvanceBy(9);
        AssertValue(1);
        
        _scheduler.AdvanceBy(1);
        AssertValue(2);
    }

    [Test]
    public void SkipAfter()
    {
        var obs = CreateTimer(10);
        
        _liveSequence.Subscribe();
        _liveSequence.Add(Observable.Never<Unit>());
        _liveSequence.AddAction(() => _value = 1);
        _liveSequence.Add(obs);
        _liveSequence.AddAction(() => _value = 2);

        AssertValue(0);
        
        _liveSequence.SkipAfter(obs);
        AssertValue(2);
    }

    [Test]
    public void TruncateBefore()
    {
        var checkpoint1 = false;
        var checkpoint2 = false;
        
        _liveSequence.Subscribe();
        var lapse1 = _liveSequence.AddLapse();
        _liveSequence.AddAction(() => checkpoint1 = true);
        var obs = _liveSequence.AddAction(() => checkpoint2 = true);
        _liveSequence.AddAction(ShouldNotReachThisPoint);

        Assert.That(checkpoint1, Is.False);
        Assert.That(checkpoint2, Is.False);
       
        _liveSequence.TruncateBefore(obs);
        
        lapse1.Dispose();
        Assert.That(checkpoint1, Is.True);
        Assert.That(checkpoint2, Is.False);
    }
    
    [Test]
    public void TruncateAfter()
    {
        var checkpoint1 = false;
        var checkpoint2 = false;
        
        _liveSequence.Subscribe();
        var lapse1 = _liveSequence.AddLapse();
        _liveSequence.AddAction(() => checkpoint1 = true);
        var obs = _liveSequence.AddAction(() => checkpoint2 = true);
        _liveSequence.AddAction(ShouldNotReachThisPoint);

        Assert.That(checkpoint1, Is.False);
        Assert.That(checkpoint2, Is.False);
       
        _liveSequence.TruncateAfter(obs);
        
        lapse1.Dispose();
        Assert.That(checkpoint1, Is.True);
        Assert.That(checkpoint2, Is.True);
    }

    [Test]
    public void AddMarker()
    {
        _liveSequence.Subscribe();
        _liveSequence.Add(Observable.Never<Unit>());
        _liveSequence.AddAction(() => _value = 1);
        var instant = _liveSequence.AddInstant();
        _liveSequence.AddAction(() => _value = 2);

        AssertValue(0);
        _liveSequence.SkipBefore(instant);
        AssertValue(2);
    }

    [Test]
    public void AddMarkerOrTruncateAfter()
    {
        var checkpoint1 = false;
        
        var instant = new Instant();
        
        _liveSequence.AddAction(() => _value = 1);
        _liveSequence.Add(CreateTimer(10));
        _liveSequence.AddAction(() => checkpoint1 = true);
        _liveSequence.Add(instant);
        _liveSequence.AddAction(ShouldNotReachThisPoint);

        _liveSequence.Subscribe();
        _scheduler.AdvanceBy(5);
        AssertValue(1);

        // Marker already in sequence: will truncate after it
        _liveSequence.AddOrTruncateAfter(instant);
        _liveSequence.AddAction(() => _value = 2);
        _liveSequence.Add(CreateTimer(100));
        _liveSequence.AddAction(() => _value = 3);

        Assert.That(checkpoint1, Is.False);
        _scheduler.AdvanceBy(5);
        Assert.That(checkpoint1, Is.True);
        AssertValue(2);

        // Marker no longer in sequence: will add marker
        _liveSequence.AddOrTruncateAfter(instant);
        _liveSequence.Add(CreateTimer(200));
        _liveSequence.AddAction(() => _value = 4);
        AssertValue(2);

        _scheduler.AdvanceBy(100);
        AssertValue(3);

        _scheduler.AdvanceBy(200);
        AssertValue(4);
    }

    [Test]
    public void DoesNotCompleteUntilItReachesAnExplicitAddComplete()
    {
        _liveSequence.AddAction(() => _value = 1);

        var isCompleted = false;
        _liveSequence.Subscribe(() => isCompleted = true);
        AssertValue(1);
        Assert.That(isCompleted, Is.False);

        _liveSequence.Add(CreateTimer(10));
        _liveSequence.AddAction(() => _value = 2);
        _liveSequence.AddComplete();
        AssertValue(1);
        Assert.That(isCompleted, Is.False);
        
        _scheduler.AdvanceBy(10);
        AssertValue(2);
        Assert.That(isCompleted, Is.True);
    }

    [Test]
    public void Clear()
    {
        _liveSequence.Subscribe();
        _liveSequence.Add(Observable.Never<Unit>());
        _liveSequence.AddAction(() => _value = 1);

        AssertValue(0);
        
        _liveSequence.Clear();
        _liveSequence.AddAction(() => _value = 2);
        AssertValue(2);
    }
}
