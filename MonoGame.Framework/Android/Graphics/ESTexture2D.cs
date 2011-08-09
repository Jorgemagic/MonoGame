#region License
/*
Microsoft Public License (Ms-PL)
MonoGame - Copyright © 2009 The MonoGame Team

All rights reserved.

This license governs use of the accompanying software. If you use the software, you accept this license. If you do not
accept the license, do not use the software.

1. Definitions
The terms "reproduce," "reproduction," "derivative works," and "distribution" have the same meaning here as under 
U.S. copyright law.

A "contribution" is the original software, or any additions or changes to the software.
A "contributor" is any person that distributes its contribution under this license.
"Licensed patents" are a contributor's patent claims that read directly on its contribution.

2. Grant of Rights
(A) Copyright Grant- Subject to the terms of this license, including the license conditions and limitations in section 3, 
each contributor grants you a non-exclusive, worldwide, royalty-free copyright license to reproduce its contribution, prepare derivative works of its contribution, and distribute its contribution or any derivative works that you create.
(B) Patent Grant- Subject to the terms of this license, including the license conditions and limitations in section 3, 
each contributor grants you a non-exclusive, worldwide, royalty-free license under its licensed patents to make, have made, use, sell, offer for sale, import, and/or otherwise dispose of its contribution in the software or derivative works of the contribution in the software.

3. Conditions and Limitations
(A) No Trademark License- This license does not grant you rights to use any contributors' name, logo, or trademarks.
(B) If you bring a patent claim against any contributor over patents that you claim are infringed by the software, 
your patent license from such contributor to the software ends automatically.
(C) If you distribute any portion of the software, you must retain all copyright, patent, trademark, and attribution 
notices that are present in the software.
(D) If you distribute any portion of the software in source code form, you may do so only under this license by including 
a complete copy of this license with your distribution. If you distribute any portion of the software in compiled or object 
code form, you may only do so under a license that complies with this license.
(E) The software is licensed "as-is." You bear the risk of using it. The contributors give no express warranties, guarantees
or conditions. You may have additional consumer rights under your local laws which this license cannot change. To the extent
permitted under your local laws, the contributors exclude the implied warranties of merchantability, fitness for a particular
purpose and non-infringement.
*/
#endregion License

using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.IO;

using Android.Graphics;

using Java.Nio;

using GL11 = OpenTK.Graphics.ES11;
using GL20 = OpenTK.Graphics.ES20;
using OpenTK.Graphics;
using Buffer = System.Buffer;


namespace Microsoft.Xna.Framework.Graphics
{
    internal class ESTexture2D : IDisposable
    {
        private uint _name;
        private Size _size = new Size(0, 0);
        private int _width, _height;
        private SurfaceFormat _format;
        private float _maxS, _maxT;
        private IntPtr _pixelData;

        private GLContextVersion openGLVersion;


        public ESTexture2D(IntPtr data, SurfaceFormat pixelFormat, int width, int height, Size size, GL20.All filter)
        {
            InitWithDataGL20(data, pixelFormat, width, height, size, filter);
        }

        public ESTexture2D(IntPtr data, SurfaceFormat pixelFormat, int width, int height, Size size, GL11.All filter)
        {
            InitWithDataGL11(data, pixelFormat, width, height, size, filter);
        }

        public ESTexture2D(Bitmap image, GL11.All filter)
        {
            InitWithBitmapGL11(image, filter);
        }

        public ESTexture2D(Bitmap image, GL20.All filter)
        {
            InitWithBitmapGL20(image, filter);
        }

