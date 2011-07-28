
using System;
using System.Text;
using System.Collections.Generic;
using GL11 = OpenTK.Graphics.ES11.GL;
using GL20 = OpenTK.Graphics.ES20.GL;
using All11 = OpenTK.Graphics.ES11.All;
using All20 = OpenTK.Graphics.ES20.All;

using Microsoft.Xna.Framework;
using OpenTK;

namespace Microsoft.Xna.Framework.Graphics
{
	public class SpriteBatch : GraphicsResource
	{
		SpriteBatcher _batcher;
		
		SpriteSortMode _sortMode;
		BlendState _blendState;
		SamplerState _samplerState;
		DepthStencilState _depthStencilState; 
		RasterizerState _rasterizerState;		
		Effect _effect;		
		Matrix _matrix;
		
		//OpenGLES2 variables
		int program;
		Matrix4 matWVPScreen, matWVPFramebuffer, matProjection, matViewScreen, matViewFramebuffer, matWorld;
		int uniformWVP, uniformTex;

        public SpriteBatch ( GraphicsDevice graphicsDevice )
		{
			if (graphicsDevice == null )
			{
				throw new ArgumentException("graphicsDevice");
			}	
			
			this.graphicsDevice = graphicsDevice;
			
			_batcher = new SpriteBatcher();
			
			if (GraphicsDevice.openGLESVersion == MonoTouch.OpenGLES.EAGLRenderingAPI.OpenGLES2)
				InitGL20();
		}
		
		/// <summary>
		///Initialize shaders and program on OpenGLES2.0 
		/// </summary>
		private void InitGL20()
		{
			string vertexShaderSrc =  @"uniform mat4 uMVPMatrix;
										attribute vec4 aPosition; 
                                        attribute vec2 aTexCoord;
										attribute vec4 aTint;
                                        varying vec2 vTexCoord;
										varying vec4 vTint;
                                        void main()                  
                                        {                         
                                           vTexCoord = aTexCoord;
										   vTint = aTint;
                                           gl_Position = uMVPMatrix * aPosition; 
                                        }";                           
            
            string fragmentShaderSrc = @"precision mediump float;
                                         varying vec2 vTexCoord;
										 varying vec4 vTint;
                                         uniform sampler2D sTexture;
                                           void main()                                
                                           {                                         
                                             vec4 baseColor = texture2D(sTexture, vTexCoord);
											 gl_FragColor = baseColor * vTint;

                                           }";
			
			int vertexShader = LoadShader (All20.VertexShader, vertexShaderSrc );
            int fragmentShader = LoadShader (All20.FragmentShader, fragmentShaderSrc );
            program = GL20.CreateProgram();
            if (program == 0)
                throw new InvalidOperationException ("Unable to create program");

            GL20.AttachShader (program, vertexShader);
            GL20.AttachShader (program, fragmentShader);
            
            //Set position
            GL20.BindAttribLocation (program, _batcher.attributePosition, "aPosition");
            GL20.BindAttribLocation (program, _batcher.attributeTexCoord, "aTexCoord");
            GL20.BindAttribLocation (program, _batcher.attributeTint, "aTint");
            
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
			
			matWorld = Matrix4.Identity;
			matViewScreen = Matrix4.CreateRotationZ((float)Math.PI)*
				      Matrix4.CreateRotationY((float)Math.PI)*
					  Matrix4.CreateTranslation(-this.graphicsDevice.Viewport.Width/2,
			                                    this.graphicsDevice.Viewport.Height/2,
			                                    1);
			matViewFramebuffer = Matrix4.CreateTranslation(-this.graphicsDevice.Viewport.Width/2,
			                                          -this.graphicsDevice.Viewport.Height/2,
			                                          1);
			matProjection = Matrix4.CreateOrthographic(this.graphicsDevice.Viewport.Width,
			                                           this.graphicsDevice.Viewport.Height,
			                                           -1f,1f);
			
			matWVPScreen = matWorld * matViewScreen * matProjection;
			matWVPFramebuffer = matWorld * matViewFramebuffer * matProjection;
			
			GetUniformVariables();
			
		}
		
