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
		public Feather(Vector2 position, float rotation, float scale, Vector2 velocity, float angularVelocity, GameObject parent) : base(TextureName.Feather, position, scale, null, 1f, 0.5f, 0, rotation, parent)
		{
			AddAngularVelocity(angularVelocity);
			AddVelocity(velocity);
		}

		float existanceTimer = 0;
		static float existanceMax = 1;

		public override void Update()
		{
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
