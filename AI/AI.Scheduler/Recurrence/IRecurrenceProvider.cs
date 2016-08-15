using System;
using AI.Scheduler.Entities;

namespace AI.Scheduler.Recurrence
{
    public interface IRecurrenceProvider
    {
        DateTime? GetNextRunDate(ScheduleInfo scheduleInfo, Schedule schedule, DateTime? lastFromDate);

        string Key { get; }

        string GuidString { get; }

        bool IsEquivalent(IRecurrenceProvider compareProvider);
    }
}