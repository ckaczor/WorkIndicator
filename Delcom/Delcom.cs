using System;
using System.Runtime.InteropServices;
using Microsoft.Win32.SafeHandles;

namespace WorkIndicator.Delcom
{
    public class Delcom
    {
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct HidTxPacketStruct
        {
            public Byte MajorCmd;
            public Byte MinorCmd;
            public Byte LSBData;
            public Byte MSBData;
            public Byte HidData0;
            public Byte HidData1;
            public Byte HidData2;
            public Byte HidData3;
            public Byte ExtData0;
            public Byte ExtData1;
            public Byte ExtData2;
            public Byte ExtData3;
            public Byte ExtData4;
            public Byte ExtData5;
            public Byte ExtData6;
            public Byte ExtData7;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct HidRxPacketStruct
        {
            public Byte Data0;
            public Byte Data1;
            public Byte Data2;
            public Byte Data3;
            public Byte Data4;
            public Byte Data5;
            public Byte Data6;
            public Byte Data7;
        }

        private readonly DeviceManagement _deviceManagement = new DeviceManagement();
        private readonly HID _hid = new HID();

        private SafeFileHandle _deviceHandle;
        private Boolean _deviceDetected;
        private String _devicePathName;
        private UInt32 _matchingDevicesFound;
        private HidTxPacketStruct _transmitPacket;

        public bool RawCommand(int major, int minor, int lsbData, int msbData)
        {
            _transmitPacket.MajorCmd = Convert.ToByte(major);
            _transmitPacket.MinorCmd = Convert.ToByte(minor);

            _transmitPacket.LSBData = Convert.ToByte(lsbData);
            _transmitPacket.MSBData = Convert.ToByte(msbData);

            return HID.HidD_SetFeature(_deviceHandle, StructureToByteArray(_transmitPacket), 8);
        }

        public int WriteBlink(int lsb, int msb)
        {
            try
            {
                _transmitPacket.MajorCmd = 101;
                _transmitPacket.MinorCmd = 20;

                _transmitPacket.LSBData = Convert.ToByte(lsb);
                _transmitPacket.MSBData = Convert.ToByte(msb);

                int r = HID.HidD_SetFeature(_deviceHandle, StructureToByteArray(_transmitPacket), 8) ? 0 : 1;

                _transmitPacket.MajorCmd = 101;
                _transmitPacket.MinorCmd = 25;

                _transmitPacket.LSBData = Convert.ToByte(msb);
                _transmitPacket.MSBData = Convert.ToByte(0);

                HID.HidD_SetFeature(_deviceHandle, StructureToByteArray(_transmitPacket), 8);

                return r;
            }
            catch (Exception)
            {
                return 2;
            }           
        }

        public int WritePorts(int port0, int port1)
        {
            try
            {
                _transmitPacket.MajorCmd = 101;
                _transmitPacket.MinorCmd = 10;

                _transmitPacket.LSBData = Convert.ToByte(port0);
                _transmitPacket.MSBData = Convert.ToByte(port1);

                return HID.HidD_SetFeature(_deviceHandle, StructureToByteArray(_transmitPacket), 8) ? 0 : 1;
            }
            catch (Exception)
            {
                return 2;
            }
        }

        public UInt32 ReadPort0(ref UInt32 port0)
        {

            try
            {
                Byte[] buffer = new Byte[16];

                buffer[0] = 100;    // Read ports command

                if (!HID.HidD_GetFeature(_deviceHandle, buffer, 8))
                    return 1;

                port0 = Convert.ToUInt32(buffer[0]);

                return 0;
            }
            catch (Exception)
            {
                return 2;
            }
        }

        public int ReadPorts(ref int port0, ref int port1)
        {
            try
            {
                Byte[] buffer = new Byte[16];

                buffer[0] = 100;    // Read ports command

                if (!HID.HidD_GetFeature(_deviceHandle, buffer, 8))
                    return 1;

                port0 = Convert.ToInt32(buffer[0]);
                port1 = Convert.ToInt32(buffer[1]);

                return 0;
            }
            catch (Exception)
            {
                return 2;
            }
        }

        public UInt32 Close()
        {
            try
            {
                if (_deviceHandle != null && !_deviceHandle.IsClosed)
                    _deviceHandle.Close();

                return 0;
            }
            catch (Exception)
            {
                return 2;
            }
        }

        public UInt32 Open()
        {
            return OpenNthDevice(1);
        }

        public UInt32 OpenNthDevice(UInt32 deviceIndex)
        {
            if (_deviceHandle != null)
                Close();

            if (!FindTheHID(deviceIndex))
                return 1;

            _deviceHandle = FileIODeclarations.CreateFile(_devicePathName, FileIODeclarations.GenericRead | FileIODeclarations.GenericWrite, FileIODeclarations.FileShareRead | FileIODeclarations.FileShareWrite, IntPtr.Zero, FileIODeclarations.OpenExisting, 0, 0);

            return _deviceHandle.IsInvalid ? 2 : (uint) 0;
        }

        public string GetDeviceName()
        {
            return _devicePathName;
        }

        public UInt32 GetDevicesCount()
        {
            FindTheHID(0);

            return _matchingDevicesFound;
        }

        private Boolean FindTheHID(UInt32 deviceIndex)
        {
            const ushort productId = 0xB080;
            const ushort vendorId = 0x0FC5;

            String[] devicePathName = new String[128];
            Guid hidGuid = Guid.Empty;
            UInt32 matchingDevices = 0;

            _deviceDetected = false;

            HID.HidD_GetHidGuid(ref hidGuid);

            Boolean deviceFound = _deviceManagement.FindDeviceFromGuid(hidGuid, ref devicePathName);

            if (deviceFound)
            {
                Int32 memberIndex = 0;
                do
                {
                    SafeFileHandle hidHandle = FileIODeclarations.CreateFile(devicePathName[memberIndex], FileIODeclarations.GenericRead | FileIODeclarations.GenericWrite, FileIODeclarations.FileShareRead | FileIODeclarations.FileShareWrite, IntPtr.Zero, FileIODeclarations.OpenExisting, 0, 0);

                    if (!hidHandle.IsInvalid)
                    {
                        _hid.DeviceAttributes.Size = Marshal.SizeOf(_hid.DeviceAttributes);

                        Boolean success = HID.HidD_GetAttributes(hidHandle, ref _hid.DeviceAttributes);
                        if (success)
                        {
                            if (_hid.DeviceAttributes.VendorID == vendorId && _hid.DeviceAttributes.ProductID == productId)
                            {
                                matchingDevices++;
                                _deviceDetected = true;
                            }

                            if (_deviceDetected && matchingDevices == deviceIndex)
                            {
                                _devicePathName = devicePathName[memberIndex];
                                hidHandle.Close();
                            }
                            else
                            {
                                _deviceDetected = false;
                                hidHandle.Close();
                            }
                        }
                        else
                        {
                            _deviceDetected = false;
                            hidHandle.Close();
                        }
                    }

                    memberIndex = memberIndex + 1;
                }
                while (!(_deviceDetected || memberIndex == devicePathName.Length));
            }

            _matchingDevicesFound = matchingDevices;

            return _deviceDetected;
        }

        private static byte[] StructureToByteArray(object obj)
        {
            int length = Marshal.SizeOf(obj);
            byte[] buffer = new byte[length];
            IntPtr ptr = Marshal.AllocHGlobal(length);
            Marshal.StructureToPtr(obj, ptr, true);
            Marshal.Copy(ptr, buffer, 0, length);
            Marshal.FreeHGlobal(ptr);
            return buffer;
        }
    }
}
