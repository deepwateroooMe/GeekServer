using System;

// 时间戳工具类
public class TimeUtils {

    private static readonly DateTime epochLocal = TimeZoneInfo.ConvertTime(new DateTime(1970, 1, 1), TimeZoneInfo.Local);
    private static readonly DateTime epochUtc = TimeZoneInfo.ConvertTime(new DateTime(1970, 1, 1), TimeZoneInfo.Utc);

    public static long CurrentTimeMillis(bool utc = false) {
        return TimeMillis(DateTime.Now, utc);
    }

    // 某个时间的毫秒数
    public static long TimeMillis(DateTime time, bool utc = false) {
        if (utc)
            return (long)(time - epochUtc).TotalMilliseconds;
        return (long)(time - epochLocal).TotalMilliseconds;
    }

    // 毫秒转时间
    public static DateTime MillisToDateTime(long time, bool utc = false) {
        if (utc)
            return epochUtc.AddMilliseconds(time);
        return epochLocal.AddMilliseconds(time);
    }
}