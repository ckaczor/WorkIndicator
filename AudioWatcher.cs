using CSCore.CoreAudioAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace WorkIndicator
{
    public static class AudioWatcher
    {
        public delegate void MicrophoneInUseChangedDelegate(bool microphoneInUse);

        private static ManualResetEvent _manualResetEvent;

        private static readonly Dictionary<string, int> ActiveSessions = new Dictionary<string, int>();

        public static event MicrophoneInUseChangedDelegate MicrophoneInUseChanged;

        private static Thread _thread;

        private static readonly List<AudioSessionManager2> _sessionManagers = new List<AudioSessionManager2>();

        public static void Start()
        {
            _manualResetEvent = new ManualResetEvent(false);

            _thread = new Thread(delegate ()
            {
                var deviceEnumerator = new MMDeviceEnumerator();

                foreach (var device in deviceEnumerator.EnumAudioEndpoints(DataFlow.Capture, DeviceState.Active))
                {
                    var sessionManager = AudioSessionManager2.FromMMDevice(device);

                    _sessionManagers.Add(sessionManager);

                    var sessionEnumerator = sessionManager.GetSessionEnumerator();

                    sessionManager.SessionCreated += (sessionSender, sessionCreatedEventArgs) => HandleDeviceSession(device, sessionCreatedEventArgs.NewSession);

                    foreach (var audioSessionControl in sessionEnumerator)
                    {
                        HandleDeviceSession(device, audioSessionControl);
                    }
                }

                _manualResetEvent.WaitOne();
            });

            _thread.SetApartmentState(ApartmentState.MTA);
            _thread.Start();
        }

        private static void HandleDeviceSession(MMDevice device, AudioSessionControl audioSessionControl)
        {
            var deviceId = device.DeviceID + audioSessionControl.GroupingParam;

            if (!ActiveSessions.ContainsKey(deviceId))
                ActiveSessions[deviceId] = 0;

            HandleAudioStateChanged(deviceId, audioSessionControl.SessionState);

            audioSessionControl.StateChanged += (sender, sessionStateChangedEventArgs) =>
                HandleAudioStateChanged(deviceId, sessionStateChangedEventArgs.NewState);
        }

        public static void Stop()
        {
            _sessionManagers.Clear();

            _manualResetEvent?.Set();
        }

        private static void HandleAudioStateChanged(string deviceId, AudioSessionState newState)
        {
            switch (newState)
            {
                case AudioSessionState.AudioSessionStateActive:
                    ActiveSessions[deviceId]++;
                    break;
                case AudioSessionState.AudioSessionStateInactive:
                    if (ActiveSessions[deviceId] > 0)
                        ActiveSessions[deviceId]--;
                    break;
                case AudioSessionState.AudioSessionStateExpired:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            MicrophoneInUseChanged?.Invoke(MicrophoneInUse());
        }

        public static bool MicrophoneInUse()
        {
            return ActiveSessions.Any(a => a.Value > 0);
        }
    }
}
