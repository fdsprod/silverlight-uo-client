using System;
using System.Net;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace Client.Graphics.Shaders
{
    public class ClearShader : ShaderBase
    {
        public ClearShader(Engine engine)
            : base(engine, "Shaders\\Deferred\\ClearPass")
        {
        }
    }
}
