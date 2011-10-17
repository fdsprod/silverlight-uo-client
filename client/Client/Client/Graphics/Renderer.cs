using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Client.Graphics.Shaders;

namespace Client.Graphics
{
    public class DeferredRenderer
    {
        private Engine _engine;

        private VertexPositionTexture[] _quadVertices;
        private ushort[] _quadIndices;

        public GraphicsDevice GraphicsDevice
        {
            get { return _engine.GraphicsDevice; }
        }

        public DeferredRenderer(Engine engine)
        {
            _engine = engine;

            _quadVertices = new VertexPositionTexture[]
                        {
                            new VertexPositionTexture(
                                new Vector3(0,0,1),
                                new Vector2(1,1)),
                            new VertexPositionTexture(
                                new Vector3(0,0,1),
                                new Vector2(0,1)),
                            new VertexPositionTexture(
                                new Vector3(0,0,1),
                                new Vector2(0,0)),
                            new VertexPositionTexture(
                                new Vector3(0,0,1),
                                new Vector2(1,0))
                        };

            _quadIndices = new ushort[] { 0, 1, 2, 2, 3, 0 };
        }

        public void DrawQuad(Vector2 v1, Vector2 v2)
        {
            _quadVertices[0].Position.X = v2.X;
            _quadVertices[0].Position.Y = v1.Y;

            _quadVertices[1].Position.X = v1.X;
            _quadVertices[1].Position.Y = v1.Y;

            _quadVertices[2].Position.X = v1.X;
            _quadVertices[2].Position.Y = v2.Y;

            _quadVertices[3].Position.X = v2.X;
            _quadVertices[3].Position.Y = v2.Y;

            GraphicsDevice.DrawUserIndexedPrimitives(PrimitiveType.TriangleList, _quadVertices, 0, 4, _quadIndices, 0, 2);
        }
    }
}
