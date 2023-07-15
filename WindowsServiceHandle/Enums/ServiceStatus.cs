using System.Runtime.InteropServices;

namespace WindowsServiceHandle.Enums;

[StructLayout(LayoutKind.Sequential)]
public class ServiceStatus
{
	internal int ServiceType { get; set; } = 0;
	internal ServiceStatuses CurrentStatus { get; set; } = (int)ServiceStatuses.NotFound;
	internal int ControlsAccepted { get; set; } = 0;
	internal int Win32ExitCode { get; set; } = 0;
	internal int ServiceSpecificExitCode { get; set; } = 0;
	internal int CheckPoint { get; set; } = 0;
	internal int WaitHint { get; set; } = 0;
}