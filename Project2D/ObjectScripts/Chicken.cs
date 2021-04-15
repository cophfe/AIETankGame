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
		public bool still = true; //if the chicken is moving or not
		readonly Player player; //the player object in the scene
		public static float distanceToScare = 300; //if the distance between the player and the chicken is lower than this, it becomes scared.
		public static float speed = 100; //the regular speed of the chicken
		public static float runMultiplier = 3; //the amount that the speed is multiplied by when scared
		public static float runDist = 200; //distance past the distanceToScare before the chicken stops being scared
		float timer = 0; //the event timer
		public static float eventTime = 1; //the time that an event happens (e.g. if it is one than an event happens once a second)
		public static float accelerationCap = 2000; //maximum acceleration
		public static float featherExplosionNumber = 24; //amount of feathers that explode from the chicken at death
		public static float featherExplosionSpeed = 200; //the speed of chickens at death
		public float cookedValue = 0; //how much a chicken is cooked (depreciates over time)
		public static float cookedLimit = 3; //the limit of cookedValue before the chicken dies
		bool controlledByPlayer = false; //if the chicken has died and is being sucked toward the player, this value is enabled
		static Colour colour = new Colour(255, 255, 255, 255); //the tint of the chicken

		bool dead = false;  //value is true after death animation has played
		bool dying = false; //value is true while the death animation is playing

		Vector2 targetVelocity = Vector2.zero; //the aim for velocity (acceleration is used to move toward it)
		static Matrix3 explosionRotationMatrix = Matrix3.GetRotateZ(Num.pi * 2 / featherExplosionNumber); //used in calculating the feather rotation
		static readonly Texture2D[] frames = Sprite.GetFramesFromFolder("Chicken"); //every frame used by the chicken

		float deathPercentDone = 0; //value used when player is vacuuming the chicken. when it reaches 100 the chicken is pulled toward the player
		
		public Chicken(TextureName fileName, Vector2 position, Player player, Sprite loadedSprite = null) : base(fileName, position, 0.5f, new Collider(-40, -55, 80, 110, CollisionLayer.Enemy), 0.5f, 0.5f, 1f, 0f, density: 1, isRotatable: false)
		{
			if (loadedSprite != null)
				spriteManager = loadedSprite;
			else
				spriteManager = new Sprite(frames, 24, 1, 11, this, colour);

			spriteManager.SetFrame(0);
			spriteManager.Pause();
			spriteManager.SetLayer(SpriteLayer.Background);
			spriteManager.SetTint(colour);
			timer += (float)Game.globalRand.NextDouble();
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
							//when the chicken is finished being sucked it moves towards the player
							//the player has a function that does this for every tied chicken
							//the chicken is tied to the player and is no longer updated unless the player cancels
							rotation = 0;
							player.TieDeadChicken(this);
							controlledByPlayer = true;
						}
						else
						{
							//chicken rotates a little bit while it is being sucked in
							rotation = (float)Math.Sin(Game.currentTime / (200 - deathPercentDone * 0.5f)) * deathPercentDone * 0.001f;
						}
						//smoke has a start position and a target object that it moves towards using an easing function
						new Smoke(LocalPosition + new Vector2((float)Game.globalRand.NextDouble() * 50 - 25, (float)Game.globalRand.NextDouble() * 30 + 10), 0, (float)Game.globalRand.NextDouble() * 0.15f, 5, player.GetChicken(), new Vector2((float)Game.globalRand.NextDouble() * 20 - 10, (float)Game.globalRand.NextDouble() * 20 - 10), 0, Game.GetCurrentScene());
					}
				}
				return;
			}
			if (dying)
			{
				//when frame reaches the chicken dead frame, it is paused there, the collider is removed, and the chicken is considered dead
				if (spriteManager.GetCurrentFrame() == 51)
				{
					spriteManager.Pause();
					dead = true;
					RemoveCollider(Game.GetCurrentScene());
					player.chickensBeingSucked = true;
				}
				return;
			}
			//the value of green and blue changes when cooked value changes to make the chicken become red before it dies
			byte gb = (byte)(colour.GetGreen() - cookedValue / cookedLimit * colour.GetGreen());
			spriteManager.SetTint(new Colour(colour.GetRed(), gb, gb, 255));

			//if the chicken is fully cooked
			if (cookedValue > cookedLimit)
			{
				//initiate feather explosion
				Vector2 rot = Vector2.right * featherExplosionSpeed;
				velocity = Vector2.zero;
				for (int i = 0; i < featherExplosionNumber; i++)
				{
					rot = explosionRotationMatrix * rot;
					new Feather(position + new Vector2(), (float)Game.globalRand.NextDouble() * Num.pi * 2 - Num.pi, (float)Game.globalRand.NextDouble() * 0.2f + 0.1f, rot, (float)Game.globalRand.NextDouble() - 0.5f, Game.GetCurrentScene());
				}
				//reset the cookedness
				cookedValue = 0;
				
				//make the chicken play the explosion animation
				spriteManager.PlayFrom(35);
				spriteManager.SetLimits(35, 51);
				spriteManager.SetTint(RLColor.WHITE);
				spriteManager.SetSpeed(16);

				dying = true;
				drag = 3;
				return;
			}
			else if (cookedValue > 0)
			{
				//cooked value depreciates over time
				cookedValue -= Game.deltaTime;
				if (still)
				{
					//if the player is still and cooked value is more than zero, it is made to be not still (aka it runs away from the player)
					spriteManager.SetLimits(1, 10);
					spriteManager.PlayFrom(1);
					still = false;
					targetVelocity = Vector2.zero;
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
				//if player is too close the chicken runs away
				if (delta.MagnitudeSquared() < distanceToScare * distanceToScare)
				{
					spriteManager.SetLimits(1, 10);
					spriteManager.PlayFrom(1);
					still = false;
					targetVelocity = Vector2.zero;
					return;
				}

				//an event plays. There are two events, half the time one plays, half the time the other does.
				if (timer > eventTime)
				{
					timer = 0;
					double r = Game.globalRand.NextDouble();

					if (r > 0.5f)
					{
						//the first event: the chicken walks in a random direction
						spriteManager.SetLimits(12, 34);
						spriteManager.PlayFrom(12);

						//if the chicken is already moving than just rotate the target velocity (could have just used targetVelocity.Rotate(angle) but i didn't think about that)
						//else set the target velocity to a randomly rotated vector
						if (targetVelocity.MagnitudeSquared() > 0.001f)
							targetVelocity = Matrix3.GetRotateZ((float)Game.globalRand.NextDouble() * Num.pi - Num.pi / 2) * targetVelocity;
						else
							targetVelocity = Matrix3.GetRotateZ((float)Game.globalRand.NextDouble() * Num.pi * 2 - Num.pi) * Vector2.one * speed;
					}
					else
					{
						//the second event: the chicken stays still
						targetVelocity = Vector2.zero;
						spriteManager.Pause();
						spriteManager.SetFrame(0);
					}
				}
			}
			else //if the chicken is running away from the player
			{
				//if the chicken has gotten far enough away from the player, unpanic it and make it stand still (as long as it isn't cooked at all)
				if (delta.MagnitudeSquared() > (distanceToScare + runDist) * (distanceToScare + runDist) && cookedValue == 0)
				{
					targetVelocity = Vector2.zero;
					spriteManager.Pause();
					spriteManager.SetFrame(0);
					still = true;
				}
				else
				{
					//the chicken throws out a feather every frame that this equality is true
					//I tried to make it independant of framerate, which I thiiink it is but idk havent thought about it that much
					if (Game.globalRand.NextDouble() < Game.deltaTime * 4)
					{
						new Feather(position + new Vector2(), (float)Game.globalRand.NextDouble() * Num.pi * 2 - Num.pi, (float)Game.globalRand.NextDouble() * 0.2f + 0.1f, -0.5f * velocity + new Vector2((float)Game.globalRand.NextDouble() * 10 - 5, (float)Game.globalRand.NextDouble() * 10 - 5), (float)Game.globalRand.NextDouble() - 0.5f, Game.GetCurrentScene());
					}

					//the chicken runs away from the player
					targetVelocity = delta.Normalised() * (-speed * runMultiplier);
				}
			}

			//flip sprite when it is moving to the right
			if (velocity.x > 0.1f)
				spriteManager.FlipX(true);
			else if (velocity.x < -0.1f)
				spriteManager.FlipX(false);

			if (float.IsNaN(targetVelocity.x))
			{
				targetVelocity = Vector2.zero;
				return;
			}

			//same idea as player movement
			//prety sure this could easily be turned into one if statement, but it confuses my brain to look in it's general direction so I'll leave it as it is
			Vector2 deltaV;
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

		public void CancelSuck() //run if the player stops right clicking whilst the chicken is moving towards the player's mouth
		{
			controlledByPlayer = false;
			deathPercentDone = 0;
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
