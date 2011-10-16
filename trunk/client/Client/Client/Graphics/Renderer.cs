using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace Client.Graphics
{
    public sealed class Renderer : IRenderer
    {
        private VertexPositionNormalTexture[] _vertices = new VertexPositionNormalTexture[8192];

        public Renderer()
        {

        }

        public void DrawLandTile(Vector2 topLeft, Vector2 topRight, Vector2 bottomLeft, Vector2 bottomRight, Texture2D texture)
        {

        }
    }
}
