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
	class Player : PhysicsObject
	{
		static float velocityCap = 300f;
		static float accelerationCap = 4000;
		static float accelerationCapNorm = 4000;
		static float velocityCapNorm = 300;
		static float velocityCapSlow = 200;
		static int frameSpeed = 25;
		static int frameSpeedNorm = 25;
		static int frameSpeedAddition = 5;
		static int frameSpeedSlowDown = -20;

		const float velocityFasterAddition = 100f;
		const float accelerationFasterAddition = 500f;

		bool standing = true;
		bool biting = false;
		bool endingBite = false;
		public bool broadcastingHunger = false;
		List<SuckedChickenInfo> suckedChickens = new List<SuckedChickenInfo>();
		const float chickenSuckSpeed = 0.5f;
		static Vector2 eatingOffset = new Vector2(-21, 21);
		float defaultIMass;

		readonly Eye eye1;
		readonly Eye eye2;
		readonly GameObject head;
		readonly GameObject fix;
		readonly GameObject chicken;
		readonly GameObject headHolder;
		readonly Sprite headSprite;

		SmoothCamera camera;

		Vector2 headOffset = new Vector2(-3, -58);

		HungerHolder h;
		Vector2 inputVelocity;
		Vector2 inputDirection = Vector2.Zero;
		bool charging = false;
		bool cooling = false;
		int chargeSpeed = 500;
		RLColor eyeColor = RLColor.BLACK;
		float buildUp = 0;
		int chickenEatAmount = 0;
		public bool chickensBeingSucked = false;
		public int chickenTotalInScene = 0;
		public int chickensEatenTotal = 0;
		bool running = false;
		bool newGamePlus = false;

		float[] eyeOffset = new float[24]
		{
			0, 0, 4, 4.5f, 6, -4, -8, -9, -9, -8, -7f, -5f, -3, 4, 4.5f, 6, -4, -8, -9, -9, -8, -5.5f, -4, -1
		};

		float[] headEyeOffset = new float[10]
		{
			0, -5, -31, -35, -40, -44, -29, -14, 8, 3
		};

		public Player(TextureName fileName, Vector2 position, Scene scene, Collider collider) : base(fileName, position, 1, collider, 0.5f, 3f, 0f, 0f, scene, 1, true, false)
		{
			eye1 = new Eye(new Vector2(-34, -21), 7, 0.14f);
			eye2 = new Eye(new Vector2(9, -19), eye1);
			AddChild(eye1);
			AddChild(eye2);
			spriteManager = new Sprite(Sprite.GetFramesFromFolder("Player"), 30, 1, 23, this, RLColor.WHITE);
			spriteManager.Pause();
			spriteManager.SetFrame(0);

			headHolder = new GameObject(TextureName.Head, headOffset, 1, 0, this, false);
			headHolder.GetSprite().SetSort(0);

			head = new GameObject(TextureName.Head, Vector2.Zero, 1, 0, headHolder);
			headSprite = new Sprite(Sprite.GetFramesFromFolder("PlayerHead"), 30, 0, 9, head, RLColor.WHITE, true, 0);
			head.SetSprite(headSprite);
			headSprite.Pause();

			fix = new GameObject(TextureName.HeadFix, Vector2.Up * -150, 1, 0, head)
			{
				LocalScale = new Vector2(1, 3)
			};

			chicken = new GameObject(TextureName.CookedChicken, eatingOffset, 0.5f, 0, headHolder);
			chicken.GetSprite().SetSort(-1);
			chicken.SetDrawn(false);

			SortChildren();
			headHolder.SortChildren();

			LocalPosition = Vector2.One * 100;
			collider.SetCollisionLayer(CollisionLayer.Player);
			spriteManager.SetLayer(SpriteLayer.Midground);
			defaultIMass = iMass;
			h = new HungerHolder(TextureName.Xray, new Vector2(Game.screenWidth - 100, 130), 1, null);
			
			Game.GetCurrentScene().AddUIElement(h);
		}

		

		public override void Update()
		{
			accelerationCap = accelerationCapNorm;
			if (newGamePlus)
			{
				
				velocityCap = velocityCapSlow;
				frameSpeed = frameSpeedNorm - frameSpeedSlowDown;
				scale = new Vector2(1.05f, 1.05f);
				int f = spriteManager.GetCurrentFrame();
				if ((f >= 1 && f <= 3) || (f >= 12 && f <= 14))
				{
					camera.SetShakeAmount(4);
					new Feather(position + new Vector2(Game.globalRand.Next(-20,20), 110), (float)Game.globalRand.NextDouble() * Trig.pi * 2 - Trig.pi, (float)Game.globalRand.NextDouble() * 0.2f + 0.1f, -3f * new Vector2(velocity.x, (float)Game.globalRand.NextDouble() * 50 - 25), (float)Game.globalRand.NextDouble() - 0.5f, Game.GetCurrentScene(), SpriteLayer.Foreground, true);
					iMass = 0.000001f;
					Eye.laserPower = 200000000;
					Eye.cookStrength = 50;
					chickenEatAmount = 200;
				}
				else
				{
					camera.SetShakeAmount(0);
				}
			}
			else
			{
				Eye.cookStrength = 5;
				iMass = defaultIMass;
				Eye.laserPower = 20000000;
				velocityCap = velocityCapNorm;
				frameSpeed = frameSpeedNorm;
				scale = Vector2.One;
			}
			inputDirection = Vector2.Zero;
			
			if (IsKeyDown(KeyboardKey.KEY_W))
			{
				inputDirection -= Vector2.Up;
			}
			if (IsKeyDown(KeyboardKey.KEY_S))
			{
				inputDirection += Vector2.Up;
			}
			if (IsKeyDown(KeyboardKey.KEY_A))
			{
				inputDirection -= Vector2.Right;
				spriteManager.FlipX(false);
				head.GetSprite().FlipX(false);
				fix.GetSprite().FlipX(false);
				eye1.FlipX(false);
				chicken.GetSprite().FlipX(false);
				chicken.LocalPosition = new Vector2(eatingOffset.x, eatingOffset.y);
				headHolder.LocalPosition = new Vector2(headOffset.x, LocalPosition.y);
			}
			if (IsKeyDown(KeyboardKey.KEY_D))
			{
				inputDirection += Vector2.Right;
				spriteManager.FlipX(true);
				head.GetSprite().FlipX(true);
				fix.GetSprite().FlipX(true);
				eye1.FlipX(true);
				chicken.GetSprite().FlipX(true);
				chicken.LocalPosition = new Vector2(-eatingOffset.x, eatingOffset.y);
				headHolder.LocalPosition = new Vector2(-headOffset.x, LocalPosition.y);
			}

			if (IsKeyPressed(KeyboardKey.KEY_LEFT_SHIFT))
			{
				running = true;
			}
			if (IsKeyReleased(KeyboardKey.KEY_LEFT_SHIFT))
			{
				running = false;
			}
			if (running)
			{
				frameSpeed -= frameSpeedAddition;
				velocityCap += velocityFasterAddition;
				accelerationCap += accelerationFasterAddition;
			}
			spriteManager.SetSpeed(frameSpeed);

			if (!biting && IsMouseButtonPressed(MouseButton.MOUSE_LEFT_BUTTON))
			{
				cooling = false;
				charging = true;
			}
			if (charging)
			{
				buildUp += Game.deltaTime * chargeSpeed;

				while (buildUp >= 1)
				{
					if (eyeColor.r >= 255)
					{
						eyeColor.r = 255;
						charging = false;
						buildUp = 0;
						eye1.SetLaser(true);
						camera.SetShakeAmount(5);
						return;
					}
					eyeColor.r++;
					buildUp--;
					eye1.SetTint(eyeColor);
					camera.SetShakeAmount(eyeColor.r * 0.005f);
				}
			}
			if (cooling)
			{
				buildUp += Game.deltaTime * chargeSpeed * 2;

				while (buildUp >= 1)
				{
					if (eyeColor.r == 0)
					{
						cooling = false;
						buildUp = 0;
						return;
					}

					eyeColor.r--;
					buildUp--;
					eye1.SetTint(eyeColor);
					camera.SetShakeAmount(eyeColor.r * 0.005f);
				}
			}
			if (IsMouseButtonReleased(MouseButton.MOUSE_LEFT_BUTTON))
			{
				cooling = true;
				charging = false;
				eye1.SetLaser(false);
				buildUp = 0;
			}

			if (!charging && IsMouseButtonPressed(MouseButton.MOUSE_RIGHT_BUTTON) && eyeColor.r != 255)
			{
				headSprite.SetLimits(0, 5);
				headSprite.SetBackwards(false);
				headSprite.PlayFrom((headSprite.GetCurrentFrame() + 1) % 5);
				biting = true;
				endingBite = false;
				broadcastingHunger = true;
				camera.SetShakeAmount(0);
			}
			if (biting)
			{
				int fr = headSprite.GetCurrentFrame();
				UpdateTiedChickens();
				
				if (chickensBeingSucked)
				{
					if (fr > 5)
						camera.SetShakeAmount( 0.5f * (5 - fr % 5));
					else
						camera.SetShakeAmount(0.3f * (fr));
				}

				if (endingBite)
				{
					if (fr <= 0)
					{
						headSprite.Pause();
						headSprite.SetBackwards(false);
						biting = false;
						endingBite = false;
						chickensBeingSucked = false;
						broadcastingHunger = false;
						camera.SetShakeAmount(0);
					}

					if (fr >= 10)
					{
						headSprite.Pause();
						headSprite.SetFrame(0);
						endingBite = false;
						biting = false;
						chickensBeingSucked = false;
						broadcastingHunger = false;
						camera.SetShakeAmount(0);
						chickensEatenTotal += chickenEatAmount;
						h.hungerPercent = (float)chickensEatenTotal / chickenTotalInScene;

						if (newGamePlus || chickensEatenTotal >= chickenTotalInScene)
						{
							newGamePlus = true;
							chickenEatAmount = 300;
						}
						else
						{
							chickenEatAmount = 0;
						}
						
						chicken.SetDrawn(false);					
					}
					else if (fr > 6)
					{
						camera.SetShakeAmount(4);
					}
				}
				else if (fr == 5)
				{
					headSprite.Pause();
					if (chickensEatenTotal > 0 && IsMouseButtonPressed(MouseButton.MOUSE_LEFT_BUTTON))
					{
						Vector2 direction = (camera.GetMouseWorldPosition() - chicken.GlobalPosition).Normalised() * 500;
						new PhysicsChicken(chicken.GlobalPosition, direction, 0.1f, 3);
						Vector2 rot = direction.Rotated(-Trig.pi / 8);
						for (int i = 0; i < 15; i++)
						{
							new Feather(position, (float)Game.globalRand.NextDouble() * Trig.pi * 2 - Trig.pi, (float)Game.globalRand.NextDouble() * 0.2f + 0.1f, (float)(Game.globalRand.NextDouble() * 0.5f + 1) * rot.Rotated((float)Game.globalRand.NextDouble() * 0.1f - 0.05f), (float)Game.globalRand.NextDouble() - 0.5f, Game.GetCurrentScene(), SpriteLayer.Foreground);
							rot.Rotate(Trig.pi * 0.01675f);
						}
						if (!newGamePlus)
						{
							chickensEatenTotal--;
							h.hungerPercent = (float)(chickensEatenTotal) / chickenTotalInScene;
							h.chickenCount = chickenTotalInScene - chickensEatenTotal;
						}
						
						return;
					}
				}
			}
			if (IsMouseButtonReleased(MouseButton.MOUSE_RIGHT_BUTTON) && !endingBite)
			{
				if (headSprite.GetCurrentFrame() >= 5)
				{
					headSprite.SetLimits(5, 10);
					headSprite.Play();
					CancelTiedChickens();
				}
				else
				{
					headSprite.SetBackwards(true);
				}
				endingBite = true;
			}

			if (standing && (inputDirection.x != 0 || inputDirection.y != 0))
			{
				standing = false;
				spriteManager.PlayFrom(1);
			}
			if ((inputDirection.x == 0 && inputDirection.y == 0)&& !standing)
			{
				spriteManager.Pause();
				spriteManager.SetFrame(0);
				standing = true;
			}
			float offset = eyeOffset[spriteManager.GetCurrentFrame()];
			eye1.SetOffsetY(offset + headEyeOffset[headSprite.GetCurrentFrame()]);
			headHolder.LocalPosition = new Vector2(headHolder.LocalPosition.x, offset + headOffset.y);
			
			Vector2 cache = inputDirection.Normalised() * velocityCap - velocity;
			
			if (cache.MagnitudeSquared() > accelerationCap * Game.deltaTime * accelerationCap * Game.deltaTime)
				inputVelocity = cache.Normalised() * (accelerationCap * Game.deltaTime);
			else
			{
				inputVelocity = cache;
			}
			inputVelocity = velocityCap > accelerationCap * Game.deltaTime ? inputVelocity : cache + velocity;
			AddVelocity(inputVelocity);
			base.Update();
			LineOfSight.SetOrigin(LocalPosition + lOSOffset);
			if (!newGamePlus)
				h.chickenCount = chickenTotalInScene - chickensEatenTotal;
		}

		Vector2 lOSOffset = new Vector2(0, 50);

		public void TieDeadChicken(Chicken chicken)
		{
			if (chicken.IsDead)
			{
				suckedChickens.Add(new SuckedChickenInfo(chicken, Game.currentTime, chicken.LocalPosition));
				chicken.SetSortingOffset(1000);
			}
		}

		void UpdateTiedChickens()
		{
			if (suckedChickens.Count > 0)
			{
				float amountThrough;
				Vector2 delta;
				for (int i = 0; i < suckedChickens.Count; i++)
				{
					amountThrough = (Game.currentTime - suckedChickens[i].timeAtStart) * 0.005f * chickenSuckSpeed;
					delta = chicken.GlobalPosition - suckedChickens[i].positionAtStart;

					//easing: https://easings.net/#easeInOutCubic
					if (amountThrough >= 1)
					{
						suckedChickens[i].chicken.Delete();
						suckedChickens.RemoveAt(i);
						i--;
						chicken.SetDrawn(true);
						chickenEatAmount++;
						continue;
					}
					if (amountThrough < 0.5f)
					{
						delta.x *= 4 * amountThrough * amountThrough * amountThrough;
						delta.y *= 4 * amountThrough * amountThrough * amountThrough;
					}
					else
					{
						delta.x *= 1 - (-2 * amountThrough + 2)  * (-2 * amountThrough + 2)  * (-2 * amountThrough + 2) / 2;
						delta.y *= 1 - (-2 * amountThrough + 2)  * (-2 * amountThrough + 2)  * (-2 * amountThrough + 2) / 2;
					}
					suckedChickens[i].chicken.LocalPosition = suckedChickens[i].positionAtStart + delta;
				}
			}
		}

		void CancelTiedChickens()
		{
			for (int i = 0; i < suckedChickens.Count; i++)
			{
				suckedChickens[i].chicken.CancelSuck();
			}
			suckedChickens.Clear();
		}

		public Vector2 GetChickenPos()
		{
			return chicken.GlobalPosition;
		}

		public GameObject GetChicken()
		{
			return chicken;
		}

		public void SetTiedCamera(SmoothCamera camera)
		{
			this.camera = camera;
		}

		public SmoothCamera GetTiedCamera()
		{
			return camera;
		}

		class SuckedChickenInfo
		{
			public Chicken chicken;
			public Vector2 positionAtStart;
			public float timeAtStart;

			public SuckedChickenInfo(Chicken chicken, float timeAtStart, Vector2 positionAtStart)
			{
				this.chicken = chicken;
				this.timeAtStart = timeAtStart;
				this.positionAtStart = positionAtStart;
			}
		}
	}
}
