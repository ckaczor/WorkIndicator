using System;
using System.Windows.Forms;

namespace WorkIndicator
{
    internal class UsbService : NativeWindow, IDisposable
    {
        public event Action DevicesChanged;

        private const int WM_DEVICECHANGE = 0x0219;
        private const int DBT_DEVICEARRIVAL = 0x8000;
        private const int DBT_DEVICEREMOVECOMPLETE = 0x8004;
        private const int DBT_DEVNODES_CHANGED = 0x0007;

        private bool _isDisposed;

        internal UsbService()
        {
            base.CreateHandle(new CreateParams());
        }


        protected override void WndProc(ref Message msg)
        {
            base.WndProc(ref msg);

            if (msg.Msg == WM_DEVICECHANGE)
            {
                switch (msg.WParam.ToInt32())
                {
                    case DBT_DEVNODES_CHANGED:
                    case DBT_DEVICEARRIVAL:
                    case DBT_DEVICEREMOVECOMPLETE:
                        DevicesChanged?.Invoke();
                        break;
                }
            }
        }

        public void Dispose()
        {
            if (!_isDisposed)
            {
                base.DestroyHandle();

                _isDisposed = true;

                GC.SuppressFinalize(this);
            }
        }
    }
}
