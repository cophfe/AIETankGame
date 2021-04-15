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
		float collideTime; //time before collider is enabled
		float wakeTime; //time before chicken hatches
		static Random rand = new Random();

		public PhysicsChicken(Vector2 position, Vector2 velocity, float timeToCollide, float timeToWake) : base(TextureName.Egg, position, 0.5f, null, 0.5f, 0.5f, 1f, 0f, Game.GetCurrentScene(), 1)
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

			if (collider == null && timer > collideTime) //after a specified time the collider is spawned in
			{
				SetCollider(new Collider(-40, -55, 80, 110, CollisionLayer.Enemy), Game.GetCurrentScene(), true);
			}
			
			if (rand.NextDouble() * Game.deltaTime < 0.00005f) //at random points the egg drops feathers with low velocity
				new Feather(position + new Vector2((float)rand.NextDouble() * 20 - 10, (float)rand.NextDouble() * 20 - 10), (float)rand.NextDouble() * Num.pi * 2 - Num.pi, (float)rand.NextDouble() * 0.2f + 0.1f, Vector2.zero, (float)rand.NextDouble() - 0.5f, Game.GetCurrentScene());
			
			if (timer > wakeTime) //at a certain point the chicken hatches (feathers spawn in a circle, a chicken is spawned, and this gameobject is deleted)
			{
				Vector2 rot = Vector2.right * 500;
				for (int i = 0; i < 40; i++)
				{
					new Feather(position, (float)rand.NextDouble() * Num.pi * 2 - Num.pi, (float)rand.NextDouble() * 0.2f + 0.1f, rot, (float)rand.NextDouble() - 0.5f, Game.GetCurrentScene());
					rot.Rotate(Num.pi * 0.05f);
				}
				Game.GetCurrentScene().AddChild(new Chicken(TextureName.Chicken, position, Game.GetPlayer()));
				RemoveCollider(Game.GetCurrentScene());
				Delete();
			}
			base.Update();
		}
	}
}
