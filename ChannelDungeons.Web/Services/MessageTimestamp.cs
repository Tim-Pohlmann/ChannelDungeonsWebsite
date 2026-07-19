using System.Globalization;

namespace ChannelDungeons.Web.Services;

public static class MessageTimestamp
{
    /// <summary>
    /// Formats a time like the original site: 12-hour clock with uppercase
    /// AM/PM (e.g. "3:07 PM"). Invariant culture keeps the format stable
    /// regardless of the visitor's locale, matching the original design.
    /// </summary>
    public static string Format(DateTimeOffset time) =>
        time.ToString("h:mm tt", CultureInfo.InvariantCulture);
}
