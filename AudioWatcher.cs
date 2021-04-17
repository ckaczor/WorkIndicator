using CSCore.CoreAudioAPI;
using System;
using System.Diagnostics;
using System.Threading;

namespace WorkIndicator
{
    public static class AudioWatcher
    {
        public delegate void MicrophoneInUseChangedDelegate(bool microphoneInUse);

        private static ManualResetEvent _manualResetEvent;

        private static int _activeSessionCount;

        public static event MicrophoneInUseChangedDelegate MicrophoneInUseChanged;

        public static void Start()
        {
            _manualResetEvent = new ManualResetEvent(false);

            var thread = new Thread(delegate ()
            {
                var deviceEnumerator = new MMDeviceEnumerator();

                foreach (var device in deviceEnumerator.EnumAudioEndpoints(DataFlow.Capture, DeviceState.Active))
                {
                    var sessionManager = AudioSessionManager2.FromMMDevice(device);

                    var sessionEnumerator = sessionManager.GetSessionEnumerator();

                    sessionManager.SessionCreated += HandleAudioSessionCreated;

                    foreach (var audioSessionControl in sessionEnumerator)
                    {
                        HandleAudioStateChanged(audioSessionControl, new AudioSessionStateChangedEventArgs(audioSessionControl.SessionState));

                        audioSessionControl.StateChanged += HandleAudioStateChanged;
                    }
                }

                _manualResetEvent.WaitOne();
            });

            thread.SetApartmentState(ApartmentState.MTA);
            thread.Start();
        }

        public static void Stop()
        {
            _manualResetEvent?.Set();
        }

        private static void HandleAudioSessionCreated(object sender, SessionCreatedEventArgs e)
        {
            HandleAudioStateChanged(null, new AudioSessionStateChangedEventArgs(e.NewSession.SessionState));

            e.NewSession.StateChanged += HandleAudioStateChanged;
        }

        private static void HandleAudioStateChanged(object sender, AudioSessionStateChangedEventArgs e)
        {
            Debug.WriteLine($"{e.NewState}");

            switch (e.NewState)
            {
                case AudioSessionState.AudioSessionStateActive:
                    _activeSessionCount++;
                    break;
                case AudioSessionState.AudioSessionStateInactive:
                    _activeSessionCount--;
                    break;
                case AudioSessionState.AudioSessionStateExpired:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            if (_activeSessionCount < 0)
                _activeSessionCount = 0;

            Debug.WriteLine($"{_activeSessionCount}");

            MicrophoneInUseChanged?.Invoke(MicrophoneInUse());
        }

        public static bool MicrophoneInUse()
        {
            return _activeSessionCount > 0;
        }
    }
}
