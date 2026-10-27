using MyBudg.Common;

namespace MyBudg.Tests.Common;

public class RegistrationPolicyTests
{
    [Fact]
    public void Default_IsClosed()
    {
        var o = new RegistrationOptions();
        Assert.False(RegistrationPolicy.IsAvailable(o));
        Assert.False(RegistrationPolicy.Check(o, "anything").Succeeded);
    }

    [Fact]
    public void Open_AllowsWithoutCode() =>
        Assert.True(RegistrationPolicy.Check(new RegistrationOptions { Mode = RegistrationMode.Open }, null).Succeeded);

    [Theory]
    [InlineData("secret-code", true)]
    [InlineData("  secret-code  ", true)]
    [InlineData("secret-codX", false)]
    [InlineData("", false)]
    [InlineData(null, false)]
    public void InviteOnly_RequiresMatchingCode(string? code, bool allowed)
    {
        var o = new RegistrationOptions { Mode = RegistrationMode.InviteOnly, InviteCode = "secret-code" };
        Assert.True(RegistrationPolicy.RequiresCode(o));
        Assert.Equal(allowed, RegistrationPolicy.Check(o, code).Succeeded);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void InviteOnly_WithoutConfiguredCode_FailsClosed(string? configured)
    {
        var o = new RegistrationOptions { Mode = RegistrationMode.InviteOnly, InviteCode = configured };
        Assert.False(RegistrationPolicy.IsAvailable(o));
        Assert.False(RegistrationPolicy.Check(o, configured).Succeeded);
    }
}
