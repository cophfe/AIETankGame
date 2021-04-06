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
	class Smoke : PhysicsObject
	{
		Vector2 target;
		Vector2 startPosition;
		float speed;


		public Smoke(Vector2 position, float rotation, float scale, float suckSpeed, Vector2 towardsPos, float angularVelocity, GameObject parent) : base(TextureName.Pupil, position, scale, null, 1f, 0.5f, 0, rotation, parent)
		{
			AddAngularVelocity(angularVelocity);
			
			spriteManager.SetLayer(SpriteLayer.Background);

			target = towardsPos + offset;
			speed = suckSpeed;
			startPosition = position;
			spriteManager.SetTint(RLColor.GRAY);
		}

		float existanceTimer = 0;
		bool done = false;
		Vector2 offset = new Vector2(-10,20);

		public override void Update()
		{
			if (existanceTimer >= 0.5f)
			{
				scale -= new Vector2(Game.deltaTime, Game.deltaTime) * 0.5f;
				if (scale.x <= 0)
				{
					Delete();
				}
			}
			if (existanceTimer < 1)
			{
				position = Vector2.Lerp(startPosition, target, existanceTimer * existanceTimer * existanceTimer);
			}
			existanceTimer += Game.deltaTime * speed;

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

