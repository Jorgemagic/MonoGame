// #region License
// /*
// Microsoft Public License (Ms-PL)
// MonoGame - Copyright © 2009 The MonoGame Team
// 
// All rights reserved.
// 
// This license governs use of the accompanying software. If you use the software, you accept this license. If you do not
// accept the license, do not use the software.
// 
// 1. Definitions
// The terms "reproduce," "reproduction," "derivative works," and "distribution" have the same meaning here as under 
// U.S. copyright law.
// 
// A "contribution" is the original software, or any additions or changes to the software.
// A "contributor" is any person that distributes its contribution under this license.
// "Licensed patents" are a contributor's patent claims that read directly on its contribution.
// 
// 2. Grant of Rights
// (A) Copyright Grant- Subject to the terms of this license, including the license conditions and limitations in section 3, 
// each contributor grants you a non-exclusive, worldwide, royalty-free copyright license to reproduce its contribution, prepare derivative works of its contribution, and distribute its contribution or any derivative works that you create.
// (B) Patent Grant- Subject to the terms of this license, including the license conditions and limitations in section 3, 
// each contributor grants you a non-exclusive, worldwide, royalty-free license under its licensed patents to make, have made, use, sell, offer for sale, import, and/or otherwise dispose of its contribution in the software or derivative works of the contribution in the software.
// 
// 3. Conditions and Limitations
// (A) No Trademark License- This license does not grant you rights to use any contributors' name, logo, or trademarks.
// (B) If you bring a patent claim against any contributor over patents that you claim are infringed by the software, 
// your patent license from such contributor to the software ends automatically.
// (C) If you distribute any portion of the software, you must retain all copyright, patent, trademark, and attribution 
// notices that are present in the software.
// (D) If you distribute any portion of the software in source code form, you may do so only under this license by including 
// a complete copy of this license with your distribution. If you distribute any portion of the software in compiled or object 
// code form, you may only do so under a license that complies with this license.
// (E) The software is licensed "as-is." You bear the risk of using it. The contributors give no express warranties, guarantees
// or conditions. You may have additional consumer rights under your local laws which this license cannot change. To the extent
// permitted under your local laws, the contributors exclude the implied warranties of merchantability, fitness for a particular
// purpose and non-infringement.
// */
// #endregion License
// 
using System;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using GL11 = OpenTK.Graphics.ES11.GL;
using GL20 = OpenTK.Graphics.ES20.GL;
using All11 = OpenTK.Graphics.ES11.All;
using All20 = OpenTK.Graphics.ES20.All;

using Microsoft.Xna.Framework;
using System.Text;

namespace Microsoft.Xna.Framework.Graphics
{
	internal class SpriteBatcher
	{
		List<SpriteBatchItem> _batchItemList;
		Queue<SpriteBatchItem> _freeBatchItemQueue;
		VertexPosition2ColorTexture[] _vertexArray;
		ushort[] _index;
		GCHandle _vertexHandle;
		GCHandle _indexHandle;
		
		//OpenGLES2 variables
		int program;
		
		int attributePosition = 0;
		int attributeTexCoord = 1;
		
		

		public SpriteBatcher ()
		{
			//256 Sprites maximum
			_batchItemList = new List<SpriteBatchItem>(256);
			_freeBatchItemQueue = new Queue<SpriteBatchItem>(256);
			
			//4 vertex for sprite
			_vertexArray = new VertexPosition2ColorTexture[4*256];
			//6 indices for sprite (2 triangles)
			_index = new ushort[6*256];
			_vertexHandle = GCHandle.Alloc(_vertexArray,GCHandleType.Pinned);
			_indexHandle = GCHandle.Alloc(_index,GCHandleType.Pinned);
			
			for ( int i = 0; i < 256; i++ )
			{
				_index[i*6+0] = (ushort)(i*4);
				_index[i*6+1] = (ushort)(i*4+1);
				_index[i*6+2] = (ushort)(i*4+2);
				_index[i*6+3] = (ushort)(i*4+1);
				_index[i*6+4] = (ushort)(i*4+3);
				_index[i*6+5] = (ushort)(i*4+2);
			}
			
			if (GraphicsDevice.openGLESVersion == MonoTouch.OpenGLES.EAGLRenderingAPI.OpenGLES2)
				InitGL20();
		}
		
