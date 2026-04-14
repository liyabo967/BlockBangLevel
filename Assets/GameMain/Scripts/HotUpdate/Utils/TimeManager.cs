using System;
using System.Globalization;
using UnityEngine;

namespace Quester
{
    public class TimeManager
    {
        public static SeasonTime SeasonTime;
        public static float StartupTime;
        
        public static void SetSeasonTime(Action<bool> callback)
        {
            var requestConfig = new NetworkRequest()
            {
                url = "http://api.xjoy.games/block/season",
                method = NetworkRequest.HttpMethod.GET,
                retryCount = 3
            };
            GameEntry.Http.SendRequest(
                requestConfig,
                res =>
                {
                    var success = res.success;
                    if (success)
                    {
                        SeasonTime = res.GetData<ApiResponse<SeasonTime>>().data;
                    }
                    else
                    {
                        SeasonTime = GetSeasonTimeFromLocal();
                    }
                    StartupTime = Time.realtimeSinceStartup;
                    SetSeason();
                    callback.Invoke(success);
                }
            );
        }
        
        private static void SetSeason()
        {
            int season = SeasonTime.year * 100 + SeasonTime.week;
            // if (season != UserDataManager.Instance.GetService().CurrentSeason)
            // {
            //     UserDataManager.Instance.GetService().Level = 1;
            //     UserDataManager.Instance.GetService().CurrentSeason = season;
            // }
        }
        
        
        private static int SecondsOneWeek = 3600 * 24 * 7;
        
        public static long GetCurrentTime()
        {
            if (SeasonTime == null)
            {
                return DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            }
            else
            {
                float delta = Time.realtimeSinceStartup - StartupTime;
                return SeasonTime.serverTime + (long)delta;
            }
        }
        
        public static SeasonTime GetSeasonTimeFromLocal()
        {
            // 获取当前日期和年份
            var currentTime = GetCurrentTime();
            Debug.LogError($"currentTime; {currentTime}");
            DateTime currentDate = DateTimeOffset.FromUnixTimeSeconds(currentTime).DateTime;
            int year = currentDate.Year;
        
            // 获取当前区域性的日历
            CultureInfo currentCulture = CultureInfo.CurrentCulture;
            Calendar calendar = currentCulture.Calendar;
        
            // 计算当前周数
            int week = calendar.GetWeekOfYear(
                currentDate,
                CalendarWeekRule.FirstFourDayWeek, 
                DayOfWeek.Monday
            );

            Debug.Log($"当前日期：{currentDate.ToShortDateString()}");
            Debug.Log($"当前是 {year} 年的第 {week} 周");
            
            SeasonTime seasonTime = new SeasonTime();
            seasonTime.serverTime = currentTime;
            seasonTime.year = year;
            seasonTime.week = week;
            seasonTime.seasonStartTime = GetMondayTimestampOfWeek(year, week);
            Debug.Log($"seasonTime.seasonStartTime: {seasonTime.seasonStartTime}");
            seasonTime.seasonEndTime = seasonTime.seasonStartTime + SecondsOneWeek; 
            Debug.Log($"seasonTime.seasonEndTime: {seasonTime.seasonEndTime}");
            return seasonTime;
        }
        
        public static long GetMondayTimestampOfWeek(int year, int week)
        {
            // ISO 8601：周一为第一天
            // 一年的第一周包含 1 月 4 日
            DateTime jan4 = new DateTime(year, 1, 4);

            // 找到该周的周一
            int dayOfWeek = (int)jan4.DayOfWeek;
            if (dayOfWeek == 0)
            {
                dayOfWeek = 7;
            }
            DateTime firstMonday = jan4.AddDays(1 - dayOfWeek);

            // 目标周的周一
            DateTime monday = firstMonday.AddDays((week - 1) * 7);

            return new DateTimeOffset(monday).ToUnixTimeSeconds();
        }
    }
}