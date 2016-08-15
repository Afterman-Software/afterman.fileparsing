using AI.Scheduler.Entities;

namespace AI.Scheduler
{
    public interface IJobRunner
    {
        JobHistoryEntry RunJob(Schedule schedule);
    }
}