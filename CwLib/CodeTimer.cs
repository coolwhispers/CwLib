using System;
using System.Diagnostics;

namespace CwLib
{
    public class CodeTimer : IDisposable
    {
        static bool _enable = true;
        public static bool Enable
        {
            get { return _enable; }
            set { _enable = value; }
        }

        static Action<Stopwatch> _displayAction;
        public static Action<Stopwatch> DisplayAction
        {
            get { return _displayAction; }
            set { _displayAction = value; }
        }

        public IDisposable Start()
        {
            return new CodeTimer();
        }

        Stopwatch _stopwatch;
        private CodeTimer()
        {
            if (_enable)
            {
                _stopwatch = Stopwatch.StartNew();
            }
        }

        public void Dispose()
        {
            if (_stopwatch != null)
            {
                _stopwatch.Stop();

                if (_displayAction == null)
                {
                    Debug.WriteLine($"{_stopwatch.Elapsed.TotalSeconds}.{_stopwatch.Elapsed.Milliseconds}");
                }
                else
                {
                    _displayAction?.Invoke(_stopwatch);
                }
            }
        }
    }
}
