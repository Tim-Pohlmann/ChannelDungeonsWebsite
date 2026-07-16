using ChannelDungeons.Web.Services;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ChannelDungeons.Web.Tests.Services;

[TestClass]
public class DelayProviderTests
{
    [TestMethod]
    public async Task Delay_Completes()
    {
        var provider = new DelayProvider();
        await provider.Delay(1);
    }

    [TestMethod]
    public async Task Delay_ThrowsWhenCancelled()
    {
        var provider = new DelayProvider();
        using var cts = new CancellationTokenSource();
        cts.Cancel();

        await Assert.ThrowsExceptionAsync<TaskCanceledException>(() => provider.Delay(1000, cts.Token));
    }
}
