using System;
using System.Net;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace Client.Graphics.Shaders
{
    public class DeferredRenderShader : ShaderBase
    {
        private SilverlightEffectParameter _worldViewProjectionParamater;

        public Matrix WorldViewProjection
        {
            set { _worldViewProjectionParamater.SetValue(value); }
        }

        public DeferredRenderShader(Engine engine)
            : base(engine, "Shaders\\Deferred\\RenderPass")
        {
            _worldViewProjectionParamater = Effect.Parameters["WorldViewProjection"];
        }
    }
}
