using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using AI.Scheduler.Entities;
using AI.Scheduler.Recurrence;

namespace AI.Scheduler.ScheduleProviders
{
    public class RecurringProvider : IRecurrenceProvider
    {
        static DateTimeFormatInfo dtfi = DateTimeFormatInfo.CurrentInfo;
        static Calendar cal = dtfi.Calendar;

        public RecurringProvider()
        {
            this.ExpectedEveryDays = new List<int>();
            this.ExpectedEveryMonths = new List<int>();
            this.ExpectedEveryMonthsOnDays = new List<int>();
        }

        private int GetDayOfWeek(DateTime getFromDateTime)
        {
            return (int)cal.GetDayOfWeek(getFromDateTime);
        }

        private int GetDayOfMonth(DateTime getFromDateTime)
        {
            return cal.GetDayOfMonth(getFromDateTime);
        }

        private int GetDayOfYear(DateTime getFromDateTime)
        {
            return cal.GetDayOfYear(getFromDateTime);
        }

        private int GetWeekOfYear(DateTime getFromDateTime)
        {
            return cal.GetWeekOfYear(getFromDateTime, CalendarWeekRule.FirstDay, DayOfWeek.Monday);
        }

        private int GetMonthOfYear(DateTime getFromDateTime)
        {
            return cal.GetMonth(getFromDateTime);
        }

        public DateTime? GetNextRunDate(ScheduleInfo scheduleInfo, Schedule schedule, DateTime? lastFromDate)
        {
            var curDate = scheduleInfo.ScheduleFromDate.Value;
            if (lastFromDate.HasValue)
                curDate = lastFromDate.Value;
            var curTime = DateTime.Parse(Time);
            var curDateTime = new DateTime(curDate.Year, curDate.Month, curDate.Day, curTime.Hour, curTime.Minute, curTime.Second);
            var nextDateTime = curDateTime;

            switch (Expected.ToLowerInvariant())
            {
                case "daily":
                    if (lastFromDate.HasValue && lastFromDate.Value == curDateTime)
                    {
                        try
                        {
                            return nextDateTime.AddDays(1);
                        }
                        catch
                        {
                            return null;
                        }
                    }

                    if (IsGoodTimeForToday(nextDateTime))
                        return nextDateTime;

                    try
                    {
                        return nextDateTime.AddDays(1);
                    }
                    catch
                    {
                        return null;
                    }
                case "weekly":
                    if (lastFromDate.HasValue && lastFromDate.Value == curDateTime)
                    {
                        try
                        {
                            nextDateTime = nextDateTime.AddDays(1);
                        }
                        catch
                        {
                            return null;
                        }
                    }

                    if (IsGoodDayOfWeek(nextDateTime) && IsGoodTimeForToday(nextDateTime))
                        return nextDateTime;

                    while (!IsGoodDayOfWeek(nextDateTime))
                    {
                        try
                        {
                            nextDateTime = nextDateTime.AddDays(1);
                        }
                        catch
                        {
                            return null;
                        }
                    }
                    return nextDateTime;
                case "monthly":
                    if (lastFromDate.HasValue && lastFromDate.Value == curDateTime)
                    {
                        try
                        {
                            nextDateTime = nextDateTime.AddDays(1);
                        }
                        catch
                        {
                            return null;
                        }
                    }

                    if (IsGoodMonthOfYear(nextDateTime) && IsGoodDayOfMonth(nextDateTime) && IsGoodTimeForToday(nextDateTime))
                        return nextDateTime;

                    while (!(IsGoodMonthOfYear(nextDateTime) && IsGoodDayOfMonth(nextDateTime)))
                    {
                        try
                        {
                            nextDateTime = nextDateTime.AddDays(1);
                        }
                        catch
                        {
                            return null;
                        }
                    }
                    return nextDateTime;
            }
            return null;
        }

        public string Time { get; set; }

        public virtual string Expected { get; set; }

        public virtual string ExpectedWhen { get; set; }

        public virtual List<int> ExpectedEveryDays { get; set; }

        public virtual List<int> ExpectedEveryMonths { get; set; }

        public virtual List<int> ExpectedEveryMonthsOnDays { get; set; }

        public string Key
        {
            get { return "RecurringProvider"; }
        }

        public string GuidString
        {
            get { return ""; }
        }

        private bool IsGoodDayOfWeek(DateTime checkDateTime)
        {
            if (ExpectedEveryDays == null || ExpectedEveryDays.Count == 0)
            {
                return true;
            }

            if (ExpectedEveryDays.Contains((int)checkDateTime.DayOfWeek))
            {
                return true;
            }

            return false;
        }

