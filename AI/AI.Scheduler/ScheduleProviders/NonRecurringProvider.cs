using System;
using AI.Scheduler.Entities;
using AI.Scheduler.Recurrence;

namespace AI.Scheduler.ScheduleProviders
{
    public class NonRecurringProvider : IRecurrenceProvider
    {
        public DateTime? GetNextRunDate(ScheduleInfo scheduleInfo, Schedule schedule, DateTime? lastFromDate)
        {
            if (schedule.LastRunCompleted.HasValue)
                return null;

            var curDate = scheduleInfo.ScheduleFromDate.Value;
            var curTime = DateTime.Parse(Time);
            var nextRun = new DateTime(curDate.Year, curDate.Month, curDate.Day, curTime.Hour, curTime.Minute, curTime.Second);
            if (curDate > nextRun)
                nextRun = nextRun.AddDays(1);
            return nextRun;
        }

        public string Time { get; set; }

        public string Key
        {
            get { return "NonRecurringProvider"; }
        }

        public string GuidString
        {
            get { return ""; }
        }

        public bool IsEquivalent(IRecurrenceProvider compareProvider)
        {
            NonRecurringProvider compareNRP = compareProvider as NonRecurringProvider;
            if (compareNRP == null)
                return false;

            var thisTime = string.IsNullOrWhiteSpace(Time) ? "" : Time.Trim().ToLowerInvariant();
            var compareTime = string.IsNullOrWhiteSpace(compareNRP.Time) ? "" : compareNRP.Time.Trim().ToLowerInvariant();

            return thisTime == compareTime;
        }
    }
}