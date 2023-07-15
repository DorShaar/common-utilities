using System.ComponentModel;
using System.Runtime.InteropServices;
using Microsoft.Extensions.Logging;
using WindowsServiceHandle.Enums;

namespace WindowsServiceHandle;

public class WindowsServiceManager
{
	#region Dll Imports
	[DllImport("advapi32.dll", SetLastError = true)]
	private static extern IntPtr OpenSCManager(string? machineName, string? databaseName, int accessRights);

	[DllImport("advapi32.dll", SetLastError = true)]
	private static extern IntPtr CreateService(IntPtr SC_HANDLE,
											   string lpSvcName,
											   string lpDisplayName,
											   int dwDesiredAccess,
											   int dwServiceType,
											   int dwStartType,
											   int dwErrorControl,
											   string lpPathName,
											   string? lpLoadOrderGroup,
											   IntPtr lpdwTagId, // Must be byValue and not by ref (according to https://www.pinvoke.net/default.aspx/advapi32.createservice)
											   string? lpDependencies,
											   string? lpServiceStartName,
											   string? lpPassword);

	[DllImport("advapi32.dll", SetLastError = true)]
	private static extern int StartService(IntPtr SVHANDLE, int dwNumServiceArgs, string[]? lpServiceArgVectors);
	
	[DllImport("advapi32.dll", SetLastError = true)]
	private static extern IntPtr OpenService(IntPtr hSCManager, string lpServiceName, int dwDesiredAccess);
	
	[DllImport("advapi32.dll", SetLastError = true)]
	private static extern int ControlService(IntPtr SVHANDLE, int dwControl, ServiceStatus lpServiceStatus);
	
	[DllImport("advapi32.dll", SetLastError = true)]
	private static extern int QueryServiceStatus(IntPtr SVHANDLE, ServiceStatus lpServiceStatus);
	#endregion Dll Imports

	private readonly ILogger<WindowsServiceManager> mLogger;

	public WindowsServiceManager(ILogger<WindowsServiceManager> logger)
	{
		mLogger = logger ?? throw new ArgumentNullException($"{nameof(logger)} is null");
	}
	
	public (bool, WindowsServiceHandle?) IsServiceExists(string serviceName)
	{
		const int serviceDoesNotExistsWin32ErrorCode = 1060;
	
		bool isServiceExists;

		WindowsServiceHandle? serviceHandle = null;
		try
		{
			using WindowsServiceHandle serviceControlManagerHandle = OpenServiceControlManager(WindowsServiceManagerAccessRights.Connect | WindowsServiceManagerAccessRights.CreateService);
			serviceHandle = openService(serviceControlManagerHandle, serviceName, WindowsServiceAccessRights.QueryStatus | WindowsServiceAccessRights.Start);
			isServiceExists = true;
			mLogger.LogInformation($"Service {serviceName} already exists");
		}
		catch (Win32Exception ex)
		{
			if (ex.NativeErrorCode != serviceDoesNotExistsWin32ErrorCode)
			{
				mLogger.LogInformation(ex, $"Failed to check if service {serviceName} exists. Error code: {ex.NativeErrorCode}");
				throw;
			}

			mLogger.LogInformation($"Service {serviceName} does not exist");
			isServiceExists = false;
		}

		return (isServiceExists, serviceHandle);
	}
	
	public bool StartService(string serviceName)
	{
		try
		{
			using WindowsServiceHandle serviceControlManagerHandle = OpenServiceControlManager(WindowsServiceManagerAccessRights.Connect);
			using WindowsServiceHandle serviceHandle = openService(serviceControlManagerHandle, serviceName, WindowsServiceAccessRights.Start);
			return StartService(serviceHandle, serviceName);
		}
		catch (Win32Exception ex)
		{
			mLogger.LogError(ex, $"Failed to start service '{serviceName}', error code {ex.NativeErrorCode}");
			return false;
		}
	}

	public bool StartService(WindowsServiceHandle serviceHandle, string serviceName)
	{
		const int serviceAlreadyRunningError = 1056;
		int startResult = StartService(serviceHandle.Handle,0,null);
		if (startResult == 0)
		{
			int errorCode = Marshal.GetLastWin32Error();
			if (errorCode == serviceAlreadyRunningError)
			{
				mLogger.LogInformation($"While trying to start, service {serviceName} already running");
				return true;
			}
			
			mLogger.LogError($"Failed to start service {serviceName}. Error code: {errorCode}");
			return false;
		}

		mLogger.LogInformation($"Service {serviceName} started");
		return true;
	}
	
