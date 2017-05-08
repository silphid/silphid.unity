using NUnit.Framework;
using Silphid.Extensions.UniRx.Schedulers;

[TestFixture]
public class TestSchedulerTest
{
    private int _value;
    private TestScheduler _scheduler;

    [SetUp]
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

    [Test]
	public void InitialState()
	{
	    Assert.That(_value, Is.EqualTo(0));
	}

    [Test]
    public void AdvanceTo_BeforeFirstScheduledAction()
    {
        _scheduler.AdvanceTo(5);
        Assert.That(_value, Is.EqualTo(0));
    }

    [Test]
    public void AdvanceTo_ExactlyAtScheduledAction()
    {
        _scheduler.AdvanceTo(20);
        Assert.That(_value, Is.EqualTo(2));
    }

    [Test]
	public void AdvanceTo_BetweenScheduledActions()
	{
	    _scheduler.AdvanceTo(15);
	    Assert.That(_value, Is.EqualTo(1));
	}

    [Test]
	public void AdvanceTo_ExactlyAtManyScheduledActionAtSameMoment_RespectScheduledOrder()
	{
	    _scheduler.AdvanceTo(30);
	    Assert.That(_value, Is.EqualTo(4));
	}

    [Test]
	public void Start_ExecutesEverything()
    {
        _scheduler.Start();
	    Assert.That(_value, Is.EqualTo(6));
	    Assert.That(_scheduler.Clock, Is.EqualTo(60));
	}

    [Test]
    public void Start_ExecutesEverythingButDoesNotExecuteSubsequentlyScheduledActionsUntilNextStart()
    {
        _scheduler.Start();
        Assert.That(_value, Is.EqualTo(6));

        _scheduler.Schedule(70, () => _value = 7);
        Assert.That(_value, Is.EqualTo(6));

        _scheduler.Start();
        Assert.That(_value, Is.EqualTo(7));
    }

    [Test]
    public void AdvanceTo_UpdatesClock()
    {
        _scheduler.AdvanceTo(35);
        Assert.That(_scheduler.Clock, Is.EqualTo(35));
    }
}
