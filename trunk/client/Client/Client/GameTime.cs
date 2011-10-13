using System;

namespace Client
{
    public class GameTime
    {
        private TimeSpan _totalGameTime;
        private TimeSpan _elapsedGameTime;
        private bool _isRunningSlowly;

        public TimeSpan TotalGameTime
        {
            get
            {
                return _totalGameTime;
            }
            internal set
            {
                _totalGameTime = value;
            }
        }

        public TimeSpan ElapsedGameTime
        {
            get
            {
                return _elapsedGameTime;
            }
            internal set
            {
                _elapsedGameTime = value;
            }
        }

        public bool IsRunningSlowly
        {
            get
            {
                return _isRunningSlowly;
            }
            internal set
            {
                _isRunningSlowly = value;
            }
        }

        public GameTime()
        {
        }

        public GameTime(TimeSpan totalGameTime, TimeSpan elapsedGameTime, bool isRunningSlowly)
        {
            _totalGameTime = totalGameTime;
            _elapsedGameTime = elapsedGameTime;
            _isRunningSlowly = isRunningSlowly;
        }

        public GameTime(TimeSpan totalGameTime, TimeSpan elapsedGameTime)
            : this(totalGameTime, elapsedGameTime, false)
        {
        }
    }
}