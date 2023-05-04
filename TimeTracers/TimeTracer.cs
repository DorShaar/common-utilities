using System.Diagnostics;

namespace TimeTracers;

public class TimeTracer : IDisposable
{
    private string? mOperationName;
    
    public Stopwatch TimeTrace { get; }
    
    public TimeTracer(string? operationName = null)
    {
        mOperationName = operationName;
        TimeTrace = Stopwatch.StartNew();
    }

    public void Dispose()
    {
        TimeTrace.Stop();
    }
}