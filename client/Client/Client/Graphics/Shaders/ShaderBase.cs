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

namespace Client.Graphics.Shaders
{
    public abstract class ShaderBase
    {
        private readonly SilverlightEffect _effect;

        public SilverlightEffect Effect
        {
            get { return _effect; }
        }

        public ShaderBase(ClientEngine engine, string assetName)
        {
            _effect = engine.Content.Load<SilverlightEffect>(assetName);
        }

        public void Apply()
        {
            _effect.CurrentTechnique.Passes[0].Apply();
        }
    }
}
