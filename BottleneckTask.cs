using System;

namespace Bottleneck
{
    public enum TaskType
    {
        MadeOn,
        Deficit
    }
    public class BottleneckTask
    {
        public TaskType taskType;
        public UIStatisticsWindow statsWindow;
        public DateTime createdAt = DateTime.Now;
    }
}