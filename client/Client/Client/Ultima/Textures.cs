using System;
using System.IO;
using System.Windows.Graphics;
using Microsoft.Xna.Framework.Graphics;

namespace Client.Ultima
{
    public class Textures
    {
        private static readonly FileIndex _fileIndex = new FileIndex("texidx.mul", "texmaps.mul", 0x1000, 10);

        public static Texture2D CreateTexture(int index)
        {
            int length, extra;
            bool patched;

            Stream stream = _fileIndex.Seek(index, out length, out extra, out patched);

            if (stream == null)
                return null;

            int size = extra == 0 ? 64 : 128;
            int pixelCount = size * size;

            byte[] buffer = new byte[pixelCount * 2];
            ushort[] pixels = new ushort[pixelCount];

            stream.Read(buffer, 0, buffer.Length);
            Buffer.BlockCopy(buffer, 0, pixels, 0, pixelCount * 2);

            Texture2D texture = new Texture2D(GraphicsDeviceManager.Current.GraphicsDevice, size, size, false, SurfaceFormat.Bgra5551);
            texture.SetData(pixels);

            return texture;
        }
    }
}