		/// <summary>
		/// Build the shaders
		/// </summary>
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
		
		private void GetUniformVariables()
		{
			uniformWVP =  GL20.GetUniformLocation(program, "uMVPMatrix");
			uniformTex = GL20.GetUniformLocation(program, "sTexture");
		}
		
		private void SetUniformMatrix4(int location, bool transpose, ref Matrix4 matrix)
		{
			unsafe
			{
				fixed (float* matrix_ptr = &matrix.Row0.X)
				{
					GL20.UniformMatrix4(location,1,transpose,matrix_ptr);
				}
			}
		}
		
		public void Begin()
		{
			_sortMode = SpriteSortMode.Deferred;
			_blendState = BlendState.AlphaBlend;
			_depthStencilState = DepthStencilState.None;
			_samplerState = SamplerState.LinearClamp;
			_rasterizerState =  RasterizerState.CullCounterClockwise;
			_matrix = Matrix.Identity;
		}
		
		public void Begin(SpriteSortMode sortMode, BlendState blendState)
		{
			_sortMode = sortMode;
			_blendState = (blendState == null) ? BlendState.AlphaBlend : blendState;
			_depthStencilState = DepthStencilState.None;
			_samplerState = SamplerState.LinearClamp;
			_rasterizerState =  RasterizerState.CullCounterClockwise;
			_matrix = Matrix.Identity;
		}
		
		public void Begin(SpriteSortMode sortMode, BlendState blendState, SamplerState samplerState, DepthStencilState depthStencilState, RasterizerState rasterizerState )
		{
			_sortMode = sortMode;
			
			_blendState = (blendState == null) ? BlendState.AlphaBlend : blendState;
			_depthStencilState = (depthStencilState == null) ? DepthStencilState.None : depthStencilState;
			_samplerState = (samplerState == null) ? SamplerState.LinearClamp : samplerState;
			_rasterizerState =  (rasterizerState == null) ? RasterizerState.CullCounterClockwise : rasterizerState;
			
			_matrix = Matrix.Identity;
		}
		
		public void Begin(SpriteSortMode sortMode, BlendState blendState, SamplerState samplerState, DepthStencilState depthStencilState, RasterizerState rasterizerState, Effect effect)
		{
			_sortMode = sortMode;
			
			_blendState = (blendState == null) ? BlendState.AlphaBlend : blendState;
			_depthStencilState = (depthStencilState == null) ? DepthStencilState.None : depthStencilState;
			_samplerState = (samplerState == null) ? SamplerState.LinearClamp : samplerState;
			_rasterizerState =  (rasterizerState == null) ? RasterizerState.CullCounterClockwise : rasterizerState;
			
			_effect = effect;
		}
		
		public void Begin(SpriteSortMode sortMode, BlendState blendState, SamplerState samplerState, DepthStencilState depthStencilState, RasterizerState rasterizerState, Effect effect, Matrix transformMatrix)
		{
			_sortMode = sortMode;
			
			_blendState = (blendState == null) ? BlendState.AlphaBlend : blendState;
			_depthStencilState = (depthStencilState == null) ? DepthStencilState.None : depthStencilState;
			_samplerState = (samplerState == null) ? SamplerState.LinearClamp : samplerState;
			_rasterizerState =  (rasterizerState == null) ? RasterizerState.CullCounterClockwise : rasterizerState;
			
			_effect = effect;
			_matrix = transformMatrix;
		}
		
		
		public void End()
		{
			if (GraphicsDevice.openGLESVersion == MonoTouch.OpenGLES.EAGLRenderingAPI.OpenGLES2)
				End20();
			else
				End11();
		}
		
