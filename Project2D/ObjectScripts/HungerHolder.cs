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
	/// <summary>
	/// A UI object attached to a player object that shows the hunger value
	/// </summary>
	class HungerHolder : GameObject
	{
		Vector2 size;
		Vector2 hungerSize;
		Vector2 pos;
		public float hungerPercent = 0f;
		public float hungerVisual = 0f;
		public int chickenCount;
		RLColor backingColor = new RLColor(24, 12, 14, 255);
		RLColor hungerFillColor = new RLColor(120, 120, 120, 255);
		float timer = 0;
		float easeTimer = 0;
		bool easing = false;
		float hungerStart = 0;
		float timeMax = 4;
		float wiggleStart = 3;

		public HungerHolder(TextureName image, Vector2 position, float scale, Scene scene) : base(image, position, scale, 0, null, false)
		{
			if (scene != null)
				scene.AddUIElement(this);

			new GameObject(image, Vector2.Zero, 0.25f, 0, this);

			size = new Vector2(70, 45);
			hungerSize = size;
			pos = new Vector2(position.x - 30, position.y);
		}

		public override void Update()
		{
			hungerSize.y = (1- hungerVisual) * size.y;

			if (hungerVisual != hungerPercent || easeTimer > 0)
			{
				if (easing)
				{
					easeTimer += Game.deltaTime * 1f;
					hungerVisual = hungerStart + (hungerPercent - hungerStart) * (float)(1 - Math.Pow(1 - easeTimer, 5));
					if(easeTimer >= 1)
					{
						easing = false;
						hungerVisual = hungerPercent;
						easeTimer = 0;
					}
				}
				else
				{
					hungerStart = hungerVisual;
					easing = true;
				}
			}
			timer += Game.deltaTime * 2;
			if (hungerPercent > 0.8f && hungerPercent < 1 && timer >= wiggleStart)
			{
				rotation = (float)Math.Sin((timer - wiggleStart)* 2 * Trig.pi) * 0.05f;
			}
			if (timer >= timeMax)
			{
				timer = 0;
				rotation = 0;
			}
			UpdateTransforms();
			base.Update();
		}

		public override void Draw()
		{
			DrawRectangleV(pos, size, hungerFillColor);
			DrawRectangleV(pos, hungerSize, backingColor);
			base.Draw();
			
			if (hungerPercent >= 1f)
			{
				DrawText($"WOW! YOU WIN!!!", (int)pos.x - 44, (int)pos.y + 105, 20, RLColor.RED);
			}
			else
			{
				DrawText($"Chickens Left: {chickenCount}", (int)pos.x - 50, (int)pos.y + 105, 20, RLColor.WHITE);
			}
		}
	}
}
