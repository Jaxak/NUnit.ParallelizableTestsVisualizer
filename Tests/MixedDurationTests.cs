using NUnit.Framework;

namespace Tests;

public class MixedDurationTests
{
    [Test]
    public void VeryShortTest()
    {
        Thread.Sleep(20);
        Assert.Pass();
    }

    [Test]
    public void MediumTest1()
    {
        Thread.Sleep(300);
        Assert.Pass();
    }

    [Test]
    public void ShortTest()
    {
        Thread.Sleep(80);
        Assert.Pass();
    }

    [Test]
    public void LongTest()
    {
        Thread.Sleep(700);
        Assert.Pass();
    }

    [Test]
    public void MediumTest2()
    {
        Thread.Sleep(350);
        Assert.Pass();
    }

    [Test]
    [Ignore("Игнорируемый тест для демонстрации")]
    public void SkippedTest()
    {
        Thread.Sleep(200);
        Assert.Pass();
    }

    [Test]
    public void AnotherShortTest()
    {
        Thread.Sleep(95);
        Assert.Pass();
    }

    [Test]
    public void AnotherMediumTest()
    {
        Thread.Sleep(280);
        Assert.Pass();
    }

    [Test]
    public void VeryLongTest()
    {
        Thread.Sleep(800);
        Assert.Pass();
    }

    [Test]
    public void QuickFailingTest()
    {
        Thread.Sleep(100);
        Assert.Fail("Быстрый падающий тест");
    }
}