		private void End20()
		{	
			// Disable Blending by default = BlendState.Opaque
			GL20.Disable(All20.Blend);
			
			// set the blend mode
			if ( _blendState == BlendState.NonPremultiplied )
			{
				GL20.BlendFunc(All20.One, All20.OneMinusSrcAlpha);
				GL20.Enable(All20.Blend);
				GL20.BlendEquation(All20.FuncAdd);
			}
			
			if ( _blendState == BlendState.AlphaBlend )
			{
				GL20.BlendFunc(All20.SrcAlpha, All20.OneMinusSrcAlpha);
				GL20.Enable(All20.Blend);
				GL20.BlendEquation(All20.FuncAdd);
			}
			
			if ( _blendState == BlendState.Additive )
			{
				GL20.BlendFunc(All20.SrcAlpha,All20.One);
				GL20.Enable(All20.Blend);	
				GL20.BlendEquation(All20.FuncAdd);
			}			
			
			//CullMode
			GL20.FrontFace(All20.Cw);

			GL20.Enable(All20.CullFace);
			
			GL20.Viewport(0, 0, this.graphicsDevice.Viewport.Width, this.graphicsDevice.Viewport.Height);			// configura el viewport
			GL20.UseProgram(program);
			
			if (GraphicsDevice.defaultFramebuffer)
			{
				GL20.CullFace(All20.Back);
				SetUniformMatrix4(uniformWVP, false, ref matWVPScreen);
			}
			else
			{
				GL20.CullFace(All20.Front);
				SetUniformMatrix4(uniformWVP,false,ref matWVPFramebuffer);
				GL20.ClearColor(0.0f,0.0f,0.0f,0.0f);
				GL20.Clear((int) (All20.ColorBufferBit | All20.DepthBufferBit));
			}
			
			_batcher.DrawBatch20 ( _sortMode );
		}
		
		private void End11()
		{		
			// Disable Blending by default = BlendState.Opaque
			GL11.Disable(All11.Blend);
			
			// set the blend mode
			if ( _blendState == BlendState.NonPremultiplied )
			{
				GL11.BlendFunc(All11.One, All11.OneMinusSrcAlpha);
				GL11.Enable(All11.Blend);
			}
			
			if ( _blendState == BlendState.AlphaBlend )
			{
				GL11.BlendFunc(All11.SrcAlpha, All11.OneMinusSrcAlpha);
				GL11.Enable(All11.Blend);				
			}
			
			if ( _blendState == BlendState.Additive )
			{
				GL11.BlendFunc(All11.SrcAlpha,All11.One);
				GL11.Enable(All11.Blend);	
			}			
			
			// set camera
			GL11.MatrixMode(All11.Projection);
			GL11.LoadIdentity();							
			
			// Switch on the flags.
	        switch (this.graphicsDevice.PresentationParameters.DisplayOrientation)
	        {
				case DisplayOrientation.LandscapeLeft:
                {
					GL11.Rotate(-90, 0, 0, 1); 
					GL11.Ortho(0, this.graphicsDevice.Viewport.Height, this.graphicsDevice.Viewport.Width,  0, -1, 1);
					break;
				}
				
				case DisplayOrientation.LandscapeRight:
                {
					GL11.Rotate(90, 0, 0, 1); 
					GL11.Ortho(0, this.graphicsDevice.Viewport.Height, this.graphicsDevice.Viewport.Width,  0, -1, 1);
					break;
				}
				
			case DisplayOrientation.PortraitUpsideDown:
                {
					GL11.Rotate(180, 0, 0, 1); 
					GL11.Ortho(0, this.graphicsDevice.Viewport.Width, this.graphicsDevice.Viewport.Height,  0, -1, 1);
					break;
				}
				
				default:
				{
					GL11.Ortho(0, this.graphicsDevice.Viewport.Width, this.graphicsDevice.Viewport.Height, 0, -1, 1);
					break;
				}
			}			
			
			// Enable Scissor Tests if necessary
			if ( this.graphicsDevice.RenderState.ScissorTestEnable )
			{
				GL11.Enable(All11.ScissorTest);				
			}
			
			GL11.MatrixMode(All11.Modelview);			
			
			GL11.Viewport(0, 0, this.graphicsDevice.Viewport.Width, this.graphicsDevice.Viewport.Height);
			
			// Enable Scissor Tests if necessary
			if ( this.graphicsDevice.RenderState.ScissorTestEnable )
			{
				GL11.Scissor(this.graphicsDevice.ScissorRectangle.X, this.graphicsDevice.ScissorRectangle.Y, this.graphicsDevice.ScissorRectangle.Width, this.graphicsDevice.ScissorRectangle.Height );
			}			
			
			GL11.LoadMatrix( ref _matrix.M11 );	
			
			// Initialize OpenGL states (ideally move this to initialize somewhere else)	
			GL11.Disable(All11.DepthTest);
			GL11.TexEnv(All11.TextureEnv, All11.TextureEnvMode,(int) All11.BlendSrc);
			GL11.Enable(All11.Texture2D);
			GL11.EnableClientState(All11.VertexArray);
			GL11.EnableClientState(All11.ColorArray);
			GL11.EnableClientState(All11.TextureCoordArray);
			
			// Enable Culling for better performance
			GL11.Enable(All11.CullFace);
			GL11.FrontFace(All11.Cw);
			GL11.Color4(1.0f, 1.0f, 1.0f, 1.0f);						
			
			_batcher.DrawBatch11 ( _sortMode );
		}
		
