using CSCore.CoreAudioAPI;
using Serilog;
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

        private static readonly ILogger Logger = Log.Logger;

        public static void Start()
        {
            Logger.Debug("AudioWatcher - Start");

            _manualResetEvent = new ManualResetEvent(false);

            _thread = new Thread(delegate ()
            {
                var deviceEnumerator = new MMDeviceEnumerator();

                foreach (var device in deviceEnumerator.EnumAudioEndpoints(DataFlow.Capture, DeviceState.Active))
                {
                    var sessionManager = AudioSessionManager2.FromMMDevice(device);

                    _sessionManagers.Add(sessionManager);

                    sessionManager.SessionCreated += (sessionSender, sessionCreatedEventArgs) => HandleDeviceSession(device, sessionCreatedEventArgs.NewSession);

                    foreach (var audioSessionControl in sessionManager.GetSessionEnumerator())
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

            Logger.Debug($"AudioWatcher - HandleDeviceSession - {device.FriendlyName}, {deviceId}");

            if (!ActiveSessions.ContainsKey(deviceId))
                ActiveSessions[deviceId] = 0;

            HandleAudioStateChanged(deviceId, audioSessionControl.SessionState);

            audioSessionControl.StateChanged += (sender, sessionStateChangedEventArgs) =>
                HandleAudioStateChanged(deviceId, sessionStateChangedEventArgs.NewState);
        }

        public static void Stop()
        {
            Logger.Debug("AudioWatcher - Stop");

            _sessionManagers.Clear();

            _manualResetEvent?.Set();
        }

        private static void HandleAudioStateChanged(string deviceId, AudioSessionState newState)
        {
            Logger.Debug($"AudioWatcher - HandleAudioStateChanged - {deviceId}, {newState}");

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
                    ActiveSessions[deviceId] = 0;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            Logger.Debug($"AudioWatcher - HandleAudioStateChanged - {deviceId} = {ActiveSessions[deviceId]}");

            MicrophoneInUseChanged?.Invoke(MicrophoneInUse());
        }

        public static bool MicrophoneInUse()
        {
            return ActiveSessions.Any(a => a.Value > 0);
        }
    }
}
