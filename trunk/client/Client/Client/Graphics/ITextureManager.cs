using Microsoft.Xna.Framework.Graphics;

namespace Client.Graphics
{
    public interface ITextureManager
    {
        Texture2D GetLand(int index);
        Texture2D GetStatic(int index);
    }
}