		public void Draw 
			( 
			 Texture2D texture,
			 Vector2 position,
			 Nullable<Rectangle> sourceRectangle,
			 Color color,
			 float rotation,
			 Vector2 origin,
			 Vector2 scale,
			 SpriteEffects effect,
			 float depth 
			 )
		{
			if (texture == null )
			{
				throw new ArgumentException("texture");
			}
			
			SpriteBatchItem item = _batcher.CreateBatchItem();
			
			item.Depth = depth;
			item.TextureID = (int) texture.ID;
			
			Rectangle rect;
			if ( sourceRectangle.HasValue)
				rect = sourceRectangle.Value;
			else
				rect = new Rectangle( 0, 0, texture.Image.ImageWidth, texture.Image.ImageHeight );
						
			Vector2 texCoordTL = texture.Image.GetTextureCoord ( rect.X, rect.Y );
			Vector2 texCoordBR = texture.Image.GetTextureCoord ( rect.X+rect.Width, rect.Y+rect.Height );
			
			if ( effect == SpriteEffects.FlipVertically )
			{
				float temp = texCoordBR.Y;
				texCoordBR.Y = texCoordTL.Y;
				texCoordTL.Y = temp;
			}
			else if ( effect == SpriteEffects.FlipHorizontally )
			{
				float temp = texCoordBR.X;
				texCoordBR.X = texCoordTL.X;
				texCoordTL.X = temp;
			}
			
			item.Set
				(
				 position.X,
				 position.Y,
				 -origin.X*scale.X,
				 -origin.Y*scale.Y,
				 rect.Width*scale.X,
				 rect.Height*scale.Y,
				 (float)Math.Sin(rotation),
				 (float)Math.Cos(rotation),
				 color,
				 texCoordTL,
				 texCoordBR
				 );
		}
		
		public void Draw 
			( 
			 Texture2D texture,
			 Vector2 position,
			 Nullable<Rectangle> sourceRectangle,
			 Color color,
			 float rotation,
			 Vector2 origin,
			 float scale,
			 SpriteEffects effect,
			 float depth 
			 )
		{
			if (texture == null )
			{
				throw new ArgumentException("texture");
			}
			
			SpriteBatchItem item = _batcher.CreateBatchItem();
			
			item.Depth = depth;
			item.TextureID = (int) texture.ID;
			
			Rectangle rect;
			if ( sourceRectangle.HasValue)
				rect = sourceRectangle.Value;
			else
				rect = new Rectangle( 0, 0, texture.Image.ImageWidth, texture.Image.ImageHeight );
			
			Vector2 texCoordTL = texture.Image.GetTextureCoord ( rect.X, rect.Y );
			Vector2 texCoordBR = texture.Image.GetTextureCoord ( rect.X+rect.Width, rect.Y+rect.Height );
			
			if ( effect == SpriteEffects.FlipVertically )
			{
				float temp = texCoordBR.Y;
				texCoordBR.Y = texCoordTL.Y;
				texCoordTL.Y = temp;
			}
			else if ( effect == SpriteEffects.FlipHorizontally )
			{
				float temp = texCoordBR.X;
				texCoordBR.X = texCoordTL.X;
				texCoordTL.X = temp;
			}
			item.Set
				(
				 position.X,
				 position.Y,
				 -origin.X*scale,
				 -origin.Y*scale,
				 rect.Width*scale,
				 rect.Height*scale,
				 (float)Math.Sin(rotation),
				 (float)Math.Cos(rotation),
				 color,
				 texCoordTL,
				 texCoordBR
				 );
		}
		
