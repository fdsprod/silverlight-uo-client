using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Client.Graphics.Shaders
{
    public class DiffuseShader : ShaderBase
    {
        private readonly SilverlightEffectParameter _worldViewProjectionParamater;

        public Matrix WorldViewProjection
        {
            set { _worldViewProjectionParamater.SetValue(value); }
        }

        public DiffuseShader(ClientEngine engine)
            : base(engine, "Shaders\\DiffuseEffect")
        {
            _worldViewProjectionParamater = Effect.Parameters["WorldViewProjection"];
        }
    }
}
