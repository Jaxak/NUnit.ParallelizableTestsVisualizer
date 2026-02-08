using NUnit.Framework;

namespace Tests;

public class ParallelTests
{
    [Test]
    public void QuickTest1()
    {
        Thread.Sleep(1100);
        Assert.Pass();
    }

    [Test]
    public void QuickTest2()
    {
        Thread.Sleep(1650);
        Assert.Pass();
    }

    [Test]
    public void QuickTest3()
    {
        Thread.Sleep(2200);
        Assert.Pass();
    }

    [Test]
    public void QuickTest4()
    {
        Thread.Sleep(1300);
        Assert.Pass();
    }

    [Test]
    public void QuickTest5()
    {
        Thread.Sleep(2000);
        Assert.Pass();
    }

    [Test]
    public void QuickTest6()
    {
        Thread.Sleep(1000);
        Assert.Pass();
    }

    [Test]
    public void QuickTest7()
    {
        Thread.Sleep(1750);
        Assert.Pass();
    }

    [Test]
    public void QuickTest8()
    {
        Thread.Sleep(1200);
        Assert.Pass();
    }
}
