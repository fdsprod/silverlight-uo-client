

using Microsoft.Xna.Framework;
using System.Windows;
namespace Client.Graphics
{
    public class Camera2D
    {
        private Engine _engine;

        private int _width;
        private int _height;
        private int _nearClip = 0;
        private int _farClip = 1;
        private bool _projectionDirty;
        private bool _transformDirty;
        private Vector3 _position;
        private Matrix _transform;
        private Matrix _projection;

        public Matrix Projection
        {
            get
            {
                if (_projectionDirty)
                {
                    _projectionDirty = false;
                    Matrix.CreateOrthographic(_width, _height, _nearClip, _farClip, out _projection);
                }

                return _projection;
            }
        }

        public Matrix Transform
        {
            get
            {
                if (_transformDirty)
                {
                    _transformDirty = false;
                    Matrix.CreateTranslation(ref _position, out _transform);
                }

                return _transform;
            }
        }

        public Vector2 Position
        {
            get { return new Vector2(_position.X, _position.Y); }
            set
            {
                _position.X = value.X;
                _position.Y = value.Y;
                _transformDirty = true;
            }
        }

        public int NearClip
        {
            get { return _nearClip; }
            set
            {
                _nearClip = value;
                _projectionDirty = true;
            }
        }

        public int FarClip
        {
            get { return _farClip; }
            set
            {
                _farClip = value;
                _projectionDirty = true;
            }
        }

        public Camera2D(Engine engine)
        {
            _projectionDirty = true;
            _transformDirty = true;

            _engine = engine;
            _engine.DrawingSurface.SizeChanged += OnDrawingSurfaceSizeChanged;

            _width = (int)engine.DrawingSurface.ActualWidth;
            _height = (int)engine.DrawingSurface.ActualHeight;
        }

        void OnDrawingSurfaceSizeChanged(object sender, SizeChangedEventArgs e)
        {
            _width = (int)_engine.DrawingSurface.ActualWidth;
            _height = (int)_engine.DrawingSurface.ActualHeight;
            _projectionDirty = true;
        }
    }
}
