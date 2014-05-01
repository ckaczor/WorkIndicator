using System;
using System.Runtime.InteropServices;

namespace WorkIndicator.Delcom
{
    internal sealed partial class DeviceManagement
    {
        ///<summary >
        // API declarations relating to device management (SetupDixxx and 
        // RegisterDeviceNotification functions).   
        /// </summary>

        // from dbt.h

        internal const Int32 DbtDeviceArrival = 0x8000;
        internal const Int32 DbtDeviceRemoveComplete = 0x8004;
        internal const Int32 DbtDevTypDeviceInterface = 5;
        internal const Int32 DbtDevTypHandle = 6;
        internal const Int32 DeviceNotifyAllInterfaceClasses = 4;
        internal const Int32 DeviceNotifyServiceHandle = 1;
        internal const Int32 DeviceNotifyWindowHandle = 0;
        internal const Int32 WmDeviceChange = 0x219;

        // from setupapi.h

        internal const Int32 DigCfPresent = 2;
        internal const Int32 DigCfDeviceInterface = 0X10;

        // Two declarations for the DEV_BROADCAST_DEVICEINTERFACE structure.

        // Use this one in the call to RegisterDeviceNotification() and
        // in checking dbch_devicetype in a DEV_BROADCAST_HDR structure:

        [StructLayout(LayoutKind.Sequential)]
        internal class DevBroadcastDeviceInterface
        {
            internal Int32 dbcc_size;
            internal Int32 dbcc_devicetype;
            internal Int32 dbcc_reserved;
            internal Guid dbcc_classguid;
            internal Int16 dbcc_name;
        }

        // Use this to read the dbcc_name String and classguid:

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        internal class DevBroadcastDeviceInterface1
        {
            internal Int32 dbcc_size;
            internal Int32 dbcc_devicetype;
            internal Int32 dbcc_reserved;
            [MarshalAs(UnmanagedType.ByValArray, ArraySubType = UnmanagedType.U1, SizeConst = 16)]
            internal Byte[] dbcc_classguid;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 255)]
            internal Char[] dbcc_name;
        }

        [StructLayout(LayoutKind.Sequential)]
        internal class DevBroadcastHdr
        {
            internal Int32 dbch_size;
            internal Int32 dbch_devicetype;
            internal Int32 dbch_reserved;
        }

        internal struct SpDeviceInterfaceData
        {
            internal Int32 CbSize;
            internal Guid InterfaceClassGuid;
            internal Int32 Flags;
            internal IntPtr Reserved;
        }

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        internal static extern IntPtr RegisterDeviceNotification(IntPtr hRecipient, IntPtr notificationFilter, Int32 flags);

        [DllImport("setupapi.dll", SetLastError = true)]
        internal static extern Int32 SetupDiCreateDeviceInfoList(ref Guid classGuid, Int32 hwndParent);

        [DllImport("setupapi.dll", SetLastError = true)]
        internal static extern Int32 SetupDiDestroyDeviceInfoList(IntPtr deviceInfoSet);

        [DllImport("setupapi.dll", SetLastError = true)]
        internal static extern Boolean SetupDiEnumDeviceInterfaces(IntPtr deviceInfoSet, IntPtr deviceInfoData, ref Guid interfaceClassGuid, Int32 memberIndex, ref SpDeviceInterfaceData spDeviceInterfaceData);

        [DllImport("setupapi.dll", SetLastError = true, CharSet = CharSet.Auto)]
        internal static extern IntPtr SetupDiGetClassDevs(ref Guid classGuid, IntPtr enumerator, IntPtr hwndParent, Int32 flags);

        [DllImport("setupapi.dll", SetLastError = true, CharSet = CharSet.Auto)]
        internal static extern Boolean SetupDiGetDeviceInterfaceDetail(IntPtr deviceInfoSet, ref SpDeviceInterfaceData spDeviceInterfaceData, IntPtr deviceInterfaceDetailData, Int32 deviceInterfaceDetailDataSize, ref Int32 requiredSize, IntPtr deviceInfoData);

        [DllImport("user32.dll", SetLastError = true)]
        internal static extern Boolean UnregisterDeviceNotification(IntPtr handle);
    }
}
