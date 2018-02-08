using Common.Native;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
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
        private static WindowPatterns _windowPatterns;
        private static StoplightIndicator _stoplightIndicator;
        private static bool _initialized;
        private static Status _status = Status.Auto;

        public static void Initialize()
        {
            WindowPatterns.Changed += HandleWindowPatternsChanged;

            _stoplightIndicator = new StoplightIndicator();
            _stoplightIndicator.SetLight(StoplightIndicator.Light.Green, StoplightIndicator.LightState.On);

            LoadPatterns();

            _initialized = true;
        }

        private static void LoadPatterns()
        {
            _windowPatterns = WindowPatterns.Load();

            if (_initialized)
                TerminateWindowDetection();

            InitializeWindowDetection();

            UpdateLights();
        }

        private static void HandleWindowPatternsChanged(object sender, EventArgs e)
        {
            LoadPatterns();
        }

        public static void Dispose()
        {
            if (!_initialized)
                return;

            TerminateWindowDetection();

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
                if (WindowHandles.Count > 0)
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

        #region Window detection

        private static readonly WinEvent.WinEventDelegate ProcDelegate = WinEventProc;

        private static readonly List<IntPtr> WindowEventHooks = new List<IntPtr>();
        private static readonly List<IntPtr> WindowHandles = new List<IntPtr>();

        private static readonly List<Regex> WindowMatchRegexList = new List<Regex>();

        private static string WildcardToRegexPattern(string value)
        {
            return "^" + Regex.Escape(value).Replace("\\?", ".").Replace("\\*", ".*") + "$";
        }

        private static void InitializeWindowDetection()
        {
            foreach (var windowPattern in _windowPatterns.Where(w => w.Enabled))
                WindowMatchRegexList.Add(new Regex(WildcardToRegexPattern(windowPattern.Pattern)));

            Functions.User32.EnumWindows(EnumWindowProc, IntPtr.Zero);

            IntPtr hook = WinEvent.SetWinEventHook(WinEvent.Event.ObjectCreate, WinEvent.Event.ObjectDestroy, IntPtr.Zero, ProcDelegate, 0, 0, WinEvent.SetWinEventHookFlags.OutOfContext | WinEvent.SetWinEventHookFlags.SkipOwnProcess);
            if (hook != IntPtr.Zero)
                WindowEventHooks.Add(hook);

            hook = WinEvent.SetWinEventHook(WinEvent.Event.ObjectNameChange, WinEvent.Event.ObjectNameChange, IntPtr.Zero, ProcDelegate, 0, 0, WinEvent.SetWinEventHookFlags.OutOfContext | WinEvent.SetWinEventHookFlags.SkipOwnProcess);
            if (hook != IntPtr.Zero)
                WindowEventHooks.Add(hook);
        }

        private static void TerminateWindowDetection()
        {
            WindowEventHooks.ForEach(h => WinEvent.UnhookWinEvent(h));

            WindowMatchRegexList.Clear();

            WindowHandles.Clear();
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

                        foreach (var regex in WindowMatchRegexList)
                        {
                            if (regex.IsMatch(Functions.Window.GetText(hwnd)))
                            {
                                WindowHandles.Add(hwnd);
                                UpdateLights();
                            }
                        }

                        break;

                    case WinEvent.Event.ObjectDestroy:
                        if (WindowHandles.Contains(hwnd))
                        {
                            WindowHandles.Remove(hwnd);
                            UpdateLights();
                        }

                        break;
                }
            }
        }

        #endregion
    }
}
