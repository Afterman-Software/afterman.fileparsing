using System;
using AI.Scheduler.Entities;

namespace AI.Scheduler.Recurrence
{
    public interface IRecurrenceCalculator
    {
        DateTime? GetNextRunDate(Schedule schedule, DateTime? fromDate, DateTime? lastFromDate = null);
    }
}
