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
using System.IO;
using System.Windows.Graphics;

namespace Client.Ultima
{
    public class Textures
    {
        private static FileIndex _fileIndex = new FileIndex("texidx.mul", "texmaps.mul", 0x1000, 10);

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
            texture.SetData<ushort>(pixels);

            return texture;
        }
    }
}
