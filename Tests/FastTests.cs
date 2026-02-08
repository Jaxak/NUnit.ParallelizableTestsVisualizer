using NUnit.Framework;

namespace Tests;

public class FastTests
{
    [Test]
    public void FastTest1()
    {
        Thread.Sleep(1650);
        Assert.Pass();
    }

    [Test]
    public void FastTest2()
    {
        Thread.Sleep(1000);
        Assert.Pass();
    }

    [Test]
    public void FastTest3()
    {
        Thread.Sleep(2000);
        Assert.Pass();
    }

    [Test]
    public void FastTest4()
    {
        Thread.Sleep(1300);
        Assert.Pass();
    }

    [Test]
    public void FastTest5()
    {
        Thread.Sleep(2300);
        Assert.Pass();
    }

    [Test]
    public void FastTest6()
    {
        Thread.Sleep(1500);
        Assert.Pass();
    }

    [Test]
    public void FastTest7()
    {
        Thread.Sleep(1800);
        Assert.Pass();
    }

    [Test]
    public void FastTest8()
    {
        Thread.Sleep(1150);
        Assert.Pass();
    }

    [Test]
    public void FastTest9()
    {
        Thread.Sleep(2150);
        Assert.Pass();
    }

    [Test]
    public void FastTest10()
    {
        Thread.Sleep(1600);
        Assert.Pass();
    }
}
