using FocusTimer.Domain.Entities;
using FocusTimer.Domain.Services;

namespace FocusTimerTest.Services;

public class UsageSplitterTest
{
    [Test]
    public void OneDay_ShouldReturnOnlyOne()
    {
        var splitter = new UsageSplitter<TimerRunningUsage>(new TimerRunningUsage
        {
            StartedAt = DateTime.Parse("2023-08-26 09:01:00"),
            UpdatedAt = DateTime.Parse("2023-08-26 10:02:00")
        });

        var result = splitter.Split();
        
        Assert.Multiple(() =>
        {
            Assert.That(result.Count(), Is.EqualTo(1));
            
            Assert.That(result.First().StartedAt, Is.EqualTo(DateTime.Parse("2023-08-26 09:01:00")));
            Assert.That(result.First().UpdatedAt, Is.EqualTo(DateTime.Parse("2023-08-26 10:02:00")));
        });
    }
    
    [Test]
    public void TwoDays_ShouldReturnTwo()
    {
        var splitter = new UsageSplitter<TimerRunningUsage>(new TimerRunningUsage
        {
            StartedAt = DateTime.Parse("2023-08-25 09:01:00"),
            UpdatedAt = DateTime.Parse("2023-08-26 10:02:00")
        });

        var result = splitter.Split();
        
        Assert.Multiple(() =>
        {
            Assert.That(result.Count(), Is.EqualTo(2));
            
            Assert.That(result.First().StartedAt, Is.EqualTo(DateTime.Parse("2023-08-25 09:01:00")));
            Assert.That(result.First().UpdatedAt, Is.EqualTo(DateTime.Parse("2023-08-26 00:00:00")));
            
            Assert.That(result.Last().StartedAt, Is.EqualTo(DateTime.Parse("2023-08-26 00:00:00")));
            Assert.That(result.Last().UpdatedAt, Is.EqualTo(DateTime.Parse("2023-08-26 10:02:00")));
        });
    }
    
    [Test]
    public void ThreeDays_ShouldReturnThree()
    {
        var splitter = new UsageSplitter<TimerRunningUsage>(new TimerRunningUsage
        {
            StartedAt = DateTime.Parse("2023-08-25 09:01:00"),
            UpdatedAt = DateTime.Parse("2023-08-27 10:02:00")
        });

        var result = splitter.Split();
        
        Assert.Multiple(() =>
        {
            Assert.That(result.Count(), Is.EqualTo(3));
            
            Assert.That(result.First().StartedAt, Is.EqualTo(DateTime.Parse("2023-08-25 09:01:00")));
            Assert.That(result.First().UpdatedAt, Is.EqualTo(DateTime.Parse("2023-08-26 00:00:00")));

            Assert.That(result.Skip(1).First().StartedAt, Is.EqualTo(DateTime.Parse("2023-08-26 00:00:00")));
            Assert.That(result.Skip(1).First().UpdatedAt, Is.EqualTo(DateTime.Parse("2023-08-27 00:00:00")));
            
            Assert.That(result.Last().StartedAt, Is.EqualTo(DateTime.Parse("2023-08-27 00:00:00")));
            Assert.That(result.Last().UpdatedAt, Is.EqualTo(DateTime.Parse("2023-08-27 10:02:00")));
        });
    }
}