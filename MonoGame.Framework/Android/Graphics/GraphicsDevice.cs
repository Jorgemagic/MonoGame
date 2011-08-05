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
using System.Collections.Generic;
using System.Runtime.InteropServices;

using GL11 = OpenTK.Graphics.ES11.GL;
using GL20 = OpenTK.Graphics.ES20.GL;
using All11 = OpenTK.Graphics.ES11.All;
using All20 = OpenTK.Graphics.ES20.All;

using Android.Opengl;

using Microsoft.Xna.Framework;
using OpenTK.Graphics;

namespace Microsoft.Xna.Framework.Graphics
{
    public class GraphicsDevice : IDisposable
    {
        private All11 _preferedFilterGL11;
        private All20 _preferedFilterGL20;
        private int _activeTexture = -1;
        private Viewport _viewport;

        private bool _isDisposed = false;
        private readonly DisplayMode _displayMode = new DisplayMode();
        private RenderState _renderState;
        public TextureCollection Textures { get; set; }
        internal List<IntPtr> _pointerCache = new List<IntPtr>();
        private VertexBuffer _vertexBuffer = null;
        private IndexBuffer _indexBuffer = null;

        public RasterizerState RasterizerState { get; set; }
        public DepthStencilState DepthStencilState { get; set; }
        public BlendState BlendState { get; set; }

        public static GLContextVersion openGLESVersion;
        //public static EAGLRenderingAPI openGLESVersion;
        public static int framebufferScreen;
        public static bool defaultFramebuffer = true;

        private RenderTargetBinding[] currentRenderTargets;

        internal All11 PreferedFilterGL11
        {
            get
            {
                return _preferedFilterGL11;
            }
            set
            {
                _preferedFilterGL11 = value;
            }
        }

        internal All20 PreferedFilterGL20
        {
            get
            {
                return _preferedFilterGL20;
            }
            set
            {
                _preferedFilterGL20 = value;
            }

        }

        internal int ActiveTexture
        {
            get
            {
                return _activeTexture;
            }
            set
            {
                _activeTexture = value;
            }
        }

        public bool IsDisposed
        {
            get
            {
                return _isDisposed;
            }
        }

        public GraphicsDevice()
        {
            // Initialize the main viewport
            _viewport = new Viewport();
            _viewport.X = 0;
            _viewport.Y = 0;
            _viewport.Width = DisplayMode.Width;
            _viewport.Height = DisplayMode.Height;
            _viewport.MinDepth = 0.0f;
            _viewport.MaxDepth = 1.0f;

            // Init RenderState
            _renderState = new RenderState();
        }

        public void Clear(Color color)
        {
            Vector4 vector = color.ToEAGLColor();
            if (openGLESVersion == GLContextVersion.Gles2_0)
            {
                GL20.ClearColor(vector.X, vector.Y, vector.Z, vector.W);
                GL20.Clear((uint)All20.ColorBufferBit);
            }
            else
            {
                GL11.ClearColor(vector.X, vector.Y, vector.Z, vector.W);
                GL11.Clear((uint)All11.ColorBufferBit);
            }
        }

        public void Clear(ClearOptions options, Color color, float depth, int stencil)
        {
            Clear(options, color.ToEAGLColor(), depth, stencil);
        }

        public void Clear(ClearOptions options, Vector4 color, float depth, int stencil)
        {
            if (openGLESVersion == GLContextVersion.Gles2_0)
            {
                GL20.ClearColor(color.X, color.Y, color.Z, color.W);
                GL20.ClearDepth(depth);
                GL20.ClearStencil(stencil);
                GL20.Clear((uint)(All20.ColorBufferBit | All20.DepthBufferBit | All20.StencilBufferBit));
            }
            else
            {
                GL11.ClearColor(color.X, color.Y, color.Z, color.W);
                GL11.ClearDepth(depth);
                GL11.ClearStencil(stencil);
                GL11.Clear((uint)(All11.ColorBufferBit | All11.DepthBufferBit | All11.StencilBufferBit));
            }
        }

        public void Clear(ClearOptions options, Color color, float depth, int stencil, Rectangle[] regions)
        {
            throw new NotImplementedException();
        }

        public void Clear(ClearOptions options, Vector4 color, float depth, int stencil, Rectangle[] regions)
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
            _isDisposed = true;
        }