        public void InitWithBitmapGL20(Bitmap imageSource, GL20.All filter)
        {
            //TODO:  Android.Opengl.GLUtils.GetInternalFormat()
            //Android.Opengl.GLUtils.GetInternalFormat(imageSource);
            openGLVersion = GLContextVersion.Gles2_0;
            
            _format = SurfaceFormat.Color;
            if (imageSource.HasAlpha)
                _format = SurfaceFormat.Color;

            _width = imageSource.Width;
            _height = imageSource.Height;

            _size.Width = imageSource.Width;
            _size.Height = imageSource.Height;

            GL20.GL.GenTextures(1, ref _name);
            GL20.GL.BindTexture(GL20.All.Texture2D, _name);

            GL20.GL.TexParameter( GL20.All.Texture2D, GL20.All.TextureMinFilter, (int)filter);
            GL20.GL.TexParameter( GL20.All.Texture2D, GL20.All.TextureMagFilter, (int)filter);
            GL20.GL.TexParameter( GL20.All.Texture2D, GL20.All.TextureWrapS, (int)GL20.All.ClampToEdge);
            GL20.GL.TexParameter( GL20.All.Texture2D, GL20.All.TextureWrapT, (int)GL20.All.ClampToEdge);

            Android.Opengl.GLUtils.TexImage2D((int)GL20.All.Texture2D, 0, imageSource, 0);
            imageSource.Recycle();

            int errAndroidGL = Android.Opengl.GLES20.GlGetError();
            GL20.All errGenericGL = GL20.GL.GetError();
            if(errAndroidGL != Android.Opengl.GLES20.GlNoError || errGenericGL != GL20.All.NoError )
                Console.WriteLine(string.Format("OpenGL-ES 2.0:\n\tAndroid:{0,10:X}\n\tGeneric:{0, 10:X}", errAndroidGL, errGenericGL));

            _maxS = _size.Width / (float)_width;
            _maxT = _size.Height / (float)_height;
        }

        public void InitWithBitmapGL11(Bitmap imageSource, GL11.All filter)
        {
            //TODO:  Android.Opengl.GLUtils.GetInternalFormat()
            openGLVersion = GLContextVersion.Gles1_1;

            _format = SurfaceFormat.Color;
            if (imageSource.HasAlpha)
                _format = SurfaceFormat.Color;

            // scale up bitmap to be power of 2 dimensions but dont exceed 1024x1024.
            // Note: may not have to do this with OpenGL 2+

            _width = (int)Math.Pow(2, Math.Min(10, Math.Ceiling(Math.Log10(imageSource.Width) / Math.Log10(2))));
            _height = (int)Math.Pow(2, Math.Min(10, Math.Ceiling(Math.Log10(imageSource.Height) / Math.Log10(2))));

            _size.Width = imageSource.Width;
            _size.Height = imageSource.Height;

            using (Bitmap imageScaled = Bitmap.CreateScaledBitmap(imageSource, _width, _height, false))
            {
                GL11.GL.GenTextures(1, ref _name);
                GL11.GL.BindTexture(GL11.All.Texture2D, _name);
                GL11.GL.TexParameter(GL11.All.Texture2D, GL11.All.TextureMinFilter, (int)filter);
                GL11.GL.TexParameter(GL11.All.Texture2D, GL11.All.TextureMagFilter, (int)filter);

                Android.Opengl.GLUtils.TexImage2D((int)GL11.All.Texture2D, 0, imageScaled, 0);
            }

            _maxS = _size.Width / (float)_width;
            _maxT = _size.Height / (float)_height;
        }

