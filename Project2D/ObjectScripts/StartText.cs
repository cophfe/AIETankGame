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
		public StartText(float scale, Scene scene) : base(TextureName.None, Vector2.zero, scale, 0, null)
		{
			if (scene != null)
			{
				scene.AddUIElement(this);
				s = scene;
			}
			position = new Vector2(Game.screenWidth/2, Game.screenHeight/2);
			//the files had to be so big since raylib makes scaled up sprites look awful
			SetSprite(new Sprite(Sprite.GetFramesFromFolder("ConsumeText"), 33, 0, 57, this, RLColor.WHITE, true));
			UpdateTransforms();
		}

		public override void Update()
		{
			//please note that the game is paused before this is updated, as a UI object it isn't affected but the game does not continue until it is finished it's animation
			//once the text finishes the animation, the game resumes and the object is deleted
			if (spriteManager.GetCurrentFrame() >= 57)
			{
				spriteManager.Pause();
				Game.pause = false;
				s.GetUIElements().Remove(this);
				//the frames take up like 50mb of ram so they are unloaded after the animation is done
				spriteManager.UnloadFrames();
			}
			base.Update();
		}

		public override void Draw()
		{
			ClearBackground(RLColor.BLACK); //the background is black whilst this object is being drawn
			base.Draw();
		}
	}
}
