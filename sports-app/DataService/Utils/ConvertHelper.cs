
using System;
using System.Linq;

namespace DataService.Utils
{
    public static class ConvertHelper
    {
        public const long MilisecondsInMinute = 60000;

        public const long MilisecondsInSecond = 1000;

        public static byte ToByte(this string byeStringValue)
        {
            if (string.IsNullOrEmpty(byeStringValue))
                return 0;

            return Convert.ToByte(byeStringValue);
        }

        public static long? ToMiliseconds(this double? seconds)
        {
            return seconds.HasValue ? Convert.ToInt64(seconds * MilisecondsInMinute) : 0;
        }

        public static double ToMinutesFromMiliseconds(this long? miliseconds)
        {
            return miliseconds.HasValue ? (double)miliseconds / MilisecondsInMinute : 0D;
        }

        public static string ToMinutesString(this long? miliseconds)
        {
            string result = string.Empty;
            if (miliseconds != null && miliseconds > 0)
            {
                TimeSpan time = TimeSpan.FromMilliseconds(double.Parse(miliseconds?.ToString()));
                var minutes = time.Hours * 60 + time.Minutes;
                result = string.Format("{0:D2}:{1:D2}",
                                        minutes,
                                        time.Seconds);
            }
            return result;
        }

        public static long? ToMilisecondsFromString(this string timeString)
        {
            var time = timeString.Split(':').Select(long.Parse).ToList();
            var minutes = time[0];
            var seconds = time[1];

            return (minutes * MilisecondsInMinute) + (seconds * MilisecondsInSecond);
        }

        public static double BuildResultSortValue(string result, int? format)
        {
            if (format == null)
            {
                format = 0;
            }
            if (result != null)
            {
                result = result.Trim();
            }
            if (string.IsNullOrWhiteSpace(result))
            {
                return 0;
            }
            switch (format)
            {
                case 1:
                    {
                        var str = $"00:00:{result}";
                        TimeSpan _time = TimeSpan.Parse(str);
                        return _time.TotalMilliseconds;
                    }
                case 2:
                    {
                        var str = $"00:{result}";
                        TimeSpan _time = TimeSpan.Parse(str);
                        return _time.TotalMilliseconds;
                    }
                case 3:
                    {
                        var str = $"00:{result}";
                        TimeSpan _time = TimeSpan.Parse(str);
                        return _time.TotalMilliseconds;
                    }
                case 4:
                    {
                        var str = $"{result}";
                        TimeSpan _time = TimeSpan.Parse(str);
                        return _time.TotalMilliseconds;
                    }
                case 5:
                    {
                        var str = $"{result}.00";
                        TimeSpan _time = TimeSpan.Parse(str);
                        return _time.TotalMilliseconds;
                    }
                case 9:
                    {
                        var str = $"00:{result}.00";
                        TimeSpan _time = TimeSpan.Parse(str);
                        return _time.TotalMilliseconds;
                    }
                case 10:
                case 6:
                    {
                        var str = $"{result}";
                        return (Convert.ToDouble(str) * 1000);
                    }
                case 11:
                case 7:
                    {
                        var str = $"{result}";
                        return (Convert.ToDouble(str) * 1000);
                    }
                case 8:
                    {
                        var str = $"{result}";
                        return (Convert.ToDouble(str) * 1000);
                    }
                default:
                    {
                        double num;
                        bool res = double.TryParse(result, out num);
                        if (res == false)
                        {
                            return 0;
                        }
                        return num;
                    }
            }
        }

        public static int? GetAge(this DateTime? birthDay)
        {
            if (birthDay.HasValue)
            {
                var today = DateTime.Today;
                int age = today.Year - birthDay.Value.Year;
                if (birthDay > today.AddYears(-age)) age--;
                return age;
            }
            return null;
        }
    }
}
