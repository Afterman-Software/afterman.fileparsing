using System;
using AI.Scheduler.Jobs;
using AI.Scheduler.Recurrence;

namespace AI.Scheduler.Entities
{
    public class Schedule
    {
        public virtual int Id { get; set; }

        public virtual int ExportedId { get; set; }

        public virtual ScheduleJobStatus Status { get; set; }

        public virtual string Name { get; set; }

        public virtual string Description { get; set; }

        public virtual bool IsRecurring { get; set; }

        public virtual IRecurrenceProvider RecurrenceManager { get; set; }

        public virtual string RecurrenceManagerString { get; set; }

        public virtual DateTime? NextRunDate { get; set; }

        public virtual DateTime? LastRunStarted { get; set; }

        public virtual DateTime? LastRunCompleted { get; set; }

        public virtual IScheduledTask Task { get; set; }

        public virtual string TaskString { get; set; }

        public virtual int OwnerId { get; set; }

        public virtual int ClientId
        {
            get { return this.OwnerId; }
            set { this.OwnerId = value; }
        }

        public virtual bool IsDeleted { get; set; }
    }
}