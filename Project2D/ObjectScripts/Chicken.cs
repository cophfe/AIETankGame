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
	class Chicken : PhysicsObject
	{
		public bool still = true;
		Character player;
		public static float distanceToScare = 300;
		public static float speed = 100;
		public static float runMultiplier = 3;
		public static float runDist = 200;
		public static float eventTime = 1;
		public static float accelerationCap = 2000;
		public static float rayNumber = 0;
		public static float featherExplosionNumber = 24;
		public static float featherExplosionSpeed = 200;
		public static float avoidDistance = 50;
		public static float rayLimit = 1000;
		public static float cookedLimit = 3;
		public float cookedValue = 0;
		float deathY = 0;
		bool controlledByPlayer = false;
		static Colour colour = new Colour(255, 255, 255, 255);

		bool falling = false;
		bool dead = false;
		bool dying = false;
		float timer = 0;
		Random rand = new Random();
		Vector2 targetVelocity = Vector2.Zero;
		static Matrix3 rayRotationMatrix = Matrix3.GetRotateZ(Trig.pi * 2 / rayNumber);
		static Matrix3 explosionRotationMatrix = Matrix3.GetRotateZ(Trig.pi * 2 / featherExplosionNumber);
		static Texture2D[] frames = Sprite.GetFramesFromFolder("Chicken");

		float deathPercentDone = 0;
		
		public Chicken(TextureName fileName, Vector2 position, Character player, Sprite loadedSprite = null) : base(fileName, position, 0.5f, new Collider(-40, -55, 80, 110, CollisionLayer.Enemy), 0.5f, 0.5f, 1f, 0f, density: 1, isRotatable: false)
		{
			if (loadedSprite != null)
				spriteManager = loadedSprite;
			else
				spriteManager = new Sprite(frames, 24, 1, 11, this, colour);

			spriteManager.SetFrame(0);
			spriteManager.Pause();
			spriteManager.SetLayer(SpriteLayer.Background);
			spriteManager.SetTint(colour);
			this.player = player;
		}

		public override void Update()
		{
			base.Update();
			if (dead)
			{
				if (!controlledByPlayer && player.broadcastingHunger)
				{
					if (dying)
					{
						dying = false;
					}
					else
					{
						deathPercentDone += Game.deltaTime * 240;
						if (deathPercentDone >= 100)
						{
							rotation = 0;
							player.TieDeadChicken(this);
							controlledByPlayer = true;
						}
						else
						{
							rotation = (float)Math.Sin(Game.currentTime / (200 - deathPercentDone * 0.5f)) * deathPercentDone * 0.001f;
						}
						new Smoke(LocalPosition + new Vector2((float)rand.NextDouble() * 50 - 25, (float)rand.NextDouble() * 30 + 10), 0, (float)rand.NextDouble() * 0.15f, 5, player.GetChicken(), new Vector2((float)rand.NextDouble() * 20 - 10, (float)rand.NextDouble() * 20 - 10), 0, Game.GetCurrentScene());
					}
				}
				else if (falling)
				{
					if (deathY > position.y)
						velocity.y += 1 * Game.deltaTime;
					else
					{
						velocity.y = 0;
						falling = false;
					}
				}

				return;
			}
			if (dying)
			{
				if (spriteManager.CurrentFrame() == 51)
				{
					spriteManager.Pause();
					dead = true;
					RemoveCollider(Game.GetCurrentScene());
					player.chickensBeingSucked = true;
				}
				return;
			}
			byte gb = (byte)(colour.GetGreen() - cookedValue / cookedLimit * colour.GetGreen());
			spriteManager.SetTint(new Colour(colour.GetRed(), gb, gb, 255));
			if (cookedValue > cookedLimit)
			{
				Vector2 rot = Vector2.Right * featherExplosionSpeed;
				velocity = Vector2.Zero;
				for (int i = 0; i < featherExplosionNumber; i++)
				{
					rot = explosionRotationMatrix * rot;
					new Feather(position + new Vector2(), (float)rand.NextDouble() * Trig.pi * 2 - Trig.pi, (float)rand.NextDouble() * 0.2f + 0.1f, rot, (float)rand.NextDouble() - 0.5f, Game.GetCurrentScene());
				}
				cookedValue = 0;
				spriteManager.PlayFrom(35);
				spriteManager.SetLimits(35, 51);
				spriteManager.SetTint(RLColor.WHITE);
				spriteManager.SetSpeed(10);
				dying = true;
				drag = 3;
				return;
			}
			else if (cookedValue > 0)
			{
				cookedValue-= Game.deltaTime;
				if (still)
				{
					spriteManager.SetLimits(1, 10);
					spriteManager.PlayFrom(1);
					still = false;
					targetVelocity = Vector2.Zero;
				}
			}
			else
			{
				cookedValue = 0;
			}
			Vector2 delta = player.GlobalPosition - position;
			timer += Game.deltaTime;
			if (still)
			{
				if (delta.MagnitudeSquared() < distanceToScare * distanceToScare)
				{
					spriteManager.SetLimits(1, 10);
					spriteManager.PlayFrom(1);
					still = false;
					targetVelocity = Vector2.Zero;
					return;
				}

				if (timer > eventTime)
				{
					timer = 0;
					double r = rand.NextDouble();
					if (r > 0.5f)
					{
						spriteManager.SetLimits(12, 34);
						spriteManager.PlayFrom(12);
						if (targetVelocity.MagnitudeSquared() > 0.001f)
							targetVelocity = Matrix3.GetRotateZ((float)rand.NextDouble() * Trig.pi - Trig.pi / 2) * targetVelocity;
						else
							targetVelocity = Matrix3.GetRotateZ((float)rand.NextDouble() * Trig.pi * 2 - Trig.pi) * Vector2.One * speed;
					}
					else
					{
						targetVelocity = Vector2.Zero;
						spriteManager.Pause();
						spriteManager.SetFrame(0);
					}
				}
			}
			else
			{
				if (delta.MagnitudeSquared() > (distanceToScare + runDist) * (distanceToScare + runDist) && cookedValue == 0)
				{
					targetVelocity = Vector2.Zero;
					spriteManager.Pause();
					spriteManager.SetFrame(0);
					still = true;
				}
				else
				{
					if (rand.NextDouble() < Game.deltaTime * 4)
					{
						new Feather(position + new Vector2(), (float)rand.NextDouble() * Trig.pi * 2 - Trig.pi, (float)rand.NextDouble() * 0.2f + 0.1f, -0.5f * velocity + new Vector2((float)rand.NextDouble() * 10 - 5, (float)rand.NextDouble() * 10 - 5), (float)rand.NextDouble() - 0.5f, Game.GetCurrentScene());
					}
					targetVelocity = delta.Normalised() * (-speed * runMultiplier);
				}
			}

			Vector2 rayVector = Vector2.Right;
			Ray ray;
			Hit hit;
			CollisionManager cM = Game.GetCurrentScene().GetCollisionManager();

			if (velocity != Vector2.Zero)
			{
				for (int i = 0; i < rayNumber; i++)
				{
					ray = new Ray(collider.GetCentrePoint(), rayVector);
					if (cM.RayCast(ray, out hit, rayLimit, CollisionLayer.Player, CollisionLayer.Enemy))
					{
						
						if (hit.distanceAlongRay < avoidDistance)
						{
							float weight = ray.direction.Dot(velocity.Normalised());
							Vector2 directionVector = ray.direction * -targetVelocity.Magnitude();
							targetVelocity = new Vector2(Trig.Lerp(targetVelocity.x, directionVector.x, 1 - weight), Trig.Lerp(targetVelocity.y, directionVector.y, 1- weight));
						}
					}
					rayVector = rayRotationMatrix * rayVector;
				}
			}
			float dot = velocity.Dot(Vector2.Right);
			if (dot > 0.1f)
				spriteManager.FlipX(true);
			else if (dot < -0.1f)
				spriteManager.FlipX(false);

			Vector2 deltaV;
			if (float.IsNaN(targetVelocity.x))
			{
				targetVelocity = Vector2.Zero;
				return;
			}
			Vector2 cache = targetVelocity - velocity;
			if (cache.MagnitudeSquared() > accelerationCap * Game.deltaTime * accelerationCap * Game.deltaTime)
				deltaV = cache.Normalised() * (accelerationCap * Game.deltaTime);
			else
			{
				deltaV = cache;
			}
			deltaV = targetVelocity.MagnitudeSquared() > (accelerationCap * Game.deltaTime) * (accelerationCap * Game.deltaTime) ? deltaV : cache;
			velocity += deltaV;
		}

		public bool IsDead
		{
			get
			{
				return dead;
			}
		}

		public void CancelSuck(float startY)
		{
			controlledByPlayer = false;
			deathY = startY;
			deathPercentDone = 0;
			falling = true;
		}

		public override GameObject Clone()
		{
			Chicken c = new Chicken(TextureName.Chicken, GlobalPosition, player, spriteManager.Clone());
			c.SetSortingOffset(sortingOffset);
			c.GetSprite().SetAttachedGameObject(c);
			c.velocity = velocity;
			c.angularVelocity = angularVelocity;
			c.force = force;
			return c;
		}

		public static Texture2D[] GetFrames()
		{
			return frames;
		}
	}
}
