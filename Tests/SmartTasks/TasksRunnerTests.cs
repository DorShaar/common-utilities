using Microsoft.Extensions.Logging.Abstractions;
using SmartTasks;
using TimeTracers;
using Xunit;

namespace Tests.SmartTasks;

public class TasksRunnerTests
{
    [Fact]
    public async Task RunTask_TaskCountIsMaximalAllowed_Blocking()
    {
        TasksRunner tasksRunner = new(1, TimeSpan.FromSeconds(1), NullLogger<TasksRunner>.Instance);
        TimeTracer timeTracer;
        
        TimeSpan taskDuration = TimeSpan.FromSeconds(5);
        using (timeTracer = new TimeTracer())
        {
            Task task = CreateTaskToBeFinishedIn(taskDuration);
            await tasksRunner.RunTask(task, CancellationToken.None);    
        }
        
        Assert.True(timeTracer.TimeTrace.Elapsed.TotalSeconds > 5);
    }
    
    [Fact]
    public async Task RunTask_TaskCountIsNotMaximalAllowed_NotBlocking()
    {
        TasksRunner tasksRunner = new(3, TimeSpan.FromSeconds(1), NullLogger<TasksRunner>.Instance);
        TimeTracer timeTracer;

        TimeSpan taskDuration = TimeSpan.FromSeconds(5);
        Task task;
        using (timeTracer = new TimeTracer())
        {
            task = CreateTaskToBeFinishedIn(taskDuration);
            await tasksRunner.RunTask(task, CancellationToken.None);    
        }

        Assert.True(timeTracer.TimeTrace.Elapsed.TotalSeconds < 4, "Waited less than 4 seconds");
        Assert.False(task.IsCompleted);

        await Task.Delay(taskDuration).ConfigureAwait(false);
        Assert.True(task.IsCompleted);
    }

    [Fact]
    public async Task WaitAll_WaitingForAllTasks_WaitForAllTasksToBeCompleted()
    {
        TasksRunner tasksRunner = new(3, TimeSpan.FromSeconds(1), NullLogger<TasksRunner>.Instance);
        
        Task task1 = CreateTaskToBeFinishedIn(TimeSpan.FromSeconds(5));
        Task task2 = CreateTaskToBeFinishedIn(TimeSpan.FromSeconds(1));
        Task task3 = CreateTaskToBeFinishedIn(TimeSpan.FromSeconds(1));

        TimeTracer waitForAllTasksTimeTracer;
        using (waitForAllTasksTimeTracer = new TimeTracer())
        {
            TimeTracer startAllTasksTimeTracer;
            using (startAllTasksTimeTracer = new TimeTracer())
            {
                await tasksRunner.RunTask(task1, CancellationToken.None);    
                await tasksRunner.RunTask(task2, CancellationToken.None);
            
                // This should be block for about 1 second.
                await tasksRunner.RunTask(task3, CancellationToken.None);
            }
        
            Assert.True(startAllTasksTimeTracer.TimeTrace.Elapsed.TotalSeconds < 2, "Waited less than 2 seconds");

            await tasksRunner.WaitAll(CancellationToken.None);
        }
        
        Assert.True(waitForAllTasksTimeTracer.TimeTrace.Elapsed.TotalSeconds > 5, "Waited less than 5 seconds");
    }

    private Task CreateTaskToBeFinishedIn(TimeSpan timeSpan)
    {
        return Task.Delay(timeSpan);
    }
}