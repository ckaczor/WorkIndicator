using System;
using WorkIndicator.Delcom;

namespace WorkIndicator
{
    public enum Status
    {
        Auto,
        Free,
        Working,
        OnPhone,
        Talking
    }

    public static class LightController
    {
        private static StoplightIndicator _stoplightIndicator;
        private static bool _initialized;
        private static Status _status = Status.Auto;

        public static void Initialize()
        {
            _stoplightIndicator = new StoplightIndicator();
            _stoplightIndicator.SetLight(StoplightIndicator.Light.Yellow, StoplightIndicator.LightState.On);

            AudioWatcher.MicrophoneInUseChanged += AudioWatcher_MicrophoneInUseChanged;
            AudioWatcher.Start();

            _initialized = true;
        }

        private static void AudioWatcher_MicrophoneInUseChanged(bool microphoneInUse)
        {
            UpdateLights();
        }

        public static void Dispose()
        {
            if (!_initialized)
                return;

            AudioWatcher.Stop();

            _stoplightIndicator.SetLights(StoplightIndicator.LightState.Off, StoplightIndicator.LightState.Off, StoplightIndicator.LightState.Off);
            _stoplightIndicator.Dispose();

            _initialized = false;
        }

        public static Status Status
        {
            get => _status;
            set
            {
                _status = value;
                UpdateLights();
            }
        }

        private static void UpdateLights()
        {
            var red = StoplightIndicator.LightState.Off;
            var yellow = StoplightIndicator.LightState.Off;
            var green = StoplightIndicator.LightState.Off;

            if (_status == Status.Auto)
            {
                if (AudioWatcher.MicrophoneInUse())
                {
                    red = StoplightIndicator.LightState.On;
                }
                else
                {
                    yellow = StoplightIndicator.LightState.On;
                }
            }
            else
            {
                switch (_status)
                {
                    case Status.Free:
                        green = StoplightIndicator.LightState.On;
                        break;
                    case Status.Working:
                        yellow = StoplightIndicator.LightState.On;
                        break;
                    case Status.OnPhone:
                        red = StoplightIndicator.LightState.On;
                        break;
                    case Status.Talking:
                        red = StoplightIndicator.LightState.Blink;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            _stoplightIndicator.SetLights(red, yellow, green);
        }
    }
}
