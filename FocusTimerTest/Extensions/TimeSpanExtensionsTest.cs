using FocusTimer.Library.Extensions;

namespace FocusTimerTest.Extensions;

[TestFixture]
public class TimeSpanExtensionsTest
{
    [Test]
    public void TimeSpanOver24Hours_ShouldNotClip()
    {
        var ts = new TimeSpan(36, 10, 12);
        
        Assert.That(ts.ToSixDigits(), Is.EqualTo("36:10:12"));
    }
}