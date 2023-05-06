using Microsoft.Extensions.Logging;

namespace SmartTasks;

public class TasksRunner
{
    private const uint mNoAvailableSlotDefaultLogInterval = 1000;
    
    private readonly Task?[] mAllRunningTasks;
    private readonly TimeSpan mIntervalForCheckingAvailableSlot;
    private readonly uint? mNoAvailableSlotLogInterval;
    private readonly ILogger<TasksRunner>? mLogger;
    
    public TasksRunner(TasksRunnerConfigurations tasksRunnerConfigurations)
    {
        ushort fixedAllowedParallelTasks = tasksRunnerConfigurations.AllowedParallelTasks == 0u 
            ? (ushort)1 
            : tasksRunnerConfigurations.AllowedParallelTasks;
        mAllRunningTasks = new Task[fixedAllowedParallelTasks];

        mIntervalForCheckingAvailableSlot = tasksRunnerConfigurations.IntervalForCheckingAvailableSlot == TimeSpan.Zero
            ? throw new ArgumentException($"{nameof(tasksRunnerConfigurations.IntervalForCheckingAvailableSlot)} cannot be zero")
            : tasksRunnerConfigurations.IntervalForCheckingAvailableSlot;

        mNoAvailableSlotLogInterval = tasksRunnerConfigurations.NoAvailableSlotLogInterval ?? mNoAvailableSlotDefaultLogInterval;
        mLogger = tasksRunnerConfigurations.Logger;
    }

    /// <summary>
    /// Becomes blocking operation when maximal tasks allowed are running.
    /// </summary>
    public async Task RunTask(Task task, CancellationToken cancellationToken)
    {
        ushort? indexToPlaceTask = null;

        int callNumber = 0;
        while (indexToPlaceTask is null)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                mLogger?.LogInformation($"Cancel requested");
                break;
            }

            callNumber++;
            indexToPlaceTask = TryFindIndexToLocateTask(ref callNumber);

            if (indexToPlaceTask is null)
            {
                await Task.Delay(mIntervalForCheckingAvailableSlot, cancellationToken);
            }
        }

        if (indexToPlaceTask is null)
        {
            // In case of cancellation.
            return;
        }
        
        InsertTaskByIndex(indexToPlaceTask.Value, task);

        await WaitOneIfRequired(cancellationToken);
    }

    public async Task<bool> WaitAll(CancellationToken cancellationToken)
    {
        mLogger?.LogInformation($"Waiting for all tasks to complete");
        
        for (ushort i = 0; i < mAllRunningTasks.Length; ++i)
        {
            Task? task = mAllRunningTasks[i];

            while (task is not null && !task.IsCompleted)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    mLogger?.LogInformation($"Cancel requested");
                    return false;
                }
                
                await Task.Delay(mIntervalForCheckingAvailableSlot, cancellationToken);
            }
        }

        return true;
    }

    /// <summary>
    /// Waits for one slot to be available. 
    /// </summary>
    private async Task WaitOneIfRequired(CancellationToken cancellationToken)
    {
        ushort? indexToPlaceTask = null;
        int callNumber = 0;
        while (indexToPlaceTask is null)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                mLogger?.LogInformation($"Cancel requested");
                break;
            }

            callNumber++;
            indexToPlaceTask = TryFindIndexToLocateTask(ref callNumber);

            if (indexToPlaceTask is null)
            {
                await Task.Delay(mIntervalForCheckingAvailableSlot, cancellationToken);
            }
        }

        if (!cancellationToken.IsCancellationRequested)
        {
            mLogger?.LogTrace($"At least one task slot is available");   
        }
    }

    private ushort? TryFindIndexToLocateTask(ref int callNumber)
    {
        for (ushort i = 0; i < mAllRunningTasks.Length; ++i)
        {
            Task? task = mAllRunningTasks[i];
            if (task is null || task.IsCompleted)
            {
                mLogger?.LogTrace($"Found available slot at index {i}");
                return i;
            }

            if (callNumber == 1 || callNumber % mNoAvailableSlotLogInterval == 0)
            {
                mLogger?.LogTrace($"Slot at index {i} has running task");
            }
        }

        if (callNumber == 1 || callNumber % mNoAvailableSlotLogInterval == 0)
        {
            mLogger?.LogTrace("No available slot found");
            if (callNumber % mNoAvailableSlotLogInterval == 0)
            {
                callNumber = 0;
            }
        }
        
        return null;
    }

    private void InsertTaskByIndex(ushort indexToPlaceTask, Task task)
    {
        Task? oldTask = mAllRunningTasks[indexToPlaceTask];
        oldTask?.Dispose();

        mLogger?.LogTrace($"Inserting task into index {indexToPlaceTask}");
        mAllRunningTasks[indexToPlaceTask] = task;
    }
}