		public void Draw (
         	Texture2D texture,
         	Rectangle destinationRectangle,
         	Nullable<Rectangle> sourceRectangle,
         	Color color,
         	float rotation,
         	Vector2 origin,
         	SpriteEffects effect,
         	float depth
			)
		{
			if (texture == null )
			{
				throw new ArgumentException("texture");
			}
			
			SpriteBatchItem item = _batcher.CreateBatchItem();
			
			item.Depth = depth;
			item.TextureID = (int) texture.ID;
			
			Rectangle rect;
			if ( sourceRectangle.HasValue)
				rect = sourceRectangle.Value;
			else
				rect = new Rectangle( 0, 0, texture.Image.ImageWidth, texture.Image.ImageHeight );

			Vector2 texCoordTL = texture.Image.GetTextureCoord ( rect.X, rect.Y );
			Vector2 texCoordBR = texture.Image.GetTextureCoord ( rect.X+rect.Width, rect.Y+rect.Height );
			if ( effect == SpriteEffects.FlipVertically )
			{
				float temp = texCoordBR.Y;
				texCoordBR.Y = texCoordTL.Y;
				texCoordTL.Y = temp;
			}
			else if ( effect == SpriteEffects.FlipHorizontally )
			{
				float temp = texCoordBR.X;
				texCoordBR.X = texCoordTL.X;
				texCoordTL.X = temp;
			}
			
			item.Set 
				( 
				 destinationRectangle.X, 
				 destinationRectangle.Y, 
				 -origin.X, 
				 -origin.Y, 
				 destinationRectangle.Width,
				 destinationRectangle.Height,
				 (float)Math.Sin(rotation),
				 (float)Math.Cos(rotation),
				 color,
				 texCoordTL,
				 texCoordBR );			
		}
		
        public void Draw(Texture2D texture, Vector2 position, Rectangle? sourceRectangle, Color color)
		{
			if (texture == null )
			{
				throw new ArgumentException("texture");
			}
			
			SpriteBatchItem item = _batcher.CreateBatchItem();
			
			item.Depth = 0.0f;
			item.TextureID = (int) texture.ID;
			
			Rectangle rect;
			if ( sourceRectangle.HasValue)
				rect = sourceRectangle.Value;
			else
				rect = new Rectangle( 0, 0, texture.Image.ImageWidth, texture.Image.ImageHeight );
			
			Vector2 texCoordTL = texture.Image.GetTextureCoord ( rect.X, rect.Y );
			Vector2 texCoordBR = texture.Image.GetTextureCoord ( rect.X+rect.Width, rect.Y+rect.Height );
			
			item.Set ( position.X, position.Y, rect.Width, rect.Height, color, texCoordTL, texCoordBR );
		}
		
		public void Draw(Texture2D texture, Rectangle destinationRectangle, Rectangle? sourceRectangle, Color color)
		{
			if (texture == null )
			{
				throw new ArgumentException("texture");
			}
			
			SpriteBatchItem item = _batcher.CreateBatchItem();
			
			item.Depth = 0.0f;
			item.TextureID = (int) texture.ID;
			
			Rectangle rect;
			if ( sourceRectangle.HasValue)
				rect = sourceRectangle.Value;
			else
				rect = new Rectangle( 0, 0, texture.Image.ImageWidth, texture.Image.ImageHeight );
			
			Vector2 texCoordTL = texture.Image.GetTextureCoord ( rect.X, rect.Y );
			Vector2 texCoordBR = texture.Image.GetTextureCoord ( rect.X+rect.Width, rect.Y+rect.Height );
			
			item.Set 
				( 
				 destinationRectangle.X, 
				 destinationRectangle.Y, 
				 destinationRectangle.Width, 
				 destinationRectangle.Height, 
				 color, 
				 texCoordTL, 
				 texCoordBR );
		}
		
