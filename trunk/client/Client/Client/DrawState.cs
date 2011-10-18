using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;

namespace Client
{
    public class DrawState
    {
        private TimeSpan _totalGameTime;
        private TimeSpan _elapsedGameTime;
        private bool _isRunningSlowly;
        private GraphicsDevice _graphicsDevice;

        public GraphicsDevice GraphicsDevice
        {
            get { return _graphicsDevice; }
            internal set { _graphicsDevice = value; }
        }

        public TimeSpan TotalGameTime
        {
            get { return _totalGameTime; }
            internal set { _totalGameTime = value; }
        }

        public TimeSpan ElapsedGameTime
        {
            get { return _elapsedGameTime; }
            internal set { _elapsedGameTime = value; }
        }

        public bool IsRunningSlowly
        {
            get { return _isRunningSlowly; }
            internal set { _isRunningSlowly = value; }
        }
    }
}
