using NUnit.Framework;

namespace Tests;

public class RandomDurationTests
{

    [Test]
    public void RandomShortSleep([Range(0, 10)] int count)
    {
        Thread.Sleep(Random.Shared.Next(1000, 60000));
        Assert.That(count, Is.InRange(0, 8));
    }    
    
    [Test]
    public void RandomSleep([Range(0, 2)] int count)
    {
        Thread.Sleep(Random.Shared.Next(120000, 150000));
        Assert.That(count, Is.InRange(0, 1));
    }
}
