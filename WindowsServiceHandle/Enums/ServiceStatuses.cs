namespace WindowsServiceHandle.Enums;

public enum ServiceStatuses
{
	Unknown = -1,
	NotFound = 0,
	Stopped = 1,
	StartPending = 2,
	StopPending = 3,
	Running = 4,
	ContinuePending = 5,
	PausePending = 6,
	Paused = 7
}
