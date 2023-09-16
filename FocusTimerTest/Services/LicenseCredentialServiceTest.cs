using FocusTimer.Domain.Services;

namespace FocusTimerTest.Services;

public class LicenseCredentialServiceTest
{
    private LicenseCredentialService _service = null!;
    
    [SetUp]
    public void Setup()
    {
        _service = new LicenseCredentialService();
    }

    [Test]
    public void ValidKey_ShouldBeTreatedValid()
    {
        const string validKey = "1227-7BCD-A5AC-9861-B81D-2996";

        var actual = _service.ValidateLicenseKey(validKey);
        
        Assert.That(actual, Is.True);
    }
    
    [Test]
    public void InvalidKey_ShouldBeTreatedInvalid()
    {
        const string invalidKey = "1111-2222-3333-4444-5555-6666";

        var actual = _service.ValidateLicenseKey(invalidKey);
        
        Assert.That(actual, Is.False);
    }
    
    [Test]
    public void ShouldSurviveNonHex()
    {
        const string invalidKey = "hello haha";

        var actual = _service.ValidateLicenseKey(invalidKey);
        
        Assert.That(actual, Is.False);
    }
}