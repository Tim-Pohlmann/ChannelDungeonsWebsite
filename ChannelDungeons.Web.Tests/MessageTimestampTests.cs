using ChannelDungeons.Web.Services;

namespace ChannelDungeons.Web.Tests;

[TestClass]
public sealed class MessageTimestampTests
{
    [TestMethod]
    public void Afternoon_FormatsAsTwelveHourWithUppercasePm()
    {
        var time = new DateTimeOffset(2026, 7, 18, 15, 7, 0, TimeSpan.Zero);

        Assert.AreEqual("3:07 PM", MessageTimestamp.Format(time));
    }

    [TestMethod]
    public void Morning_FormatsAsTwelveHourWithUppercaseAm()
    {
        var time = new DateTimeOffset(2026, 7, 18, 9, 5, 0, TimeSpan.Zero);

        Assert.AreEqual("9:05 AM", MessageTimestamp.Format(time));
    }

    [TestMethod]
    public void Midnight_FormatsAsTwelve()
    {
        var time = new DateTimeOffset(2026, 7, 18, 0, 30, 0, TimeSpan.Zero);

        Assert.AreEqual("12:30 AM", MessageTimestamp.Format(time));
    }
}
