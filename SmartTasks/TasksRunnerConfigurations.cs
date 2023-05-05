using Microsoft.Extensions.Logging;

namespace SmartTasks;

public class TasksRunnerConfigurations
{
    public required ushort AllowedParallelTasks { get; init; }
    
    public required TimeSpan IntervalForCheckingAvailableSlot { get; init; }

    public ILogger<TasksRunner>? Logger { get; init; }
    
    public uint? NoAvailableSlotLogInterval { get; init; }
}