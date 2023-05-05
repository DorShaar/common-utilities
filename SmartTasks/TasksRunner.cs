using Microsoft.Extensions.Logging;

namespace SmartTasks;

public class TasksRunner
{
    private readonly Task?[] mAllRunningTasks;
    private readonly TimeSpan mIntervalForCheckingAvailableSlot;
    private readonly TimeSpan mDefaultIntervalForCheckingAvailableSlot = TimeSpan.FromMilliseconds(500);
    private readonly ILogger<TasksRunner>? mLogger;
    
    public TasksRunner(ushort allowedParallelTasks, TimeSpan intervalForCheckingAvailableSlot, ILogger<TasksRunner>? logger = null)
    {
        ushort fixedAllowedParallelTasks = allowedParallelTasks == 0u ? (ushort)1 : allowedParallelTasks;
        mAllRunningTasks = new Task[fixedAllowedParallelTasks];

        mIntervalForCheckingAvailableSlot = intervalForCheckingAvailableSlot == TimeSpan.Zero
            ? mDefaultIntervalForCheckingAvailableSlot
            : intervalForCheckingAvailableSlot;  
        mIntervalForCheckingAvailableSlot = intervalForCheckingAvailableSlot;
        
        mLogger = logger;
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
            
            mLogger?.LogTrace($"Slot at index {i} has running task");
        }

        if (callNumber == 1 || callNumber % 1000 == 0)
        {
            mLogger?.LogTrace("No available slot found");
            if (callNumber % 1000 == 0)
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