	public bool StopService(string serviceName)
	{
		using WindowsServiceHandle serviceControlManagerHandle = OpenServiceControlManager(WindowsServiceManagerAccessRights.Connect);
		const int serviceNotActiveError = 1062;
		const int serviceNotExistError = 1060;
		
		try
		{
			using WindowsServiceHandle serviceHandle = openService(serviceControlManagerHandle, serviceName, WindowsServiceAccessRights.Stop);
		
			int stopResult = ControlService(serviceHandle.Handle,(int)WindowsServiceControlOptions.Stop, new ServiceStatus());
			if (stopResult == 0)
			{
				int errorCode = Marshal.GetLastWin32Error();
				if (errorCode == serviceNotActiveError)
				{
					mLogger.LogInformation($"While trying to stop, service {serviceName} not active");
					return true;
				}
			
				if (errorCode == serviceNotExistError)
				{
					mLogger.LogInformation($"While trying to stop, service {serviceName} not exist");
					return true;
				}
			
				mLogger.LogError($"Failed to stop service {serviceName}. Error code: {errorCode}");
				return false;
			}

			mLogger.LogInformation($"Service {serviceName} stopped");
			return true;
		}
		catch (Win32Exception ex)
		{
			if (ex.NativeErrorCode == serviceNotActiveError)
			{
				mLogger.LogInformation($"While trying to stop, service {serviceName} not active");
				return true;
			}
		
			if (ex.NativeErrorCode == serviceNotExistError)
			{
				mLogger.LogInformation($"While trying to stop, service {serviceName} not exist");
				return true;
			}

			mLogger.LogError($"While trying to stop service {serviceName}, got exception with error code {ex.ErrorCode}");
			return false;
		}
	}

	public WindowsServiceHandle? TryCreateService(string serviceName, string serviceExePath)
	{
		const int serviceWin32OwnProcess = 0x00000010;
		
		try
		{
			using WindowsServiceHandle serviceControlManagerHandle = OpenServiceControlManager(WindowsServiceManagerAccessRights.CreateService);
			IntPtr servicePtr = CreateService(serviceControlManagerHandle.Handle,
											  serviceName,
											  serviceName,
											  (int)(WindowsServiceAccessRights.Start | WindowsServiceAccessRights.Stop | WindowsServiceAccessRights.StandardRightsRequired),
											  serviceWin32OwnProcess,
											  (int)WindowsServiceStartOptions.DemandStart,
											  (int)WindowsServiceErrorOptions.Normal,
											  $"\"{serviceExePath}\"",
											  null,
											  IntPtr.Zero,
											  null,
											  null,
											  null);
			WindowsServiceHandle serviceHandle = new(servicePtr);
			mLogger.LogInformation($"Service {serviceName} created from path '{serviceExePath}'");
			return serviceHandle;
		}
		catch (Exception ex)
		{
			mLogger.LogError(ex, $"Could not create service {serviceName} from path '{serviceExePath}'");
			return null;
		}
	}

	public ServiceStatuses GetServiceStatus(string serviceName)
	{
		using WindowsServiceHandle serviceControlManagerHandle = OpenServiceControlManager(WindowsServiceManagerAccessRights.Connect);
		using WindowsServiceHandle serviceHandle = openService(serviceControlManagerHandle, serviceName, WindowsServiceAccessRights.QueryStatus);
		
		ServiceStatus serviceStatus = new();
		if (QueryServiceStatus(serviceHandle.Handle, serviceStatus) == 0)
		{
			mLogger.LogError($"Could not get service status for '{serviceName}', error code {Marshal.GetLastWin32Error()}");
			return ServiceStatuses.Unknown;
		}

		return serviceStatus.CurrentStatus;
	}

	private static WindowsServiceHandle OpenServiceControlManager(WindowsServiceManagerAccessRights accessRights)
	{
		IntPtr serviceControlManagerPtr = OpenSCManager(machineName: null, databaseName: null, (int)accessRights);
		return new WindowsServiceHandle(serviceControlManagerPtr);
	}
	
	private static WindowsServiceHandle openService(WindowsServiceHandle serviceManagerHandle, string serviceName, WindowsServiceAccessRights accessRights)
	{
		IntPtr servicePtr = OpenService(serviceManagerHandle.Handle, serviceName, (int)accessRights);
		return new WindowsServiceHandle(servicePtr);
	}
}