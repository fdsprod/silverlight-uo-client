using System;

namespace Client.Framework
{
    internal class GameClock
    {
        private long _baseRealTime;
        private long _lastRealTime;
        private bool _lastRealTimeValid;
        private int _suspendCount;
        private long _suspendStartTime;
        private long _timeLostToSuspension;
        private TimeSpan _currentTimeOffset;
        private TimeSpan _currentTimeBase;
        private TimeSpan _elapsedTime;
        private TimeSpan _elapsedAdjustedTime;

        internal TimeSpan CurrentTime
        {
            get { return _currentTimeBase + _currentTimeOffset; }
        }

        internal TimeSpan ElapsedTime
        {
            get { return _elapsedTime; }
        }

        internal TimeSpan ElapsedAdjustedTime
        {
            get { return _elapsedAdjustedTime; }
        }

        internal static long Counter
        {
            get { return Stopwatch.GetTimestamp(); }
        }

        internal static long Frequency
        {
            get { return Stopwatch.Frequency; }
        }

        public GameClock()
        {
            Reset();
        }

        internal void Reset()
        {
            _currentTimeBase = TimeSpan.Zero;
            _currentTimeOffset = TimeSpan.Zero;
            _baseRealTime = GameClock.Counter;
            _lastRealTimeValid = false;
        }

        internal void Step()
        {
            long counter = GameClock.Counter;
            if (!_lastRealTimeValid)
            {
                _lastRealTime = counter;
                _lastRealTimeValid = true;
            }
            try
            {
                _currentTimeOffset = GameClock.CounterToTimeSpan(counter - _baseRealTime);
            }
            catch (OverflowException ex1)
            {
                _currentTimeBase += _currentTimeOffset;
                _baseRealTime = _lastRealTime;
                try
                {
                    _currentTimeOffset = GameClock.CounterToTimeSpan(counter - _baseRealTime);
                }
                catch (OverflowException ex2)
                {
                    _baseRealTime = counter;
                    _currentTimeOffset = TimeSpan.Zero;
                }
            }
            try
            {
                _elapsedTime = GameClock.CounterToTimeSpan(counter - _lastRealTime);
            }
            catch (OverflowException ex)
            {
                _elapsedTime = TimeSpan.Zero;
            }

            try
            {
                long num = _lastRealTime + _timeLostToSuspension;
                _elapsedAdjustedTime = GameClock.CounterToTimeSpan(counter - num);
                _timeLostToSuspension = 0L;
            }
            catch (OverflowException ex)
            {
                _elapsedAdjustedTime = TimeSpan.Zero;
            }

            _lastRealTime = counter;
        }

        internal void Suspend()
        {
            ++_suspendCount;
            if (_suspendCount == 1)
                _suspendStartTime = GameClock.Counter;
        }

        internal void Resume()
        {
            --_suspendCount;
            if (_suspendCount <= 0)
            {
                _timeLostToSuspension += GameClock.Counter - _suspendStartTime;
                _suspendStartTime = 0L;
            }
        }

        private static TimeSpan CounterToTimeSpan(long delta)
        {
            long num = 10000000L;
            return TimeSpan.FromTicks(checked(delta * num) / GameClock.Frequency);
        }
    }
}