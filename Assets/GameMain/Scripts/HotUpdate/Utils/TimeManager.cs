using System;
using System.Globalization;
using BlockPuzzleGameToolkit.Scripts.Data;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Quester
{
    public class TimeManager
    {
        private const int SecondsOneWeek = 3600 * 24 * 7;
        public static SeasonTime SeasonTime;
        public static float StartupTime;
        
        public static void SetSeasonTimeByUnlockAdventureTime()
        {
            if (UserDataManager.Instance.UnlockAdventureTimestamp != 0)
            {
                long startTime = UserDataManager.Instance.UnlockAdventureTimestamp;
                long nowTime = DateTimeOffset.Now.ToUnixTimeSeconds();
                while (startTime + SecondsOneWeek < nowTime)
                {
                    startTime += SecondsOneWeek;
                }
                
                DateTime dateTime = DateTimeOffset
                    .FromUnixTimeSeconds(startTime)
                    .LocalDateTime;

                Debug.Log(dateTime.ToString("yyyy-MM-dd HH:mm:ss"));
                
                var seasonTime = new SeasonTime();
                seasonTime.seasonStartTime = startTime;
                seasonTime.seasonEndTime = startTime + SecondsOneWeek;
                SeasonTime = seasonTime;
                SetSeasonData();
            }
        }
        
        // public static void SetSeasonTime(Action<bool> callback)
        // {
        //     var requestConfig = new NetworkRequest()
        //     {
        //         url = "http://api.xjoy.games/block/season",
        //         method = NetworkRequest.HttpMethod.GET,
        //         retryCount = 1,
        //         timeout = 5
        //     };
        //     GameEntry.Http.SendRequest(
        //         requestConfig,
        //         res =>
        //         {
        //             var responseTime = res.GetData<ApiResponse<SeasonTime>>();
        //             if (res.success && responseTime != null)
        //             {
        //                 SeasonTime = responseTime.data;
        //             }
        //             if(SeasonTime == null)
        //             {
        //                 SeasonTime = GetSeasonTimeFromLocal();
        //             }
        //             StartupTime = Time.realtimeSinceStartup;
        //             SetSeason();
        //             callback.Invoke(res.success);
        //         }
        //     );
        // }
        
        private static void SetSeasonData()
        {
            var seasonTime = SeasonTime.seasonStartTime;
            if (seasonTime != UserDataManager.Instance.CurrentSeasonTimestamp)
            {
                UserDataManager.Instance.SetLevel(1);
                UserDataManager.Instance.SetSeason(seasonTime);
                UserDataManager.Instance.SetSeasonShape(Random.Range(1, 11));
                UserDataManager.Instance.SetLevelGroup(1);
                if (UserDataManager.Instance.FirstSeasonTimestamp == 0)
                {
                    UserDataManager.Instance.SetFirstSeason(seasonTime);
                }
            }
        }
        
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
            // Debug.Log($"currentTime; {currentTime}");
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

            // Debug.Log($"当前日期：{currentDate.ToShortDateString()}");
            // Debug.Log($"当前是 {year} 年的第 {week} 周");
            
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