        protected virtual void Dispose(bool aReleaseEverything)
        {
            if (aReleaseEverything)
            {

            }

            _isDisposed = true;
        }

        public void Present()
        {
            if (openGLESVersion == GLContextVersion.Gles2_0)
                GL20.Flush();
            else
                GL11.Flush();
        }

        public void Present(Rectangle? sourceRectangle, Rectangle? destinationRectangle, IntPtr overrideWindowHandle)
        {
            throw new NotImplementedException();
        }

        public void Reset()
        {
            throw new NotImplementedException();
        }

        public void Reset(Microsoft.Xna.Framework.Graphics.PresentationParameters presentationParameters)
        {
            throw new NotImplementedException();
        }

        public void Reset(Microsoft.Xna.Framework.Graphics.PresentationParameters presentationParameters, GraphicsAdapter graphicsAdapter)
        {
            throw new NotImplementedException();
        }

        public Microsoft.Xna.Framework.Graphics.DisplayMode DisplayMode
        {
            get
            {
                //return GraphicsAdapter.DefaultAdapter.CurrentDisplayMode;
                return _displayMode;
            }
        }

        public Microsoft.Xna.Framework.Graphics.GraphicsDeviceCapabilities GraphicsDeviceCapabilities
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public Microsoft.Xna.Framework.Graphics.GraphicsDeviceStatus GraphicsDeviceStatus
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public Microsoft.Xna.Framework.Graphics.PresentationParameters PresentationParameters
        {
            get;
            set;
        }

        public Microsoft.Xna.Framework.Graphics.Viewport Viewport
        {
            get
            {
                return _viewport;
            }
            set
            {
                _viewport = value;
            }
        }

        public Microsoft.Xna.Framework.Graphics.GraphicsProfile GraphicsProfile
        {
            get;
            set;
        }

        public VertexDeclaration VertexDeclaration
        {
            get;
            set;
        }

        Rectangle _scissorRectangle;
        public Rectangle ScissorRectangle
        {
            get
            {
                return _scissorRectangle;
            }
            set
            {
                _scissorRectangle = value;

                switch (this.PresentationParameters.DisplayOrientation)
                {
                    case DisplayOrientation.Portrait:
                        {
                            _scissorRectangle.Y = _viewport.Height - _scissorRectangle.Y - _scissorRectangle.Height;
                            break;
                        }

                    case DisplayOrientation.LandscapeLeft:
                        {
                            var x = _scissorRectangle.X;
                            _scissorRectangle.X = _viewport.Width - _scissorRectangle.Height - _scissorRectangle.Y;
                            _scissorRectangle.Y = _viewport.Height - _scissorRectangle.Width - x;

                            // Swap Width and Height
                            var w = _scissorRectangle.Width;
                            _scissorRectangle.Width = _scissorRectangle.Height;
                            _scissorRectangle.Height = w;
                            break;
                        }

                    case DisplayOrientation.LandscapeRight:
                        {
                            // Swap X and Y
                            var x = _scissorRectangle.X;
                            _scissorRectangle.X = _scissorRectangle.Y;
                            _scissorRectangle.Y = x;

                            // Swap Width and Height
                            var w = _scissorRectangle.Width;
                            _scissorRectangle.Width = _scissorRectangle.Height;
                            _scissorRectangle.Height = w;
                            break;
                        }

                    case DisplayOrientation.PortraitUpsideDown:
                        {
                            _scissorRectangle.Y = _viewport.Height - _scissorRectangle.Height - _scissorRectangle.Y;
                            _scissorRectangle.X = _viewport.Width - _scissorRectangle.Width - _scissorRectangle.X;
                            break;
                        }

                    case DisplayOrientation.Default:
                        {
                            _scissorRectangle.Y = _viewport.Height - _scissorRectangle.Y - _scissorRectangle.Height;
                            break;
                        }
                }
            }
        }

        public RenderState RenderState
        {
            get
            {
                return _renderState;
            }
            set
            {
                if (_renderState != value)
                {
                    _renderState = value;
                }
            }
        }

