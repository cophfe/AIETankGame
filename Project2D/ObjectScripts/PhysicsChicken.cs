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
	class PhysicsChicken : PhysicsObject
	{
		float timer = 0;
		float collideTime;
		float wakeTime;
		static Random rand = new Random();

		public PhysicsChicken(Vector2 position, Vector2 velocity, float timeToCollide, float timeToWake) : base(TextureName.GooBird, position, 0.5f, null, 0.5f, 0.5f, 1f, 0f, Game.GetCurrentScene(), 1)
		{
			collideTime = timeToCollide;
			wakeTime = timeToWake;
			spriteManager.SetLayer(SpriteLayer.Background);
			spriteManager.SetTint(new RLColor(240,240,255, 255));
			AddVelocity(velocity);
			AddAngularVelocity((float)rand.NextDouble() * 2f);
		}

		public override void Update()
		{
			timer += Game.deltaTime;

			if (collider == null && timer > collideTime)
			{
				SetCollider(new Collider(-40, -55, 80, 110, CollisionLayer.Enemy), Game.GetCurrentScene(), true);
			}
			
			if (rand.NextDouble() * Game.deltaTime < 0.00005f)
				new Feather(position + new Vector2((float)rand.NextDouble() * 20 - 10, (float)rand.NextDouble() * 20 - 10), (float)rand.NextDouble() * Trig.pi * 2 - Trig.pi, (float)rand.NextDouble() * 0.2f + 0.1f, Vector2.Zero, (float)rand.NextDouble() - 0.5f, Game.GetCurrentScene());
			
			if (timer > wakeTime)
			{
				Vector2 rot = Vector2.Right * 500;
				for (int i = 0; i < 40; i++)
				{
					new Feather(position, (float)rand.NextDouble() * Trig.pi * 2 - Trig.pi, (float)rand.NextDouble() * 0.2f + 0.1f, rot, (float)rand.NextDouble() - 0.5f, Game.GetCurrentScene());
					rot.Rotate(Trig.pi * 0.05f);
				}
				Game.GetCurrentScene().AddChild(new Chicken(TextureName.Chicken, position, Game.GetPlayer()));
				RemoveCollider(Game.GetCurrentScene());
				Delete();
			}
			base.Update();
		}
	}
}
