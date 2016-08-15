using System;
using AI.Scheduler.Entities;

namespace AI.Scheduler.Recurrence
{
    public class RecurrenceCalculator : IRecurrenceCalculator
    {
        public DateTime? GetNextRunDate(Schedule schedule, DateTime? fromDate, DateTime? lastFromDate = null)
        {
            var provider = GetRecurrenceProvider(schedule);
            if (provider == null)
                return null;

            var info = new ScheduleInfo();
            if (fromDate.HasValue)
                info.ScheduleFromDate = fromDate.Value;
            else
                info.ScheduleFromDate = schedule.LastRunStarted;
            if (!info.ScheduleFromDate.HasValue)
                info.ScheduleFromDate = DateTime.Now;

            DateTime? nextRun = provider.GetNextRunDate(info, schedule, lastFromDate);
            while (nextRun.HasValue && nextRun.Value < DateTime.Now)
            {
                nextRun = provider.GetNextRunDate(info, schedule, nextRun);
            }
            return nextRun;
        }

        private IRecurrenceProvider GetRecurrenceProvider(Schedule schedule)
        {
            return schedule.RecurrenceManager;
        }
    }
}