        private bool IsGoodTimeForToday(DateTime checkDateTime)
        {
            if (DateTime.Now.Date != checkDateTime.Date)
                return false;

            return DateTime.Now.TimeOfDay < checkDateTime.TimeOfDay;
        }

        private bool IsGoodMonthOfYear(DateTime checkDateTime)
        {
            if (ExpectedEveryMonths == null || ExpectedEveryMonths.Count == 0)
            {
                return true;
            }

            if (ExpectedEveryMonths.Contains(checkDateTime.Month))
            {
                return true;
            }

            return false;
        }

        private bool IsGoodDayOfMonth(DateTime checkDateTime)
        {
            if (ExpectedEveryMonthsOnDays == null || ExpectedEveryMonthsOnDays.Count == 0)
                return true;

            int lastDay = 0;
            int[] has30 = { 4, 6, 9, 11 };
            int[] has31 = { 1, 3, 5, 7, 8, 10, 12 };
            int febLast = (checkDateTime.Year % 4 == 0 && checkDateTime.Year % 100 != 0 ? 29 : 28);
            if (has30.Contains(checkDateTime.Month)) lastDay = 30;
            if (has31.Contains(checkDateTime.Month)) lastDay = 31;
            if (checkDateTime.Month == 2) lastDay = febLast;

            if (checkDateTime.Day <= 31 && ExpectedEveryMonthsOnDays.Contains(checkDateTime.Day))
            {
                return true;
            }
            else if (checkDateTime.Day == lastDay && ExpectedEveryMonthsOnDays.Contains(40))
            {
                return true;
            }
            else
            {
                int dayOfWeek = (int)checkDateTime.DayOfWeek;
                int day = checkDateTime.Day;
                int dayOfWeekNum = (((day.Between(1, 7) ? 1 : (day.Between(8, 14) ? 2 : (day.Between(15, 21) ? 3 : (day.Between(22, 28) ? 4 : 5)))) + 4) * 10) + dayOfWeek;

                return ExpectedEveryMonthsOnDays.Contains(dayOfWeekNum);
            }
        }

        public bool IsEquivalent(IRecurrenceProvider compareProvider)
        {
            RecurringProvider compareRP = compareProvider as RecurringProvider;
            if (compareRP == null)
                return false;

            var thisTime = string.IsNullOrWhiteSpace(Time) ? "" : Time.Trim().ToLowerInvariant();
            var compareTime = string.IsNullOrWhiteSpace(compareRP.Time) ? "" : compareRP.Time.Trim().ToLowerInvariant();
            var thisExpected = string.IsNullOrWhiteSpace(Expected) ? "" : Expected.Trim().ToLowerInvariant();
            var compareExpected = string.IsNullOrWhiteSpace(compareRP.Expected) ? "" : compareRP.Expected.Trim().ToLowerInvariant();
            var thisExpectedWhen = string.IsNullOrWhiteSpace(ExpectedWhen) ? "" : ExpectedWhen.Trim().ToLowerInvariant();
            var compareExpectedWhen = string.IsNullOrWhiteSpace(compareRP.ExpectedWhen) ? "" : compareRP.ExpectedWhen.Trim().ToLowerInvariant();
            var thisExpectedEveryDays = ExpectedEveryDays == null ? new List<int>() : ExpectedEveryDays;
            var compareExpectedEveryDays = compareRP.ExpectedEveryDays == null ? new List<int>() : compareRP.ExpectedEveryDays;
            var thisExpectedEveryMonths = ExpectedEveryMonths == null ? new List<int>() : ExpectedEveryMonths;
            var compareExpectedEveryMonths = compareRP.ExpectedEveryMonths == null ? new List<int>() : compareRP.ExpectedEveryMonths;
            var thisExpectedEveryMonthsOnDays = ExpectedEveryMonthsOnDays == null ? new List<int>() : ExpectedEveryMonthsOnDays;
            var compareExpectedEveryMonthsOnDays = compareRP.ExpectedEveryMonthsOnDays == null ? new List<int>() : compareRP.ExpectedEveryMonthsOnDays;

            return thisTime == compareTime
                && thisExpected == compareExpected
                && thisExpectedWhen == compareExpectedWhen
                && thisExpectedEveryDays.HasSameContent(compareExpectedEveryDays)
                && thisExpectedEveryMonths.HasSameContent(compareExpectedEveryMonths)
                && thisExpectedEveryMonthsOnDays.HasSameContent(compareExpectedEveryMonthsOnDays);
        }
    }
}