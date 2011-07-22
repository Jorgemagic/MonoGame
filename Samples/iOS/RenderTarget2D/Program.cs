
#region Using Statements
using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Media;

using MonoTouch.Foundation;
using MonoTouch.UIKit;
#endregion

namespace RenderTarget2DTest
{
	[Register("AppDelegate")]
	class Program : UIApplicationDelegate
	{
		public override void FinishedLaunching (UIApplication app)
		{
			using (Game1 game = new Game1 ()) {
				game.Run ();
			}
		}

		static void Main (string[] args)
		{
			UIApplication.Main (args, null, "AppDelegate");
		}
	}
}

