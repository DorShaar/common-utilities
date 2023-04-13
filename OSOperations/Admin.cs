using System.Runtime.InteropServices;
using System.Security.Principal;

namespace OSOperations;

public class Admin
{
    public static bool IsRunningAsAdministrator()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
#pragma warning disable CA1416
            WindowsIdentity windowsIdentity = WindowsIdentity.GetCurrent();
            WindowsPrincipal windowsPrincipal = new(windowsIdentity);
            return windowsPrincipal.IsInRole(WindowsBuiltInRole.Administrator);
#pragma warning restore CA1416                
        }

        return true;
    }
}