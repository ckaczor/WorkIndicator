using System;
using System.Runtime.InteropServices;
using Microsoft.Win32.SafeHandles;

namespace WorkIndicator.Delcom
{
    internal sealed partial class HID
    {
        //  API declarations for HID communications.

        //  from hidpi.h
        //  Typedef enum defines a set of integer constants for HidP_Report_Type

        internal const Int16 HidPInput = 0;
        internal const Int16 HidPOutput = 1;
        internal const Int16 HidPFeature = 2;

        [StructLayout(LayoutKind.Sequential)]
        internal struct HiddAttributes
        {
            internal Int32 Size;
            internal UInt16 VendorID;
            internal UInt16 ProductID;
            internal UInt16 VersionNumber;
        }

        internal struct HidpCaps
        {
            internal Int16 Usage;
            internal Int16 UsagePage;
            internal Int16 InputReportByteLength;
            internal Int16 OutputReportByteLength;
            internal Int16 FeatureReportByteLength;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 17)]
            internal Int16[] Reserved;
            internal Int16 NumberLinkCollectionNodes;
            internal Int16 NumberInputButtonCaps;
            internal Int16 NumberInputValueCaps;
            internal Int16 NumberInputDataIndices;
            internal Int16 NumberOutputButtonCaps;
            internal Int16 NumberOutputValueCaps;
            internal Int16 NumberOutputDataIndices;
            internal Int16 NumberFeatureButtonCaps;
            internal Int16 NumberFeatureValueCaps;
            internal Int16 NumberFeatureDataIndices;
        }

        [DllImport("hid.dll", SetLastError = true)]
        internal static extern Boolean HidD_FlushQueue(SafeFileHandle hidDeviceObject);

        [DllImport("hid.dll", SetLastError = true)]
        internal static extern Boolean HidD_FreePreparsedData(IntPtr preparsedData);

        [DllImport("hid.dll", SetLastError = true)]
        internal static extern Boolean HidD_GetAttributes(SafeFileHandle hidDeviceObject, ref HiddAttributes hiddAttributes);

        [DllImport("hid.dll", SetLastError = true)]
        internal static extern Boolean HidD_GetFeature(SafeFileHandle hidDeviceObject, Byte[] lpReportBuffer, Int32 reportBufferLength);

        [DllImport("hid.dll", SetLastError = true)]
        internal static extern Boolean HidD_GetInputReport(SafeFileHandle hidDeviceObject, Byte[] lpReportBuffer, Int32 reportBufferLength);

        [DllImport("hid.dll", SetLastError = true)]
        internal static extern void HidD_GetHidGuid(ref Guid hidGuid);

        [DllImport("hid.dll", SetLastError = true)]
        internal static extern Boolean HidD_GetNumInputBuffers(SafeFileHandle hidDeviceObject, ref Int32 numberBuffers);

        [DllImport("hid.dll", SetLastError = true)]
        internal static extern Boolean HidD_GetPreparsedData(SafeFileHandle hidDeviceObject, ref IntPtr preparsedData);

        [DllImport("hid.dll", SetLastError = true)]
        internal static extern Boolean HidD_SetFeature(SafeFileHandle hidDeviceObject, Byte[] lpReportBuffer, Int32 reportBufferLength);

        [DllImport("hid.dll", SetLastError = true)]
        internal static extern Boolean HidD_SetNumInputBuffers(SafeFileHandle hidDeviceObject, Int32 numberBuffers);

        [DllImport("hid.dll", SetLastError = true)]
        internal static extern Boolean HidD_SetOutputReport(SafeFileHandle hidDeviceObject, Byte[] lpReportBuffer, Int32 reportBufferLength);

        [DllImport("hid.dll", SetLastError = true)]
        internal static extern Int32 HidP_GetCaps(IntPtr preparsedData, ref HidpCaps capabilities);

        [DllImport("hid.dll", SetLastError = true)]
        internal static extern Int32 HidP_GetValueCaps(Int32 reportType, Byte[] valueCaps, ref Int32 valueCapsLength, IntPtr preparsedData);
    }
}
