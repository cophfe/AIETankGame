using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Mlib;
using Raylib;
using static Raylib.Raylib;

namespace Project2D
{
	class Feather : PhysicsObject
	{
		//feather is just a physics object that has no collision and that deletes itself after one second
		static float existanceMax = 1;
		float existanceTimer = 0;

		public Feather(Vector2 position, float rotation, float scale, Vector2 velocity, float angularVelocity, GameObject parent, SpriteLayer layer = SpriteLayer.Background, bool randomColor = false) : base(TextureName.Feather, position, scale, null, 1f, 0.5f, 0, rotation, parent)
		{
			AddAngularVelocity(angularVelocity);
			AddVelocity(velocity);
			spriteManager.SetLayer(layer);

			//can be given a random color if bool is set
			if (randomColor)
			{
				spriteManager.SetTint(new RLColor((byte)(Game.globalRand.NextDouble() * 255), (byte)(Game.globalRand.NextDouble() * 255), (byte)(Game.globalRand.NextDouble() * 255), 255));
			}
		}

		public override void Update()
		{
			//if existance time is more than max, shrink over time. when it is fully shrunk, delete it
			if (existanceTimer >= existanceMax)
			{
				scale -= new Vector2(Game.deltaTime, Game.deltaTime);
				if (scale.x <= 0)
				{
					Delete();
				}
			}
			existanceTimer += Game.deltaTime;

			base.Update();
		}

		public override void Draw()
		{
			base.Draw();
		}

		public override void UpdateTransforms()
		{
			base.UpdateTransforms();
		}

	}
}
