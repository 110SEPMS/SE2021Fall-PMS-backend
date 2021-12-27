using System;

namespace project_manage_system_backend.Shares
{
    public static class DateHandler
    {
        public static string ConvertToDateString(long javascriptTicks)
        {
            const long baseTick = 621355968000000000;
            var dateTime = new DateTime(javascriptTicks * 10000000 + baseTick);
            return dateTime.ToString("MM/dd");
        }

        public static string ConvertToDayOfWeek(int dayOfWeek)
        {
            return dayOfWeek switch
            {
                0 => "Sunday",
                1 => "Monday",
                2 => "Tuesday",
                3 => "Wednesday",
                4 => "Thursday",
                5 => "Friday",
                6 => "Saturday",
                _ => throw new Exception("error dayOfWeek Number"),
            };
        }
    }
}