        public void SetRenderTarget(RenderTarget2D rendertarget)
        {
            //if (openGLESVersion == EAGLRenderingAPI.OpenGLES2)
            if (openGLESVersion == GLContextVersion.Gles2_0)
                SetRenderTargetGL20(rendertarget);
            else
                SetRenderTargetGL11(rendertarget);

        }


        public void SetRenderTargetGL20(RenderTarget2D rendertarget)
        {
            if (rendertarget == null)
            {
                GL20.BindFramebuffer(All20.Framebuffer, framebufferScreen);
                defaultFramebuffer = true;
            }
            else
            {
                GL20.BindFramebuffer(All20.Framebuffer, rendertarget.framebuffer);
                GL20.FramebufferTexture2D(All20.Framebuffer, All20.ColorAttachment0, All20.Texture2D, rendertarget.ID, 0);

                All20 status = GL20.CheckFramebufferStatus(All20.Framebuffer);
                // if (status != All20.FramebufferComplete)
                //     throw new Exception("Error creating framebuffer: " + status);
                defaultFramebuffer = false;
            }
        }


        public void SetRenderTargetGL11(RenderTarget2D renderTarget)
        {
            if (renderTarget == null)
            {
                // Detach the render buffers.
                GL11.Oes.FramebufferRenderbuffer(All11.FramebufferOes, All11.DepthAttachmentOes,
                        All11.RenderbufferOes, 0);
                // delete the RBO's
                GL11.Oes.DeleteRenderbuffers(renderBufferIDs.Length, renderBufferIDs);
                // delete the FBO
                GL11.Oes.DeleteFramebuffers(1, ref framebufferId);
                // Set the frame buffer back to the system window buffer
                GL11.Oes.BindFramebuffer(All11.FramebufferOes, 0);
            }
            else
            {
                SetRenderTargets(new RenderTargetBinding(renderTarget));
            }
        }

        private int framebufferId = -1;
        int[] renderBufferIDs;

        public void SetRenderTargets(params RenderTargetBinding[] renderTargets)
        {

            currentRenderTargets = renderTargets;

            if (currentRenderTargets != null)
            {

                // http://www.songho.ca/opengl/gl_fbo.html

                // create framebuffer
                GL11.Oes.GenFramebuffers(1, ref framebufferId);
                GL11.Oes.BindFramebuffer(All11.FramebufferOes, framebufferId);

                renderBufferIDs = new int[currentRenderTargets.Length];
                GL11.Oes.GenRenderbuffers(currentRenderTargets.Length, renderBufferIDs);

                for (int i = 0; i < currentRenderTargets.Length; i++)
                {
                    RenderTarget2D target = (RenderTarget2D)currentRenderTargets[0].RenderTarget;

                    // attach the texture to FBO color attachment point
                    GL11.Oes.FramebufferTexture2D(All11.FramebufferOes, All11.ColorAttachment0Oes,
                        All11.Texture2D, target.ID, 0);

                    // create a renderbuffer object to store depth info
                    GL11.Oes.BindRenderbuffer(All11.RenderbufferOes, renderBufferIDs[i]);
                    GL11.Oes.RenderbufferStorage(All11.RenderbufferOes, All11.DepthComponent24Oes,
                        target.Width, target.Height);
                    GL11.Oes.BindRenderbuffer(All11.RenderbufferOes, 0);

                    // attach the renderbuffer to depth attachment point
                    GL11.Oes.FramebufferRenderbuffer(All11.FramebufferOes, All11.DepthAttachmentOes,
                        All11.RenderbufferOes, renderBufferIDs[i]);

                }

                All11 status = GL11.Oes.CheckFramebufferStatus(All11.FramebufferOes);

                if (status != All11.FramebufferCompleteOes)
                    throw new Exception("Error creating framebuffer: " + status);
                //GL.ClearColor (Color4.Transparent);
                //GL.Clear((int)(All.ColorBufferBit | All.DepthBufferBit));

            }


        }

        public void ResolveBackBuffer(ResolveTexture2D resolveTexture)
        {
        }

        public All11 PrimitiveTypeGL11(PrimitiveType primitiveType)
        {
            switch (primitiveType)
            {
                case PrimitiveType.LineList:
                    return All11.Lines;
                case PrimitiveType.LineStrip:
                    return All11.LineStrip;
                case PrimitiveType.TriangleList:
                    return All11.Triangles;
                case PrimitiveType.TriangleStrip:
                    return All11.TriangleStrip;
            }

            throw new NotImplementedException();
        }

