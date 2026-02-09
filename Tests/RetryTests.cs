using NUnit.Framework;

namespace Tests;

public class RetryTests
{
    private static int _a = 0;
    private static int _b = 0;
    
    [Test, Retry(4)]
    public void RetryTest1()
    {
        Assert.That(_a++, Is.EqualTo(2));
    }
    
    [Test, Retry(5)]
    public void RetryTest2()
    {
        Assert.That(_b++, Is.EqualTo(7));
    }
    
    
}