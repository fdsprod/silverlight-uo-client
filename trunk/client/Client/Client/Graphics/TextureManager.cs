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
using Client.Collections;

namespace Client.Graphics
{
    public enum TextureType
    {
        Land,
        Static
    }

    public class TextureManager : ITextureManager
    {
        private Texture2D _missingTexture;
        private Cache<int, Texture2D> _landCache;

        public Cache<int, Texture2D> LandCache
        {
            get { return _landCache; }
        }

        public TextureManager(Engine engine)
        {
            _missingTexture = engine.Content.Load<Texture2D>("Textures\\missing-texture");
            _landCache = new Cache<int, Texture2D>(TimeSpan.FromMinutes(5), 0x1000);
        }

        public Texture2D GetLand(int index)
        {
            Texture2D texture = _landCache[index];

            if (texture != null)
                return texture;



            return _missingTexture;
        }

        public Texture2D GetStatic(int index)
        {
            throw new NotImplementedException();
        }

        public void Update(GameTime gameTime)
        {
            _landCache.Clean();
        }
    }
}
