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
	class HungerHolder : GameObject
	{
		Rectangle hungerRect;
		Vector2 size;
		Vector2 hungerSize;
		Vector2 pos;
		public float hungerPercent = 0f;
		RLColor backingColor = new RLColor(24, 12, 14, 255);
		RLColor hungerFillColor = new RLColor(125, 65, 36, 255);

		public HungerHolder(TextureName image, float width, float height, Vector2 position, float scale, Scene scene) : base(image, position, scale, 0, null, false)
		{
			if (scene != null)
				scene.AddUIElement(this);
			
			new GameObject(image, Vector2.Zero, 0.25f, 0, this);

			size = new Vector2(70, 60);
			hungerSize = new Vector2(70, 60);
			pos = new Vector2(position.x - 30, position.y - 10);
		}

		public override void Update()
		{
			hungerSize.y = (1-hungerPercent) * size.y;
			base.Update();
		}

		public override void Draw()
		{
			DrawRectangleV(pos, size, hungerFillColor);
			DrawRectangleV(pos, hungerSize, backingColor);
			base.Draw();
		}
	}
}
