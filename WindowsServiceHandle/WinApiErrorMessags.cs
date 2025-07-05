using System.Runtime.InteropServices;
using System.Text;

namespace WindowsServiceHandler
{
    internal static class WinApiErrorMessags
    {
        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern int FormatMessage(int flags,
                    IntPtr source,
                    int messageId,
                    int languageId,
                    StringBuilder buffer,
                    int size,
                    IntPtr arguments);

        private const int FORMAT_MESSAGE_FROM_SYSTEM = 0x00001000;

        public static string GetErrorMessage(int errorCode)
        {
            StringBuilder messageBuffer = new(512);
            int size = FormatMessage(
                FORMAT_MESSAGE_FROM_SYSTEM,
                IntPtr.Zero,
                errorCode,
                0,
                messageBuffer,
                messageBuffer.Capacity,
                IntPtr.Zero);

            if (size == 0)
            {
                return $"Unknown error (code {errorCode})";
            }

            return messageBuffer.ToString().Trim();
        }
    }
}