		private void InitGL20()
		{
			string vertexShaderSrc =  @"attribute vec4 aPosition; 
                                        attribute vec2 aTexCoord;
                                        varying vec2 vTexCoord;
                                        void main()                  
                                        {                         
                                           vTexCoord = aTexCoord;
                                           gl_Position = aPosition; 
                                        }";                           
            
            string fragmentShaderSrc = @"precision mediump float;
                                         varying vec2 vTexCoord;
                                         uniform sampler2D sTexture;
                                           void main()                                
                                           {                                         
                                             gl_FragColor = vec4(1.0,0.0,0.0,1.0);
                                             //gl_FragColor = texture2D(sTexture, vTexCoord);
                                           }";
			
			int vertexShader = LoadShader (All20.VertexShader, vertexShaderSrc );
            int fragmentShader = LoadShader (All20.FragmentShader, fragmentShaderSrc );
            program = GL20.CreateProgram();
            if (program == 0)
                throw new InvalidOperationException ("Unable to create program");

            GL20.AttachShader (program, vertexShader);
            GL20.AttachShader (program, fragmentShader);
            
            //Set position
            GL20.BindAttribLocation (program, attributePosition, "aPosition");
            GL20.BindAttribLocation (program, attributeTexCoord, "aTexCoord");
            
            
            GL20.LinkProgram (program);

            int linked = 0;
            GL20.GetProgram (program, All20.LinkStatus, ref linked);
            if (linked == 0) {
                // link failed
                int length = 0;
                GL20.GetProgram (program, All20.InfoLogLength, ref length);
                if (length > 0) {
                    var log = new StringBuilder (length);
                    GL20.GetProgramInfoLog (program, length, ref length, log);
                    Console.WriteLine ("GL2" + log.ToString ());
                }

                GL20.DeleteProgram (program);
                throw new InvalidOperationException ("Unable to link program");
            }
			
			GL20.UseProgram(program);
		}
		
		private int LoadShader ( All20 type, string source )
        {
           int shader = GL20.CreateShader(type);

           if ( shader == 0 )
                   throw new InvalidOperationException("Unable to create shader");         
        
           // Load the shader source
           int length = 0;
            GL20.ShaderSource(shader, 1, new string[] {source}, (int[])null);
           
           // Compile the shader
           GL20.CompileShader( shader );
                
              int compiled = 0;
            GL20.GetShader (shader, All20.CompileStatus, ref compiled);
            if (compiled == 0) {
                length = 0;
                GL20.GetShader (shader, All20.InfoLogLength, ref length);
                if (length > 0) {
                    var log = new StringBuilder (length);
                    GL20.GetShaderInfoLog (shader, length, ref length, log);
                    Console.WriteLine("GL2" + log.ToString ());
                }

                GL20.DeleteShader (shader);
                throw new InvalidOperationException ("Unable to compile shader of type : " + type.ToString ());
            }

            return shader;
        
        }
		
		public SpriteBatchItem CreateBatchItem()
		{
			SpriteBatchItem item;
			if ( _freeBatchItemQueue.Count > 0 )
				item = _freeBatchItemQueue.Dequeue();
			else
				item = new SpriteBatchItem();
			_batchItemList.Add(item);
			return item;
		}
		
		#region compareMethods
		int CompareTexture ( SpriteBatchItem a, SpriteBatchItem b )
		{
			return a.TextureID.CompareTo(b.TextureID);
		}
		int CompareDepth ( SpriteBatchItem a, SpriteBatchItem b )
		{
			return a.Depth.CompareTo(b.Depth);
		}
		int CompareReverseDepth ( SpriteBatchItem a, SpriteBatchItem b )
		{
			return b.Depth.CompareTo(a.Depth);
		}
		#endregion
		
		public void DrawBatch11 ( SpriteSortMode sortMode )
		{
			// nothing to do
			if ( _batchItemList.Count == 0 )
				return;
			
			// sort the batch items
			switch ( sortMode )
			{
			case SpriteSortMode.Texture :
				_batchItemList.Sort( CompareTexture );
				break;
			case SpriteSortMode.FrontToBack :
				_batchItemList.Sort ( CompareDepth );
				break;
			case SpriteSortMode.BackToFront :
				_batchItemList.Sort ( CompareReverseDepth );
				break;
			}
			
			// make sure an old draw isn't still going on.
			// cross fingers, commenting this out!!
			//GL.Flush();
			int size = VertexPosition2ColorTexture.GetSize();
			GL11.VertexPointer(2,All11.Float,size,_vertexHandle.AddrOfPinnedObject() );
			GL11.ColorPointer(4, All11.UnsignedByte,size,(IntPtr)((uint)_vertexHandle.AddrOfPinnedObject()+(uint)(sizeof(float)*2)));
			GL11.TexCoordPointer(2, All11.Float,size,(IntPtr)((uint)_vertexHandle.AddrOfPinnedObject()+(uint)(sizeof(float)*2+sizeof(uint))) );

			// setup the vertexArray array
			int startIndex = 0;
			int index = 0;
			int texID = -1;

			// make sure the vertexArray has enough space
			if ( _batchItemList.Count*4 > _vertexArray.Length )
				ExpandVertexArray( _batchItemList.Count );
			
			foreach ( SpriteBatchItem item in _batchItemList )
			{
				// if the texture changed, we need to flush and bind the new texture
				if ( item.TextureID != texID )
				{
					FlushVertexArray11( startIndex, index );
					startIndex = index;
					texID = item.TextureID;
					GL11.BindTexture ( All11.Texture2D, texID );
				}
				// store the SpriteBatchItem data in our vertexArray
				_vertexArray[index++] = item.vertexTL;
				_vertexArray[index++] = item.vertexTR;
				_vertexArray[index++] = item.vertexBL;
				_vertexArray[index++] = item.vertexBR;
				
				_freeBatchItemQueue.Enqueue ( item );
			}
			// flush the remaining vertexArray data
			FlushVertexArray11(startIndex, index);
			
			_batchItemList.Clear();
		}
		
