using System;

namespace AI.Scheduler.Jobs
{
    public interface IScheduledTask
    {
        bool Run();

        string Key { get; }

        Guid RunId { get; }

        int ClientId { get; set; }

        int FileTypeId { get; set; }

        string FileTypeName { get; set; }

        bool IsEquivalent(IScheduledTask compareTask);
    }
}