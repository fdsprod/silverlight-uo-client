using System;
using Client.Collections;
using Microsoft.Xna.Framework.Graphics;

namespace Client.Graphics
{
    public enum TextureType
    {
        Land,
        Static
    }

    public class TextureManager : ITextureManager
    {
        private readonly TimeSpan _cleanInterval = TimeSpan.FromMinutes(1);
        private readonly Texture2D _missingTexture;
        private readonly Cache<int, Texture2D> _landCache;
        private DateTime _lastCacheClean;

        public Cache<int, Texture2D> LandCache
        {
            get { return _landCache; }
        }

        public TextureManager(Engine engine)
        {
            _missingTexture = engine.Content.Load<Texture2D>("Textures\\missing-texture");
            _landCache = new Cache<int, Texture2D>(TimeSpan.FromMinutes(5), 0x1000);
            _lastCacheClean = DateTime.MinValue;
        }

        public Texture2D GetLand(int index)
        {
            Texture2D texture = _landCache[index];

            if (texture != null)
                return texture;

            // TODO: First check if the texture is cached on disk, if not
            //       we need to fetch it from the online storage

            return _missingTexture;
        }

        public Texture2D GetStatic(int index)
        {
            throw new NotImplementedException();
        }

        public void Update(GameTime gameTime)
        {
            DateTime now = DateTime.Now;

            if (_lastCacheClean + _cleanInterval >= now)
                return;

            _landCache.Clean();
            _lastCacheClean = now;
        }
    }
}
