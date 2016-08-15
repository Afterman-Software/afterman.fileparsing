using System;

namespace AI.Scheduler.Jobs
{
    public class TaskToTypeAttribute : Attribute
    {
        private readonly Type _viewType;

        public TaskToTypeAttribute(Type viewType)
        {
            _viewType = viewType;
        }

        public Type ViewType { get { return _viewType; } }
    }
}