        public void InitWithDataGL20(IntPtr data, SurfaceFormat pixelFormat, int width, int height, Size size, GL20.All filter)
        {
            openGLVersion = GLContextVersion.Gles2_0;

            GL20.GL.GenTextures(1, ref _name);
            GL20.GL.BindTexture(GL20.All.Texture2D, _name);
            GL20.GL.TexParameter(GL20.All.Texture2D, GL20.All.TextureMinFilter, (int)filter);
            GL20.GL.TexParameter(GL20.All.Texture2D, GL20.All.TextureMagFilter, (int)filter);
            GL20.GL.TexParameter(GL20.All.Texture2D, GL20.All.TextureWrapS, (int)GL20.All.ClampToEdge);
            GL20.GL.TexParameter(GL20.All.Texture2D, GL20.All.TextureWrapT, (int)GL20.All.ClampToEdge);

            int sz = 0;

            switch (pixelFormat)
            {
                case SurfaceFormat.Color /*kTexture2DPixelFormat_RGBA8888*/:
                case SurfaceFormat.Dxt1:
                case SurfaceFormat.Dxt3:
                    sz = 4;
                    GL20.GL.TexImage2D(GL20.All.Texture2D, 0, (int)GL20.All.Rgba, (int)width, (int)height, 0, GL20.All.Rgba, GL20.All.UnsignedByte, data);
                    break;
                case SurfaceFormat.Bgra4444 /*kTexture2DPixelFormat_RGBA4444*/:
                    sz = 2;
                    GL20.GL.TexImage2D(GL20.All.Texture2D, 0, (int)GL20.All.Rgba, (int)width, (int)height, 0, GL20.All.Rgba, GL20.All.UnsignedShort4444, data);
                    break;
                case SurfaceFormat.Bgra5551 /*kTexture2DPixelFormat_RGB5A1*/:
                    sz = 2;
                    GL20.GL.TexImage2D(GL20.All.Texture2D, 0, (int)GL20.All.Rgba, (int)width, (int)height, 0, GL20.All.Rgba, GL20.All.UnsignedShort5551, data);
                    break;
                case SurfaceFormat.Alpha8 /*kTexture2DPixelFormat_A8*/:
                    sz = 1;
                    GL20.GL.TexImage2D(GL20.All.Texture2D, 0, (int)GL20.All.Alpha, (int)width, (int)height, 0, GL20.All.Alpha, GL20.All.UnsignedByte, data);
                    break;
                default:
                    throw new NotSupportedException("Texture format");
            }

            _size = size;
            _width = width;
            _height = height;
            _format = pixelFormat;
            _maxS = size.Width / (float)width;
            _maxT = size.Height / (float)height;

            _pixelData = data;

        }

        public void InitWithDataGL11(IntPtr data, SurfaceFormat pixelFormat, int width, int height, Size size, GL11.All filter)
        {
            openGLVersion = GLContextVersion.Gles1_1;
            GL11.GL.GenTextures(1, ref _name);
            GL11.GL.BindTexture(GL11.All.Texture2D, _name);
            GL11.GL.TexParameter(GL11.All.Texture2D, GL11.All.TextureMinFilter, (int)filter);
            GL11.GL.TexParameter(GL11.All.Texture2D, GL11.All.TextureMagFilter, (int)filter);
            GL11.GL.TexParameter(GL11.All.Texture2D, GL11.All.TextureWrapS, (int)GL11.All.ClampToEdge);
            GL11.GL.TexParameter(GL11.All.Texture2D, GL11.All.TextureWrapT, (int)GL11.All.ClampToEdge);

            int sz = 0;

            switch (pixelFormat)
            {
                case SurfaceFormat.Color /*kTexture2DPixelFormat_RGBA8888*/:
                case SurfaceFormat.Dxt1:
                case SurfaceFormat.Dxt3:
                    sz = 4;
                    GL11.GL.TexImage2D(GL11.All.Texture2D, 0, (int)GL11.All.Rgba, (int)width, (int)height, 0, GL11.All.Rgba, GL11.All.UnsignedByte, data);
                    break;
                case SurfaceFormat.Bgra4444 /*kTexture2DPixelFormat_RGBA4444*/:
                    sz = 2;
                    GL11.GL.TexImage2D(GL11.All.Texture2D, 0, (int)GL11.All.Rgba, (int)width, (int)height, 0, GL11.All.Rgba, GL11.All.UnsignedShort4444, data);
                    break;
                case SurfaceFormat.Bgra5551 /*kTexture2DPixelFormat_RGB5A1*/:
                    sz = 2;
                    GL11.GL.TexImage2D(GL11.All.Texture2D, 0, (int)GL11.All.Rgba, (int)width, (int)height, 0, GL11.All.Rgba, GL11.All.UnsignedShort5551, data);
                    break;
                case SurfaceFormat.Alpha8 /*kTexture2DPixelFormat_A8*/:
                    sz = 1;
                    GL11.GL.TexImage2D(GL11.All.Texture2D, 0, (int)GL11.All.Alpha, (int)width, (int)height, 0, GL11.All.Alpha, GL11.All.UnsignedByte, data);
                    break;
                default:
                    throw new NotSupportedException("Texture format");
            }

            _size = size;
            _width = width;
            _height = height;
            _format = pixelFormat;
            _maxS = size.Width / (float)width;
            _maxT = size.Height / (float)height;

            _pixelData = data;
        }

