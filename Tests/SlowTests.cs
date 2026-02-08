using NUnit.Framework;

namespace Tests;

public class SlowTests
{
    [Test]
    public void SlowTest1()
    {
        Thread.Sleep(500);
        Assert.Pass();
    }

    [Test]
    public void SlowTest2()
    {
        Thread.Sleep(600);
        Assert.Pass();
    }

    [Test]
    public void SlowTest3()
    {
        Thread.Sleep(450);
        Assert.Pass();
    }

    [Test]
    public void SlowTest4()
    {
        Thread.Sleep(550);
        Assert.Pass();
    }

    [Test]
    public void FailingSlowTest()
    {
        Thread.Sleep(400);
        Assert.Fail("Этот тест специально падает для демонстрации");
    }

    [Test]
    public void SlowTest6()
    {
        Thread.Sleep(650);
        Assert.Pass();
    }
}
