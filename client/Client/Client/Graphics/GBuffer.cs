using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using Microsoft.Xna.Framework.Graphics;

namespace Client.Graphics
{
    public sealed class GBuffer
    {
        private readonly Engine _engine;

        private RenderTarget2D _diffuseTarget;
        private RenderTarget2D _normalTarget;
        private RenderTarget2D _lightingTarget;

        private bool _drawSurfaceSizeChanged;
        private bool _isDeferredLightingEnabled;

        private SilverlightEffect _renderEffect;
        private SilverlightEffect _lightingEffect;
        private SilverlightEffect _combineEffect;

        public bool IsDeferredLightingEnabled
        {
            get { return _isDeferredLightingEnabled; }
            set { _isDeferredLightingEnabled = value; }
        }

        public GBuffer(Engine engine)
        {
            _engine = engine;
            _engine.DrawingSurface.SizeChanged += new SizeChangedEventHandler(OnDrawingSurfaceSizeChanged);

            InitializeTargets();
        }

        private void InitializeTargets()
        {
            PresentationParameters pp = _engine.GraphicsDevice.PresentationParameters;

            _diffuseTarget = new RenderTarget2D(_engine.GraphicsDevice, pp.BackBufferWidth, pp.BackBufferHeight, false, SurfaceFormat.Color, DepthFormat.None);
            _normalTarget = new RenderTarget2D(_engine.GraphicsDevice, pp.BackBufferWidth, pp.BackBufferHeight, false, SurfaceFormat.Color, DepthFormat.None);
            _lightingTarget = new RenderTarget2D(_engine.GraphicsDevice, pp.BackBufferWidth, pp.BackBufferHeight, false, SurfaceFormat.Color, DepthFormat.None);
        }

        public void EndDraw()
        {
            if (_drawSurfaceSizeChanged)
            {
                _drawSurfaceSizeChanged = false;
                InitializeTargets();
            }
        }

        public void PrepareDraw()
        {
            _engine.GraphicsDevice.SetRenderTargets(_diffuseTarget, _normalTarget);
        }
        
        public void PrepareLighting()
        {
            _engine.GraphicsDevice.SetRenderTargets(_lightingTarget);
        }

        public void Combine()
        {

        }

        public void Present()
        {

        }

        private void OnDrawingSurfaceSizeChanged(object sender, SizeChangedEventArgs e)
        {
            _drawSurfaceSizeChanged = true;
        }
    }
}
