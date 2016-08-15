using System;

namespace AI.Scheduler.Entities
{
    public class JobHistoryEntry
    {
        public virtual int Id { get; set; }

        public virtual DateTime Started { get; set; }

        public virtual DateTime? Completed { get; set; }

        public virtual ScheduleJobStatus Status { get; set; }

        public virtual string Details { get; set; }

        public virtual Schedule ScheduledJob { get; set; }

        public virtual Guid JobId { get; set; }
    }
}