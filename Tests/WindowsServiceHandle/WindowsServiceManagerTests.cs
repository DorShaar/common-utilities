using Microsoft.Extensions.Logging.Abstractions;
using WindowsServiceHandle;
using WindowsServiceHandle.Enums;
using Xunit;

namespace Tests.WindowsServiceHandle;

public class WindowsServiceManagerTests
{
	[Fact]
	public void StopService_ServiceNotExists_ReturnTrue()
	{
		WindowsServiceManager windowsServiceManager = new(NullLogger<WindowsServiceManager>.Instance);

		Assert.True(windowsServiceManager.StopService("non existing service"));
	}
	
	[Fact(Skip="Requires real service")]
	public void StopService_ServiceNotRunning_ReturnTrue()
	{
		const string serviceName = "Dor Backuper Service";
		WindowsServiceManager windowsServiceManager = new(NullLogger<WindowsServiceManager>.Instance);

		Assert.True(windowsServiceManager.StopService(serviceName));
	}
	
	// [Fact(Skip="Requires real service")]
	[Fact]
	public async Task StopService_ServiceRunning_ReturnTrue()
	{
		const string serviceName = "Dor Backuper Service";
		WindowsServiceManager windowsServiceManager = new(NullLogger<WindowsServiceManager>.Instance);

		ServiceStatuses currentStatus = windowsServiceManager.GetServiceStatus(serviceName);

		if (currentStatus != ServiceStatuses.Running)
		{
			Assert.True(windowsServiceManager.StartService(serviceName));
			await Task.Delay(TimeSpan.FromSeconds(2)).ConfigureAwait(false);
		}
		
		Assert.True(windowsServiceManager.StopService(serviceName));
	}
}