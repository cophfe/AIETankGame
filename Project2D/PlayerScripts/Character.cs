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
	class Character : PhysicsObject
	{
		static float velocityCap = 300f;
		static float accelerationCap = 4000;

		const float velocityFasterAddition = 100f;
		const float accelerationFasterAddition = 500f;

		bool standing = true;
		bool biting = false;
		bool endingBite = false;
		public bool broadcastingHunger = false;
		List<SuckedChickenInfo> suckedChickens = new List<SuckedChickenInfo>();
		const float chickenSuckSpeed = 0.5f;
		static Vector2 eatingOffset = new Vector2(-21, 21);
		Vector2 eatingFlip = Vector2.One;

		Eye eye1;
		Eye eye2;
		GameObject head;
		GameObject fix;
		GameObject chicken;
		GameObject headHolder;

		Sprite headSprite;

		Vector2 headOffset = new Vector2(-3, -58);

		float[] eyeOffset = new float[24]
		{
			0, 0, 4, 4.5f, 6, -4, -8, -9, -9, -8, -7f, -5f, -3, 4, 4.5f, 6, -4, -8, -9, -9, -8, -5.5f, -4, -1
		};

		float[] headEyeOffset = new float[10]
		{
			0, -5, -31, -35, -40, -44, -29, -14, 8, 3
		};
		public Character(TextureName fileName, Vector2 position, float scale, float rotation, Scene scene, Collider collider) : base(fileName, position, 1, collider, 0.5f, 3f, 0f, 0f, scene, 1, true, false)
		{
			eye1 = new Eye(new Vector2(-34, -21), 7, 0.14f);
			eye2 = new Eye(new Vector2(9, -19), eye1);
			AddChild(eye1);
			AddChild(eye2);
			//new Arm(armName, armOffset, armScale, 0, this);
			spriteManager = new Sprite(Sprite.GetFramesFromFolder("Player"), 30, 1, 23, this);
			spriteManager.Pause();
			spriteManager.SetFrame(0);

			headHolder = new GameObject(TextureName.Head, headOffset, 1, 0, this, false);
			headHolder.GetSprite().SetSort(0);

			head = new GameObject(TextureName.Head, Vector2.Zero, 1, 0, headHolder);
			headSprite = new Sprite(Sprite.GetFramesFromFolder("PlayerHead"), 30, 0, 9, head, true, 0);
			head.SetSprite(headSprite);
			headSprite.Pause();

			fix = new GameObject(TextureName.HeadFix, Vector2.Up * -150, 1, 0, head);
			fix.LocalScale = new Vector2(1, 3);

			chicken = new GameObject(TextureName.CookedChicken, eatingOffset, 0.5f, 0, headHolder);
			chicken.GetSprite().SetSort(-1);
			chicken.SetDrawn(false);

			LocalPosition = Vector2.One * 100;
			collider.SetCollisionLayer(CollisionLayer.Player);
		}

		Vector2 inputVelocity;
		Vector2 inputDirection = Vector2.Zero;
		bool charging = false;
		bool cooling = false;
		int chargeSpeed = 400;
		RLColor eyeColor = RLColor.BLACK;
		float buildUp = 0;
		int chickenEatAmount = 0;

		public override void Update()
		{
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
				velocityCap += velocityFasterAddition;
				accelerationCap += accelerationFasterAddition;
				spriteManager.SetSpeed(25);
			}
			if (IsKeyReleased(KeyboardKey.KEY_LEFT_SHIFT))
			{
				velocityCap -= velocityFasterAddition;
				accelerationCap -= accelerationFasterAddition;
				spriteManager.SetSpeed(30);
			}

			if (!biting && IsMouseButtonPressed(MouseButton.MOUSE_LEFT_BUTTON))
			{
				cooling = false;
				charging = true;
			}
			if (charging)
			{
				buildUp += Game.deltaTime * chargeSpeed;
				if (eyeColor.r == 255)
				{
					charging = false;
					buildUp = 0;
					eye1.SetLaser(true);
					Game.camera.SetShakeAmount(5);
				}
				else 
					while (buildUp >= 1)
					{
						eyeColor.r++;
					
						buildUp = 0;
						eye1.SetTint(eyeColor);
						Game.camera.SetShakeAmount(eyeColor.r * 0.005f);
					}
			}
			if (cooling)
			{
				buildUp += Game.deltaTime * chargeSpeed;
				if (eyeColor.r == 0)
				{
					cooling = false;
					buildUp = 0;
				}
				else 
					while (buildUp >= 1)
					{
						eyeColor.r--;
						buildUp = 0;
						eye1.SetTint(eyeColor);
						Game.camera.SetShakeAmount(eyeColor.r * 0.005f);
					}
			}
			if (IsMouseButtonReleased(MouseButton.MOUSE_LEFT_BUTTON))
			{
				cooling = true;
				charging = false;
				eye1.SetLaser(false);
				buildUp = 0;
			}

			if (!charging && IsMouseButtonPressed(MouseButton.MOUSE_RIGHT_BUTTON))
			{
				headSprite.SetLimits(0, 5);
				headSprite.SetBackwards(false);
				headSprite.PlayFrom((headSprite.CurrentFrame() + 1) % 5);
				biting = true;
				endingBite = false;
				broadcastingHunger = true;
			}
			if (biting)
			{
				int fr = headSprite.CurrentFrame();
				UpdateTiedChickens();
				if (endingBite)
				{
					if (fr <= 0)
					{
						headSprite.Pause();
						headSprite.SetBackwards(false);
						biting = false;
						endingBite = false;
						broadcastingHunger = false;
					}

					if (fr >= 10)
					{
						headSprite.Pause();
						headSprite.SetFrame(0);
						endingBite = false;
						biting = false;
						broadcastingHunger = false;
						if (chicken.GetDrawn())
						{
							chicken.SetDrawn(false);
						}
					}
				}
				else if (fr == 5)
				{
					headSprite.Pause();
				}
			}
			if (IsMouseButtonReleased(MouseButton.MOUSE_RIGHT_BUTTON) && !endingBite)
			{
				if (headSprite.CurrentFrame() >= 5)
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
			float offset = eyeOffset[spriteManager.CurrentFrame()];
			eye1.SetOffsetY(offset + headEyeOffset[headSprite.CurrentFrame()]);
			headHolder.LocalPosition = new Vector2(headHolder.LocalPosition.x, offset + headOffset.y);
			
			//make it so this can't overshoot wanted value (inputdirection.normalized * velocityCap)
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
		}

		public override void Draw()
		{
			 base.Draw();
		}

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
					amountThrough = (Game.currentTime - suckedChickens[i].timeAtStart) * 0.001f * chickenSuckSpeed;
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