        public void Dispose()
        {
            if (_name != 0)
            {
                switch (openGLVersion)
                {
                    case GLContextVersion.Gles1_0:
                    case GLContextVersion.Gles1_1:
                        GL11.GL.DeleteTextures(1, ref _name);
                        break;
                    case GLContextVersion.Gles2_0:
                        GL20.GL.DeleteTextures(1, ref _name);
                        break;

                }
            }
        }

        private static byte GetBits64(ulong source, int first, int length, int shift)
        {
            uint[] bitmasks = { 0x00, 0x01, 0x03, 0x07, 0x0f, 0x1f, 0x3f, 0x7f, 0xff };
            uint bitmask = bitmasks[length];
            source = source >> first;
            source = source & bitmask;
            source = source << shift;
            return (byte)source;
        }

        private static byte GetBits(uint source, int first, int length, int shift)
        {
            uint[] bitmasks = { 0x00, 0x01, 0x03, 0x07, 0x0f, 0x1f, 0x3f, 0x7f, 0xff };

            uint bitmask = bitmasks[length];
            source = source >> first;
            source = source & bitmask;
            source = source << shift;
            return (byte)source;
        }


        private static void SetColorFromPacked(byte[] data, int offset, byte alpha, uint packed)
        {
            byte r = (byte)(GetBits(packed, 0, 8, 0));
            byte g = (byte)(GetBits(packed, 8, 8, 0));
            byte b = (byte)(GetBits(packed, 16, 8, 0));
            data[offset] = r;
            data[offset + 1] = g;
            data[offset + 2] = b;
            data[offset + 3] = alpha;
        }

        private static void ColorsFromPacked(uint[] colors, uint c0, uint c1, bool flag)
        {
            uint rb0, rb1, rb2, rb3, g0, g1, g2, g3;

            rb0 = (c0 << 3 | c0 << 8) & 0xf800f8;
            rb1 = (c1 << 3 | c1 << 8) & 0xf800f8;
            rb0 += (rb0 >> 5) & 0x070007;
            rb1 += (rb1 >> 5) & 0x070007;
            g0 = (c0 << 5) & 0x00fc00;
            g1 = (c1 << 5) & 0x00fc00;
            g0 += (g0 >> 6) & 0x000300;
            g1 += (g1 >> 6) & 0x000300;

            colors[0] = rb0 + g0;
            colors[1] = rb1 + g1;

            if (c0 > c1 || flag)
            {
                rb2 = (((2 * rb0 + rb1) * 21) >> 6) & 0xff00ff;
                rb3 = (((2 * rb1 + rb0) * 21) >> 6) & 0xff00ff;
                g2 = (((2 * g0 + g1) * 21) >> 6) & 0x00ff00;
                g3 = (((2 * g1 + g0) * 21) >> 6) & 0x00ff00;
                colors[3] = rb3 + g3;
            }
            else
            {
                rb2 = ((rb0 + rb1) >> 1) & 0xff00ff;
                g2 = ((g0 + g1) >> 1) & 0x00ff00;
                colors[3] = 0;
            }

            colors[2] = rb2 + g2;
        }

