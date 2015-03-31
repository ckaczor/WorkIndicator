using System;

using Common.Extensions;

namespace WorkIndicator.Delcom
{
    public class StoplightIndicator : IDisposable
    {
        public enum Light
        {
            Green,
            Yellow,
            Red
        }

        public enum LightState
        {
            Off,
            On,
            Blink
        }

        private readonly Delcom _delcom;

        private LightState _green = LightState.Off;
        private LightState _yellow = LightState.Off;
        private LightState _red = LightState.Off;

        public StoplightIndicator()
        {
            _delcom = new Delcom();
            _delcom.Open();

            SetLights(_red, _yellow, _green);
        }

        public void Dispose()
        {
            _delcom.Close();
        }

        public Delcom Device
        {
            get { return _delcom; }
        }

        public void GetLights(out LightState red, out LightState yellow, out LightState green)
        {
            red = _red;
            yellow = _yellow;
            green = _green;
        }

        public LightState GetLight(Light light)
        {
            switch (light)
            {
                case Light.Red:
                    return _red;

                case Light.Yellow:
                    return _yellow;

                case Light.Green:
                    return _green;

                default:
                    throw new ArgumentOutOfRangeException("light");
            }
        }

        public void SetLights(LightState red, LightState yellow, LightState green)
        {
            if (_red == red && _yellow == yellow && _green == green)
                return;

            int port1 = 0;

            _red = red;
            _yellow = yellow;
            _green = green;

            port1 = port1.SetBitValue((int) Light.Green, green == LightState.Off);
            port1 = port1.SetBitValue((int) Light.Yellow, yellow == LightState.Off);
            port1 = port1.SetBitValue((int) Light.Red, red == LightState.Off);

            _delcom.WritePorts(0, port1);

            int blinkEnable = 0;
            int blinkDisable = 0;

            if (red == LightState.Blink)
                blinkEnable = blinkEnable.SetBitValue((int) Light.Red, true);
            else
                blinkDisable = blinkDisable.SetBitValue((int) Light.Red, true);

            if (yellow == LightState.Blink)
                blinkEnable = blinkEnable.SetBitValue((int) Light.Yellow, true);
            else
                blinkDisable = blinkDisable.SetBitValue((int) Light.Yellow, true);

            if (green == LightState.Blink)
                blinkEnable = blinkEnable.SetBitValue((int) Light.Green, true);
            else
                blinkDisable = blinkDisable.SetBitValue((int) Light.Green, true);

            _delcom.WriteBlink(blinkDisable, blinkEnable);
        }

        public void SetLight(Light light, LightState state)
        {
            switch (light)
            {
                case Light.Red:
                    _red = state;
                    break;

                case Light.Yellow:
                    _yellow = state;
                    break;

                case Light.Green:
                    _green = state;
                    break;

                default:
                    throw new ArgumentOutOfRangeException("light");
            }

            SetLights(_red, _yellow, _green);
        }
    }
}
