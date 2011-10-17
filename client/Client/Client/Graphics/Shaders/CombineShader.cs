using System;
using System.Net;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace Client.Graphics.Shaders
{
    public class CombineShader : ShaderBase
    {
        private SilverlightEffectParameter _worldViewProjectionParamater;
        private SilverlightEffectParameter _ambientLightColorParameter;
        private SilverlightEffectParameter _ambientLightPowerParameter;

        public Matrix WorldViewProjection
        {
            set { _worldViewProjectionParamater.SetValue(value); }
        }

        public Vector3 AmbientLightColor
        {
            set { _ambientLightColorParameter.SetValue(value); }
        }

        public float AmbientLightPower
        {
            set { _ambientLightPowerParameter.SetValue(value); }
        }

        public CombineShader(Engine engine)
            : base(engine, "Shaders\\Deferred\\CombinePass")
        {
            _worldViewProjectionParamater = Effect.Parameters["WorldViewProjection"];
            _ambientLightColorParameter = Effect.Parameters["ambientLightColor"];
            _ambientLightPowerParameter = Effect.Parameters["ambientLightPower"];
        }
    }
}
