using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;

using Common.Native;
using SKYPE4COMLib;
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
            _stoplightIndicator.SetLight(StoplightIndicator.Light.Green, StoplightIndicator.LightState.On);

            InitializeWindowDetection();
            InitializeSkypeDetection();

            _initialized = true;
        }

        public static void Dispose()
        {
            if (!_initialized)
                return;

            TerminateWindowDetection();
            TerminateSkypeDetection();

            _stoplightIndicator.SetLights(StoplightIndicator.LightState.Off, StoplightIndicator.LightState.Off, StoplightIndicator.LightState.Off);
            _stoplightIndicator.Dispose();

            _initialized = false;
        }

        public static Status Status
        {
            get { return _status; }
            set
            {
                _status = value;
                UpdateLights();
            }
        }

        private static void UpdateLights()
        {
            StoplightIndicator.LightState red = StoplightIndicator.LightState.Off;
            StoplightIndicator.LightState yellow = StoplightIndicator.LightState.Off;
            StoplightIndicator.LightState green = StoplightIndicator.LightState.Off;

            if (_status == Status.Auto)
            {
                ISkype skype = _skype;

                if (skype != null && skype.ActiveCalls.Count > 0)
                {
                    red = skype.Mute ? StoplightIndicator.LightState.On : StoplightIndicator.LightState.Blink;
                }
                else if (WindowHandles.Count > 0)
                {
                    yellow = StoplightIndicator.LightState.On;
                }
                else
                {
                    green = StoplightIndicator.LightState.On;
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

        #region Skype detection

        private static Skype _skype;

        public static void InitializeSkypeDetection()
        {
            _skype = new Skype();
            _skype.Attach();

            _ISkypeEvents_Event skypeEvents = _skype;

            skypeEvents.CallStatus += HandleSkypeCallStatus;
            skypeEvents.Mute += HandleSkypeEventsMute;
        }

        static void HandleSkypeEventsMute(bool mute)
        {
            UpdateLights();
        }

        static void HandleSkypeCallStatus(Call call, TCallStatus status)
        {
            UpdateLights();
        }

        public static void TerminateSkypeDetection()
        {
            _ISkypeEvents_Event skypeEvents = _skype;

            skypeEvents.CallStatus -= HandleSkypeCallStatus;
            skypeEvents.Mute -= HandleSkypeEventsMute;

            Marshal.ReleaseComObject(_skype);

            _skype = null;
        }

        #endregion

        #region Window detection

        private static readonly WinEvent.WinEventDelegate ProcDelegate = WinEventProc;

        private static readonly List<IntPtr> WindowEventHooks = new List<IntPtr>();
        private static readonly List<IntPtr> WindowHandles = new List<IntPtr>();

        private static readonly Regex WindowMatchRegex = new Regex(Properties.Settings.Default.WindowPattern);

        private static void InitializeWindowDetection()
        {
            Functions.User32.EnumWindows(EnumWindowProc, IntPtr.Zero);

            IntPtr hook = WinEvent.SetWinEventHook(WinEvent.Event.ObjectCreate, WinEvent.Event.ObjectDestroy, IntPtr.Zero, ProcDelegate, 0, 0, WinEvent.SetWinEventHookFlags.OutOfContext | WinEvent.SetWinEventHookFlags.SkipOwnProcess);
            if (hook != IntPtr.Zero)
                WindowEventHooks.Add(hook);

            hook = WinEvent.SetWinEventHook(WinEvent.Event.ObjectNameChange, WinEvent.Event.ObjectNameChange, IntPtr.Zero, ProcDelegate, 0, 0, WinEvent.SetWinEventHookFlags.OutOfContext | WinEvent.SetWinEventHookFlags.SkipOwnProcess);
            if (hook != IntPtr.Zero)
                WindowEventHooks.Add(hook);
        }

        public static void TerminateWindowDetection()
        {
            WindowEventHooks.ForEach(h => WinEvent.UnhookWinEvent(h));
        }

        private static bool EnumWindowProc(IntPtr hWnd, IntPtr lParam)
        {
            WinEventProc(IntPtr.Zero, WinEvent.Event.ObjectCreate, hWnd, WinEvent.ObjectIdentifier.Window, 0, 0, 0);
            return true;
        }

        private static void WinEventProc(IntPtr hWinEventHook, WinEvent.Event eventType, IntPtr hwnd, WinEvent.ObjectIdentifier idObject, int idChild, uint dwEventThread, uint dwmsEventTime)
        {
            if (idObject == WinEvent.ObjectIdentifier.Window && idChild == (int) WinEvent.ChildIdentifier.Self)
            {
                switch (eventType)
                {
                    case WinEvent.Event.ObjectCreate:
                    case WinEvent.Event.ObjectNameChange:

                        if (WindowHandles.Contains(hwnd))
                            WindowHandles.Remove(hwnd);

                        if (WindowMatchRegex.IsMatch(Functions.Window.GetText(hwnd)))
                        {
                            WindowHandles.Add(hwnd);
                            UpdateLights();
                        }

                        break;

                    case WinEvent.Event.ObjectDestroy:
                        if (WindowHandles.Contains(hwnd))
                            WindowHandles.Remove(hwnd);

                        if (WindowMatchRegex.IsMatch(Functions.Window.GetText(hwnd)))
                        {
                            UpdateLights();
                        }

                        break;
                }
            }
        }

        #endregion
    }
}