        static public ESTexture2D InitiFromDxt3File(BinaryReader rdr, int length, int width, int height)
        {
            byte[] b = GetBits(width, length, height, rdr);

            // Copy bits
            IntPtr pointer = Marshal.AllocHGlobal(length);
            Marshal.Copy(b, 0, pointer, length);
            ESTexture2D result = new ESTexture2D(pointer, SurfaceFormat.Dxt3, width, height, new Size(width, height), GL11.All.Linear);
            Marshal.FreeHGlobal(pointer);
            return result;
        }

        public static byte[] GetBits(int width, int length, int height, BinaryReader rdr)
        {
            int xoffset = 0;
            int yoffset = 0;
            int rowLength = width * 4;
            byte[] b = new byte[length];
            ulong alpha;
            ushort c0, c1;
            uint[] colors = new uint[4];
            uint lu;
            for (int y = 0; y < height / 4; y++)
            {
                yoffset = y * 4;
                for (int x = 0; x < width / 4; x++)
                {
                    xoffset = x * 4;
                    alpha = rdr.ReadUInt64();
                    c0 = rdr.ReadUInt16();
                    c1 = rdr.ReadUInt16();
                    ColorsFromPacked(colors, c0, c1, true);
                    lu = rdr.ReadUInt32();
                    for (int i = 0; i < 16; i++)
                    {
                        int idx = GetBits(lu, 30 - i * 2, 2, 0);
                        uint ci = colors[idx];
                        int ii = 15 - i;
                        byte a = (byte)(GetBits64(alpha, ii * 4, 4, 0));
                        a += (byte)(a << 4);
                        int yy = yoffset + (ii / 4);
                        int xx = xoffset + (ii % 4);
                        int offset = yy * rowLength + xx * 4;
                        SetColorFromPacked(b, offset, a, ci);
                    }
                }
            }
            return b;
        }

        public void DrawAtPoint(Vector2 point)
        {
            float[] coordinates = { 0, _maxT, _maxS, _maxT, 0, 0, _maxS, 0 };
            float width = (float)_width * _maxS;
            float height = (float)_height * _maxT;
            float[] vertices = {	-width / 2.0f + point.X, -height / 2.0f + point.Y,	0.0f,
								width / 2.0f + point.X,	-height / 2.0f + point.Y,	0.0f,
								-width / 2.0f + point.X,	height / 2.0f + point.Y,	0.0f,
								width / 2.0f + point.X,	height / 2.0f + point.Y,	0.0f };

            GL11.GL.BindTexture(GL11.All.Texture2D, _name);
            GL11.GL.VertexPointer(3, GL11.All.Float, 0, vertices);
            GL11.GL.TexCoordPointer(2, GL11.All.Float, 0, coordinates);
            GL11.GL.DrawArrays(GL11.All.TriangleStrip, 0, 4);
        }

        public void DrawInRect(Rectangle rect)
        {
            float[] coordinates = { 0, _maxT, _maxS, _maxT, 0, 0, _maxS, 0 };
            float[] vertices = { rect.Left, rect.Top, 0.0f, rect.Right, rect.Top, 0.0f, rect.Left, rect.Bottom, 0.0f, rect.Right, rect.Bottom, 0.0f };

            GL11.GL.BindTexture(GL11.All.Texture2D, _name);
            GL11.GL.VertexPointer(3, GL11.All.Float, 0, vertices);
            GL11.GL.TexCoordPointer(2, GL11.All.Float, 0, coordinates);
            GL11.GL.DrawArrays(GL11.All.TriangleStrip, 0, 4);
        }

        public Size ContentSize
        {
            get
            {
                return _size;
            }
        }

        public SurfaceFormat PixelFormat
        {
            get
            {
                return _format;
            }
        }

        public int PixelsWide
        {
            get
            {
                return _width;
            }
        }

        public int PixelsHigh
        {
            get
            {
                return _height;
            }
        }

        public uint Name
        {
            get
            {
                return _name;
            }
        }

        public float MaxS
        {
            get
            {
                return _maxS;
            }
        }

        public float MaxT
        {
            get
            {
                return _maxT;
            }
        }
        public IntPtr PixelData
        {
            get
            {
                return _pixelData;
            }
        }       
    }

}