        public void SetVertexBuffer(VertexBuffer vertexBuffer)
        {
            _vertexBuffer = vertexBuffer;
            GL11.BindBuffer(All11.ArrayBuffer, vertexBuffer._bufferStore);
        }

        private void SetIndexBuffer(IndexBuffer indexBuffer)
        {
            _indexBuffer = indexBuffer;
            GL11.BindBuffer(All11.ElementArrayBuffer, indexBuffer._bufferStore);
        }

        public IndexBuffer Indices { set { SetIndexBuffer(value); } }

        public void DrawIndexedPrimitives(PrimitiveType primitiveType, int baseVertex, int minVertexIndex, int numbVertices, int startIndex, int primitiveCount)
        {
            if (minVertexIndex > 0 || baseVertex > 0)
                throw new NotImplementedException("baseVertex > 0 and minVertexIndex > 0 are not supported");

            var vd = VertexDeclaration.FromType(_vertexBuffer._type);
            // Hmm, can the pointer here be changed with baseVertex?
            VertexDeclaration.PrepareForUse(vd, IntPtr.Zero);

            GL11.DrawElements(PrimitiveTypeGL11(primitiveType), _indexBuffer._count, All11.UnsignedShort, new IntPtr(startIndex));
        }

        public void DrawUserPrimitives<T>(PrimitiveType primitiveType, T[] vertexData, int vertexOffset, int primitiveCount) where T : struct, IVertexType
        {
            // Unbind the VBOs
            GL11.BindBuffer(All11.ArrayBuffer, 0);
            GL11.BindBuffer(All11.ElementArrayBuffer, 0);

            var vd = VertexDeclaration.FromType(typeof(T));

            IntPtr arrayStart = GCHandle.Alloc(vertexData, GCHandleType.Pinned).AddrOfPinnedObject();

            if (vertexOffset > 0)
                arrayStart = new IntPtr(arrayStart.ToInt32() + (vertexOffset * vd.VertexStride));

            VertexDeclaration.PrepareForUse(vd, arrayStart);

            GL11.DrawArrays(PrimitiveTypeGL11(primitiveType), vertexOffset, getElementCountArray(primitiveType, primitiveCount));
        }

        public void DrawPrimitives(PrimitiveType primitiveType, int vertexStart, int primitiveCount)
        {
            var vd = VertexDeclaration.FromType(_vertexBuffer._type);
            VertexDeclaration.PrepareForUse(vd, IntPtr.Zero);

            GL11.DrawArrays(PrimitiveTypeGL11(primitiveType), vertexStart, getElementCountArray(primitiveType, primitiveCount));
        }

        public void DrawUserIndexedPrimitives<T>(PrimitiveType primitiveType, T[] vertexData, int vertexOffset, int vertexCount, int[] indexData, int indexOffset, int primitiveCount) where T : IVertexType
        {
            // Unbind the VBOs
            GL11.BindBuffer(All11.ArrayBuffer, 0);
            GL11.BindBuffer(All11.ElementArrayBuffer, 0);

            var vd = VertexDeclaration.FromType(typeof(T));

            IntPtr arrayStart = GCHandle.Alloc(vertexData, GCHandleType.Pinned).AddrOfPinnedObject();

            if (vertexOffset > 0)
                arrayStart = new IntPtr(arrayStart.ToInt32() + (vertexOffset * vd.VertexStride));

            VertexDeclaration.PrepareForUse(vd, arrayStart);

            GL11.DrawArrays(PrimitiveTypeGL11(primitiveType), vertexOffset, getElementCountArray(primitiveType, primitiveCount));
        }

        public int getElementCountArray(PrimitiveType primitiveType, int primitiveCount)
        {
            //TODO: Overview the calculation
            switch (primitiveType)
            {
                case PrimitiveType.LineList:
                    return primitiveCount * 2;
                case PrimitiveType.LineStrip:
                    return 3 + (primitiveCount - 1); // ???
                case PrimitiveType.TriangleList:
                    return primitiveCount * 2;
                case PrimitiveType.TriangleStrip:
                    return 3 + (primitiveCount - 1); // ???
            }

            throw new NotSupportedException();
        }

    }
}

