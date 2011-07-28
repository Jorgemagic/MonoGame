
#region Using Statements
using System;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Storage;

using MonoTouch.StoreKit;
using MonoTouch.Foundation;
#endregion

namespace RenderTarget2DTest
{
	/// <summary>
	/// This is the main type for your game
	/// </summary>
	public class Game1 : Game
	{
		GraphicsDeviceManager graphics;
		SpriteBatch spriteBatch;
		Texture2D texture;
		RenderTarget2D renderTarget;

		public Game1 ()
		{
			graphics = new GraphicsDeviceManager (this);
			Content.RootDirectory = "Content";
			graphics.IsFullScreen = true;
		}

		/// <summary>
		/// Allows the game to perform any initialization it needs to before starting to run.
		/// This is where it can query for any required services and load any non-graphic
		/// related content.  Calling base.Initialize will enumerate through any components
		/// and initialize them as well.
		/// </summary>
		protected override void Initialize ()
		{
			base.Initialize ();
			
		}

		/// <summary>
		/// LoadContent will be called once per game and is the place to load
		/// all of your content.
		/// </summary>
		protected override void LoadContent ()
		{
			// Create a new SpriteBatch, which can be used to draw textures.
			spriteBatch = new SpriteBatch (GraphicsDevice);
			
			texture = Content.Load<Texture2D>("cat.png");
			
			renderTarget = new RenderTarget2D(graphics.GraphicsDevice, 100,100,false,
			                                  SurfaceFormat.Color,DepthFormat.Depth24);
			
			
		}

		/// <summary>
		/// Allows the game to run logic such as updating the world,
		/// checking for collisions, gathering input, and playing audio.
		/// </summary>
		/// <param name="gameTime">Provides a snapshot of timing values.</param>
		protected override void Update (GameTime gameTime)
		{
			// TODO: Add your update logic here			
			base.Update (gameTime);
		}
		
		public void CreateRender()
		{
			graphics.GraphicsDevice.SetRenderTarget(renderTarget);
			
			GraphicsDevice.Clear(Color.Red);
			
			spriteBatch.Begin();
			spriteBatch.Draw(texture, Vector2.Zero, Color.White);
			spriteBatch.End();
			
			graphics.GraphicsDevice.SetRenderTarget(null);
			
		}

		/// <summary>
		/// This is called when the game should draw itself.
		/// </summary>
		/// <param name="gameTime">Provides a snapshot of timing values.</param>
		protected override void Draw (GameTime gameTime)
		{
			graphics.GraphicsDevice.SetRenderTarget(renderTarget);
			
			GraphicsDevice.Clear(Color.Red);
			
			spriteBatch.Begin();
			spriteBatch.Draw(texture, Vector2.Zero,Color.White);
			spriteBatch.End();
			
			GraphicsDevice.SetRenderTarget(null);
			
			graphics.GraphicsDevice.Clear(Color.CornflowerBlue);
			
			spriteBatch.Begin();
			spriteBatch.Draw(renderTarget, new Vector2(200,200),Color.Green);

			spriteBatch.End();
			
			base.Draw (gameTime);
			
			
		}
	}
}

