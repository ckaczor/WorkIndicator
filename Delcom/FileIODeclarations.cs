using System;
using System.Runtime.InteropServices;
using Microsoft.Win32.SafeHandles;

namespace WorkIndicator.Delcom
{
    internal sealed class FileIODeclarations
    {
        internal const Int32 FileFlagOverlapped = 0x40000000;

        internal const Int32 FileShareRead = 1;
        internal const Int32 FileShareWrite = 2;

        internal const UInt32 GenericRead = 0x80000000;
        internal const UInt32 GenericWrite = 0x40000000;

        internal const Int32 InvalidHandleValue = -1;

        internal const Int32 OpenExisting = 3;

        internal const Int32 WaitTimeout = 0x102;
        internal const Int32 WaitObject0 = 0;

        [StructLayout(LayoutKind.Sequential)]
        internal class SecurityAttributes
        {
            internal Int32 nLength;
            internal Int32 lpSecurityDescriptor;
            internal Int32 bInheritHandle;
        }

        [DllImport("kernel32.dll", SetLastError = true)]
        internal static extern Int32 CancelIo(SafeFileHandle hFile);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        internal static extern IntPtr CreateEvent(IntPtr securityAttributes, Boolean bManualReset, Boolean bInitialState, String lpName);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        internal static extern SafeFileHandle CreateFile(String lpFileName, UInt32 dwDesiredAccess, Int32 dwShareMode, IntPtr lpSecurityAttributes, Int32 dwCreationDisposition, Int32 dwFlagsAndAttributes, Int32 hTemplateFile);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        internal static extern Boolean GetOverlappedResult(SafeFileHandle hFile, IntPtr lpOverlapped, ref Int32 lpNumberOfBytesTransferred, Boolean bWait);

        [DllImport("kernel32.dll", SetLastError = true)]
        internal static extern Boolean ReadFile(SafeFileHandle hFile, IntPtr lpBuffer, Int32 nNumberOfBytesToRead, ref Int32 lpNumberOfBytesRead, IntPtr lpOverlapped);

        [DllImport("kernel32.dll", SetLastError = true)]
        internal static extern Int32 WaitForSingleObject(IntPtr hHandle, Int32 dwMilliseconds);

        [DllImport("kernel32.dll", SetLastError = true)]
        internal static extern Boolean WriteFile(SafeFileHandle hFile, Byte[] lpBuffer, Int32 nNumberOfBytesToWrite, ref Int32 lpNumberOfBytesWritten, IntPtr lpOverlapped);
    }
}
