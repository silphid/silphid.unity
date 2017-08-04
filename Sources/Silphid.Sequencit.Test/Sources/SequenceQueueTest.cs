using NUnit.Framework;
using Silphid.Sequencit;
using UniRx;

[TestFixture]
public class SequenceQueueTest : SequencingTestBase
{
    private SequenceQueue _sequenceQueue;

    [SetUp]
    public override void Setup()
    {
        base.Setup();
        _sequenceQueue = new SequenceQueue();
    }

    [Test]
    public void SequencerIsNotStartedInitially()
    {
        Assert.That(_value, Is.EqualTo(0));

        _sequenceQueue.AddAction(() => _value = 123);
        Assert.That(_value, Is.EqualTo(0));
    }

    [Test]
    public void Start_ExecutesAllInstantActionsInstantly()
    {
        _sequenceQueue.AddAction(() => _value = 1);
        _sequenceQueue.AddAction(() => _value = 2);
        _sequenceQueue.AddAction(() => _value = 3);

        Assert.That(_value, Is.EqualTo(0));

        _sequenceQueue.Start();
        Assert.That(_value, Is.EqualTo(3));
    }

    [Test]
    public void Static_Create_DoesNotStartSequencer()
    {
        SequenceQueue.Create(s => s.AddAction(() => _value = 123));
        Assert.That(_value, Is.EqualTo(0));
    }

    [Test]
    public void Static_Start_DoesStartSequencer()
    {
        SequenceQueue.Start(s => s.AddAction(() => _value = 123));
        Assert.That(_value, Is.EqualTo(123));
    }

    [Test]
    public void Stop_DisposesCurrentlyExecutingObservable()
    {
        var seq = SequenceQueue.Start(s =>
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
        var seq = SequenceQueue.Start(s =>
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
        _sequenceQueue.Add(CreateDelay(10).DoOnCompleted(() => _value = 1));
        _sequenceQueue.Start();
        _scheduler.AdvanceTo(10);
        Assert.That(_value, Is.EqualTo(1));

        _sequenceQueue.Add(() =>
        {
            _value = 2;
            return CreateDelay(10).DoOnCompleted(() => _value = 3);
        });
        Assert.That(_value, Is.EqualTo(2));
    }

    [Test]
    public void Add_AddedActionIsNotStartedWhenSequencerStopped()
    {
        _sequenceQueue.Start();
        _sequenceQueue.Add(CreateDelay(10).DoOnCompleted(() => _value = 1));
        _scheduler.AdvanceTo(10);
        Assert.That(_value, Is.EqualTo(1));

        _sequenceQueue.Stop();
        _sequenceQueue.AddAction(() => _value = 2);
        Assert.That(_value, Is.EqualTo(1));
    }
}
