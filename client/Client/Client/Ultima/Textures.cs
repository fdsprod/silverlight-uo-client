using System;
using System.IO;
using System.Windows.Graphics;
using Microsoft.Xna.Framework.Graphics;
using Client.Configuration;
using Microsoft.Xna.Framework;

namespace Client.Ultima
{
    public class Textures
    {
        private readonly FileIndex _fileIndex;

        public Textures(Engine engine)
        {
            _fileIndex = new FileIndex(engine, "texidx.mul", "texmaps.mul", 0x1000,  10);
        }

        public Texture2D CreateTexture(int index)
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

        public void CreateTexture(int index, out Texture2D diffuse, out Texture2D normal)
        {
            diffuse = null;
            normal = null;

            int length, extra;
            bool patched;

            Stream stream = _fileIndex.Seek(index, out length, out extra, out patched);

            if (stream == null)
                return;

            int size = extra == 0 ? 64 : 128;
            int pixelCount = size * size;

            byte[] buffer = new byte[pixelCount * 2];
            ushort[] pixels = new ushort[pixelCount];

            stream.Read(buffer, 0, buffer.Length);
            Buffer.BlockCopy(buffer, 0, pixels, 0, pixelCount * 2);

            diffuse = new Texture2D(GraphicsDeviceManager.Current.GraphicsDevice, size, size, false, SurfaceFormat.Bgra5551);
            diffuse.SetData(pixels);

            float[] heights = GenerateHeightFields(pixels, size);
            Vector3[] normals = GenerateNormalFields(heights, size);

            Color[] c = new Color[heights.Length];
            for (int i = 0; i < c.Length; i++)
            {
                Vector3 n = normals[i];
                c[i] = new Color(n.X, n.Y, n.Z);
            }

            normal = new Texture2D(GraphicsDeviceManager.Current.GraphicsDevice, size, size, false, SurfaceFormat.Color);
            normal.SetData(c);
        }

        private Vector3[] GenerateNormalFields(float[] heights, int size)
        {
            Vector3[] normals = new Vector3[heights.Length];

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    //Clamping...
                    int ic = (y * size) + x;
                    int il = (y * size) + ClampZero(x - 1, size);
                    int ir = (y * size) + ClampZero(x + 1, size);
                    int it = (ClampZero(y - 1, size) * size) + x;
                    int ib = (ClampZero(y + 1, size) * size) + x;
                    int itl = (ClampZero(y - 1, size) * size) + ClampZero(x - 1, size);
                    int itr = (ClampZero(y - 1, size) * size) + ClampZero(x + 1, size);
                    int ibl = (ClampZero(y + 1, size) * size) + ClampZero(x - 1, size);
                    int ibr = (ClampZero(y + 1, size) * size) + ClampZero(x + 1, size);

                    // Wrapping...
                    //int ic = (y * size) + x;
                    //int il = (y * size) + WrapU(x - 1, size);
                    //int ir = (y * size) + WrapU(x + 1, size);
                    //int it = (WrapV(y - 1, size) * size) + x;
                    //int ib = (WrapV(y + 1, size) * size) + x;
                    //int itl = (WrapV(y - 1, size) * size) + WrapU(x - 1, size);
                    //int itr = (WrapV(y - 1, size) * size) + WrapU(x + 1, size);
                    //int ibl = (WrapV(y + 1, size) * size) + WrapU(x - 1, size);
                    //int ibr = (WrapV(y + 1, size) * size) + WrapU(x + 1, size);

                    float c = heights[ic];
                    float l = heights[il];
                    float r = heights[ir];
                    float t = heights[it];
                    float b = heights[ib];
                    float tl = heights[itl];
                    float tr = heights[itr];
                    float bl = heights[ibl];
                    float br = heights[ibr];

                    float dx = tr + 2 * r + br - tl - 2 * l - bl;
                    float dy = bl + 2 * b + br - tl - 2 * t - tr;

                    Vector3 normal = new Vector3(dx, dy, 1f / 8);
                    normal.Normalize();
                    normal.X = normal.X * 0.5f + 0.5f;
                    normal.Y = normal.Y * 0.5f + 0.5f;
                    normal.Z = normal.Z * 0.5f + 0.5f;
                    normals[ic] = normal;
                }
            }

            return normals;
        }

        private int ClampZero(int i, int size)
        {
            if (i < 0)
                return 0;

            if (i == size)
                return size - 1;

            return i;
        }

        private int WrapU(int u, int size)
        {
            if (u < 0)
                return size - 1;

            if (u == size)
                return 0;

            return u;
        }

        private int WrapV(int v, int size)
        {
            if (v < 0)
                return size - 1;

            if (v == size)
                return 0;

            return v;
        }

        private float[] GenerateHeightFields(ushort[] pixels, int size)
        {
            float[] heights = new float[pixels.Length];

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    int c = (y * size) + x;
                    ushort pixel = pixels[c];
                    int b = (pixel >> 10) & 0x1F;
                    int g = (pixel >> 5) & 0x1F;
                    int r = (pixel & 0x1F);

                    heights[c] = ((b + g + r) / 3) / 255f;
                }
            }

            return heights;
        }
    }
}