		public void Draw 
			( 
			 Texture2D texture,
			 Vector2 position,
			 Color color
			 )
		{
			if (texture == null )
			{
				throw new ArgumentException("texture");
			}
			
			SpriteBatchItem item = _batcher.CreateBatchItem();
			
			item.Depth = 0;
			item.TextureID = (int) texture.ID;
			
			Rectangle rect = new Rectangle( 0, 0, texture.Image.ImageWidth, texture.Image.ImageHeight );
			
			Vector2 texCoordTL = texture.Image.GetTextureCoord ( rect.X, rect.Y );
			Vector2 texCoordBR = texture.Image.GetTextureCoord ( rect.X+rect.Width, rect.Y+rect.Height );
			
			item.Set 
				(
				 position.X,
			     position.Y,
				 rect.Width,
				 rect.Height,
				 color,
				 texCoordTL,
				 texCoordBR
				 );
		}
		
		public void Draw (Texture2D texture, Rectangle rectangle, Color color)
		{
			if (texture == null )
			{
				throw new ArgumentException("texture");
			}
			
			SpriteBatchItem item = _batcher.CreateBatchItem();
			
			item.Depth = 0;
			item.TextureID = (int) texture.ID;
			
			Vector2 texCoordTL = texture.Image.GetTextureCoord ( 0, 0 );
			Vector2 texCoordBR = texture.Image.GetTextureCoord ( texture.Image.ImageWidth, texture.Image.ImageHeight );
			
			item.Set
				(
				 rectangle.X,
				 rectangle.Y,
				 rectangle.Width,
				 rectangle.Height,
				 color,
				 texCoordTL,
				 texCoordBR
			    );
		}
		
		
		public void DrawString(SpriteFont spriteFont, string text, Vector2 position, Color color)
		{
			if (spriteFont == null )
			{
				throw new ArgumentException("spriteFont");
			}
			
			Vector2 p = position;
			
            foreach (char c in text)
            {
                if (c == '\n')
                {
                    p.Y += spriteFont.LineSpacing;
                    p.X = position.X;
                    continue;
                }
                if (spriteFont.characterData.ContainsKey(c) == false) 
					continue;
                GlyphData g = spriteFont.characterData[c];
				
				SpriteBatchItem item = _batcher.CreateBatchItem();
				
				item.Depth = 0.0f;
				item.TextureID = (int) spriteFont._texture.ID;

				Vector2 texCoordTL = spriteFont._texture.Image.GetTextureCoord ( g.Glyph.X, g.Glyph.Y );
				Vector2 texCoordBR = spriteFont._texture.Image.GetTextureCoord ( g.Glyph.X+g.Glyph.Width, g.Glyph.Y+g.Glyph.Height );

				item.Set
					(
					 p.X,
					 p.Y+g.Cropping.Y,
					 g.Glyph.Width,
					 g.Glyph.Height,
					 color,
					 texCoordTL,
					 texCoordBR
					 );
		                
				p.X += (g.Kerning.Y + g.Kerning.Z + spriteFont.Spacing);
            }			
		}
		
		public void DrawString
			(
			SpriteFont spriteFont, 
			string text, 
			Vector2 position,
			Color color,
			float rotation,
			Vector2 origin,
			float scale,
			SpriteEffects effects,
			float depth
			)
		{
			if (spriteFont == null )
			{
				throw new ArgumentException("spriteFont");
			}
			
			Vector2 p = new Vector2(-origin.X,-origin.Y);
			
			float sin = (float)Math.Sin(rotation);
			float cos = (float)Math.Cos(rotation);
			
            foreach (char c in text)
            {
                if (c == '\n')
                {
                    p.Y += spriteFont.LineSpacing;
                    p.X = -origin.X;
                    continue;
                }
                if (spriteFont.characterData.ContainsKey(c) == false) 
					continue;
                GlyphData g = spriteFont.characterData[c];
				
				SpriteBatchItem item = _batcher.CreateBatchItem();
				
				item.Depth = depth;
				item.TextureID = (int) spriteFont._texture.ID;

				Vector2 texCoordTL = spriteFont._texture.Image.GetTextureCoord ( g.Glyph.X, g.Glyph.Y );
				Vector2 texCoordBR = spriteFont._texture.Image.GetTextureCoord ( g.Glyph.X+g.Glyph.Width, g.Glyph.Y+g.Glyph.Height );
				
				if ( effects == SpriteEffects.FlipVertically )
				{
					float temp = texCoordBR.Y;
					texCoordBR.Y = texCoordTL.Y;
					texCoordTL.Y = temp;
				}
				else if ( effects == SpriteEffects.FlipHorizontally )
				{
					float temp = texCoordBR.X;
					texCoordBR.X = texCoordTL.X;
					texCoordTL.X = temp;
				}
				
				item.Set
					(
					 position.X,
					 position.Y,
					 p.X*scale,
					 (p.Y+g.Cropping.Y)*scale,
					 g.Glyph.Width*scale,
					 g.Glyph.Height*scale,
					 sin,
					 cos,
					 color,
					 texCoordTL,
					 texCoordBR
					 );

				p.X += (g.Kerning.Y + g.Kerning.Z + spriteFont.Spacing);
            }			
		}
		
