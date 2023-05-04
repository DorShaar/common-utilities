using Microsoft.Extensions.Logging;

namespace SmartTasks;

public class TasksRunner
{
    private readonly Task?[] mAllRunningTasks;
    private readonly TimeSpan mIntervalForCheckingAvailableSlot;
    private readonly TimeSpan mDefaultIntervalForCheckingAvailableSlot = TimeSpan.FromMilliseconds(500);
    private readonly ILogger<TasksRunner>? mLogger;
    
    public TasksRunner(ushort allowedParallelTasks, TimeSpan iIntervalForCheckingAvailableSlot, ILogger<TasksRunner>? logger = null)
    {
        ushort fixedAllowedParallelTasks = allowedParallelTasks == 0u ? (ushort)1 : allowedParallelTasks;
        mAllRunningTasks = new Task[fixedAllowedParallelTasks];

        mIntervalForCheckingAvailableSlot = iIntervalForCheckingAvailableSlot == TimeSpan.Zero
            ? mDefaultIntervalForCheckingAvailableSlot
            : iIntervalForCheckingAvailableSlot;  
        mIntervalForCheckingAvailableSlot = iIntervalForCheckingAvailableSlot;
        
        mLogger = logger;
    }

    /// <summary>
    /// Becomes blocking operation when maximal tasks allowed are running.
    /// </summary>
    public void RunTask(Task task, CancellationToken cancellationToken)
    {
        ushort? indexToPlaceTask = null;

        while (indexToPlaceTask is null)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                mLogger?.LogInformation($"Cancel requested");
                break;
            }
            
            indexToPlaceTask = TryFindIndexToLocateTask();
            Task.Delay(mIntervalForCheckingAvailableSlot, cancellationToken);
        }

        if (indexToPlaceTask is null)
        {
            return;
        }
        
        InsertTaskByIndex(indexToPlaceTask.Value, task);

        WaitOneIfRequired(cancellationToken);
    }

    public bool WaitAll(CancellationToken cancellationToken)
    {
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
                
                Task.Delay(mIntervalForCheckingAvailableSlot, cancellationToken);
            }
        }

        return true;
    }

    /// <summary>
    /// Waits for one slot to be available. 
    /// </summary>
    private void WaitOneIfRequired(CancellationToken cancellationToken)
    {
        ushort? indexToPlaceTask = null;
        while (indexToPlaceTask is null)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                mLogger?.LogInformation($"Cancel requested");
                break;
            }
            
            indexToPlaceTask = TryFindIndexToLocateTask();
            Task.Delay(mIntervalForCheckingAvailableSlot, cancellationToken);
        }

        if (!cancellationToken.IsCancellationRequested)
        {
            mLogger?.LogInformation($"At least one task slot is available");   
        }
    }

    private ushort? TryFindIndexToLocateTask()
    {
        for (ushort i = 0; i < mAllRunningTasks.Length; ++i)
        {
            Task? task = mAllRunningTasks[i];
            if (task is null || task.IsCompleted)
            {
                mLogger?.LogDebug($"Found available slot at index {i}");
                return i;
            }
        }

        mLogger?.LogDebug("No available slot found");
        return null;
    }

    private void InsertTaskByIndex(ushort indexToPlaceTask, Task task)
    {
        Task? oldTask = mAllRunningTasks[indexToPlaceTask];
        oldTask?.Dispose();

        mAllRunningTasks[indexToPlaceTask] = task;
    }
}
