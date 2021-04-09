using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Raylib;
using static Raylib.Raylib;
using Mlib;


namespace Project2D
{
	class StartText : GameObject
	{
		Scene s;
		public StartText(float scale, Scene scene) : base(TextureName.None, Vector2.Zero, scale, 0, null)
		{
			if (scene != null)
			{
				scene.AddUIElement(this);
				s = scene;
			}
			this.position = new Vector2(Game.screenWidth/2, Game.screenHeight/2);
			SetSprite(new Sprite(Sprite.GetFramesFromFolder("ConsumeText"), 33, 0, 57, this, RLColor.WHITE, true));
			UpdateTransforms();
		}

		public override void Update()
		{
			if (spriteManager.GetCurrentFrame() >= 57)
			{
				spriteManager.Pause();
				Game.pause = false;
				s.GetUIElements().Remove(this);
				spriteManager.UnloadFrames();
			}
			base.Update();
		}

		public override void Draw()
		{
			ClearBackground(RLColor.BLACK);
			base.Draw();
		}
	}
}
