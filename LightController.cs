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
        private static UsbService _usbService;
        private static bool _initialized;
        private static Status _status = Status.Auto;

        public static void Initialize()
        {
            _usbService = new UsbService();
            _usbService.DevicesChanged += DevicesChanged;

            _stoplightIndicator = new StoplightIndicator();
            _stoplightIndicator.SetLight(StoplightIndicator.Light.Yellow, StoplightIndicator.LightState.On);

            Properties.Settings.Default.PropertyChanged += HandleSettingChange;

            AudioWatcher.MicrophoneInUseChanged += AudioWatcher_MicrophoneInUseChanged;
            AudioWatcher.Start();

            _initialized = true;
        }

        private static void HandleSettingChange(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(Properties.Settings.Default.DefaultStatus))
                UpdateLights();
        }

        private static void DevicesChanged()
        {
            _stoplightIndicator?.Dispose();
            _stoplightIndicator = new StoplightIndicator();

            UpdateLights();
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

            _usbService.Dispose();

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

            var status = _status;

            if (status == Status.Auto)
            {
                if (AudioWatcher.MicrophoneInUse())
                {
                    status = Status.OnPhone;
                }
                else
                {
                    status = (Status) Enum.Parse(typeof(Status), Properties.Settings.Default.DefaultStatus);
                }
            }

            switch (status)
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

            _stoplightIndicator.SetLights(red, yellow, green);
        }
    }
}