		public void DrawString
			(
			SpriteFont spriteFont, 
			string text, 
			Vector2 position,
			Color color,
			float rotation,
			Vector2 origin,
			Vector2 scale,
			SpriteEffects effects,
			float depth
			)
		{			
			if (spriteFont == null )
			{
				throw new ArgumentException("spriteFont");
			}
			
			Vector2 p = new Vector2(-origin.X,-origin.Y);
			
			float sin = (float)Math.Sin(rotation);
			float cos = (float)Math.Cos(rotation);
			
            foreach (char c in text)
            {
                if (c == '\n')
                {
                    p.Y += spriteFont.LineSpacing;
                    p.X = -origin.X;
                    continue;
                }
                if (spriteFont.characterData.ContainsKey(c) == false) 
					continue;
                GlyphData g = spriteFont.characterData[c];
				
				SpriteBatchItem item = _batcher.CreateBatchItem();
				
				item.Depth = depth;
				item.TextureID = (int) spriteFont._texture.ID;

				Vector2 texCoordTL = spriteFont._texture.Image.GetTextureCoord ( g.Glyph.X, g.Glyph.Y );
				Vector2 texCoordBR = spriteFont._texture.Image.GetTextureCoord ( g.Glyph.X+g.Glyph.Width, g.Glyph.Y+g.Glyph.Height );
				
				if ( effects == SpriteEffects.FlipVertically )
				{
					float temp = texCoordBR.Y;
					texCoordBR.Y = texCoordTL.Y;
					texCoordTL.Y = temp;
				}
				else if ( effects == SpriteEffects.FlipHorizontally )
				{
					float temp = texCoordBR.X;
					texCoordBR.X = texCoordTL.X;
					texCoordTL.X = temp;
				}
				
				item.Set
					(
					 position.X,
					 position.Y,
					 p.X*scale.X,
					 (p.Y+g.Cropping.Y)*scale.Y,
					 g.Glyph.Width*scale.X,
					 g.Glyph.Height*scale.Y,
					 sin,
					 cos,
					 color,
					 texCoordTL,
					 texCoordBR
					 );

				p.X += (g.Kerning.Y + g.Kerning.Z + spriteFont.Spacing);
            }			
		}
		
		public void DrawString(SpriteFont spriteFont, StringBuilder text, Vector2 position, Color color)
		{
			DrawString ( spriteFont, text.ToString(), position, color );
		}
		
		public void DrawString
			(
			SpriteFont spriteFont, 
			StringBuilder text, 
			Vector2 position,
			Color color,
			float rotation,
			Vector2 origin,
			float scale,
			SpriteEffects effects,
			float depth
			)
		{
			DrawString ( spriteFont, text.ToString(), position, color, rotation, origin, scale, effects, depth );
		}
		
		public void DrawString
			(
			SpriteFont spriteFont, 
			StringBuilder text, 
			Vector2 position,
			Color color,
			float rotation,
			Vector2 origin,
			Vector2 scale,
			SpriteEffects effects,
			float depth
			)
		{
			DrawString ( spriteFont, text.ToString(), position, color, rotation, origin, scale, effects, depth );
		}
	}
}