		public void DrawBatch20 ( SpriteSortMode sortMode )
		{
			// nothing to do
			if ( _batchItemList.Count == 0 )
				return;
			
			// sort the batch items
			switch ( sortMode )
			{
			case SpriteSortMode.Texture :
				_batchItemList.Sort( CompareTexture );
				break;
			case SpriteSortMode.FrontToBack :
				_batchItemList.Sort ( CompareDepth );
				break;
			case SpriteSortMode.BackToFront :
				_batchItemList.Sort ( CompareReverseDepth );
				break;
			}
			
			// make sure an old draw isn't still going on.
			// cross fingers, commenting this out!!
			//GL.Flush();
			int size = VertexPosition2ColorTexture.GetSize();
			GL20.VertexAttribPointer(attributePosition,2,All20.Float,false,size,_vertexHandle.AddrOfPinnedObject());
			GL20.VertexAttribPointer(attributeTexCoord,2,All20.Float,false,size,(IntPtr)((uint)_vertexHandle.AddrOfPinnedObject()+(uint)(sizeof(float)*2+sizeof(uint))));
//			GL11.VertexPointer(2,All11.Float,size,_vertexHandle.AddrOfPinnedObject() );
//			GL11.ColorPointer(4, All11.UnsignedByte,size,(IntPtr)((uint)_vertexHandle.AddrOfPinnedObject()+(uint)(sizeof(float)*2)));
//			GL11.TexCoordPointer(2, All11.Float,size,(IntPtr)((uint)_vertexHandle.AddrOfPinnedObject()+(uint)(sizeof(float)*2+sizeof(uint))) );

			// setup the vertexArray array
			int startIndex = 0;
			int index = 0;
			int texID = -1;

			// make sure the vertexArray has enough space
			if ( _batchItemList.Count*4 > _vertexArray.Length )
				ExpandVertexArray( _batchItemList.Count );
			
			foreach ( SpriteBatchItem item in _batchItemList )
			{
				// if the texture changed, we need to flush and bind the new texture
				if ( item.TextureID != texID )
				{
//					FlushVertexArray20( startIndex, index );
//					startIndex = index;
//					texID = item.TextureID;
//					GL20.BindTexture ( All20.Texture2D, texID );
				}
				// store the SpriteBatchItem data in our vertexArray
				_vertexArray[index++] = item.vertexTL;
				_vertexArray[index++] = item.vertexTR;
				_vertexArray[index++] = item.vertexBL;
				_vertexArray[index++] = item.vertexBR;
				
				_freeBatchItemQueue.Enqueue ( item );
			}
			// flush the remaining vertexArray data
			FlushVertexArray20(startIndex, index);
			
			_batchItemList.Clear();
		}
		
		
		void ExpandVertexArray( int batchSize )
		{
			// increase the size of the vertexArray
			int newCount = _vertexArray.Length / 4;
			
			while ( batchSize*4 > newCount )
				newCount += 128;
			
			_vertexHandle.Free();
			_indexHandle.Free();
			
			_vertexArray = new VertexPosition2ColorTexture[4*newCount];
			_index = new ushort[6*newCount];
			_vertexHandle = GCHandle.Alloc(_vertexArray,GCHandleType.Pinned);
			_indexHandle = GCHandle.Alloc(_index,GCHandleType.Pinned);
			
			for ( int i = 0; i < newCount; i++ )
			{
				_index[i*6+0] = (ushort)(i*4);
				_index[i*6+1] = (ushort)(i*4+1);
				_index[i*6+2] = (ushort)(i*4+2);
				_index[i*6+3] = (ushort)(i*4+1);
				_index[i*6+4] = (ushort)(i*4+3);
				_index[i*6+5] = (ushort)(i*4+2);
			}
		}
		
		
		void FlushVertexArray11 ( int start, int end )
		{
			// draw stuff
			if ( start != end )
				GL11.DrawElements ( All11.Triangles, (end-start)/2*3, All11.UnsignedShort,(IntPtr)((uint)_indexHandle.AddrOfPinnedObject()+(uint)(start/2*3*sizeof(short))) );
		}
		
		void FlushVertexArray20 ( int start, int end )
		{
			// draw stuff
			if ( start != end )
				GL20.DrawElements ( All20.Triangles, (end-start)/2*3, All20.UnsignedShort,(IntPtr)((uint)_indexHandle.AddrOfPinnedObject()+(uint)(start/2*3*sizeof(short))) );
		}
	}
}

