using NUnit.Framework;

namespace Tests;

public class RandomDurationTests
{

    [Test]
    public void RandomShortSleep([Range(0, 10)] int count)
    {
        Thread.Sleep(Random.Shared.Next(1000, 60000));
        Assert.That(count, Is.InRange(0, 100));
    }    
    
    [Test]
    public void RandomSleep([Range(0, 3)] int count)
    {
        Thread.Sleep(Random.Shared.Next(120000, 150000));
        Assert.That(count, Is.InRange(0, 100));
    }
}
