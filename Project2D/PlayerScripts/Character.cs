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

		Eye eye1;
		Eye eye2;

		float[] eyeOffset = new float[24]
		{
			0, 0, 4, 4.5f, 6, -4, -8, -9, -9, -8, -7f, -5f, -3, 4, 4.5f, 6, -4, -8, -9, -9, -8, -5.5f, -4, -1
		};//num 12 (-5) and 22 (-7) cut out 
		public Character(TextureName fileName, Vector2 position, float scale, float rotation, Scene scene, Collider collider) : base(fileName, position, 1, collider, 0.5f, 3f, 0f, 0f, scene, 1, true, false)
		{
			eye1 = new Eye(new Vector2(-34, -21), 7, 0.14f);
			eye2 = new Eye(new Vector2(9, -19), eye1);
			addChild(eye1);
			addChild(eye2);
			//new Arm(armName, armOffset, armScale, 0, this);
			spriteManager = new Sprite(Sprite.GetFramesFromFolder("Walking"), 30, 1, 23, this);
			spriteManager.Pause();
			spriteManager.SetFrame(0);
			collider.SetCollisionLayer(CollisionLayer.Player);
		}

		Vector2 inputVelocity;
		Vector2 inputDirection = Vector2.Zero;
		bool charging = false;
		bool cooling = false;
		int chargeSpeed = 400;
		RLColor eyeColor = RLColor.BLACK;
		float buildUp = 0;
		public override void Update(float deltaTime)
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
				eye1.FlipX(false);
			}
			if (IsKeyDown(KeyboardKey.KEY_D))
			{
				inputDirection += Vector2.Right;
				spriteManager.FlipX(true);
				eye1.FlipX(true);
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

			if (IsMouseButtonPressed(MouseButton.MOUSE_LEFT_BUTTON))
			{
				cooling = false;
				charging = true;
			}
			if (charging)
			{
				buildUp += deltaTime * chargeSpeed;
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
				buildUp += deltaTime * chargeSpeed;
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
			eye1.SetOffsetY(offset);
			
			//make it so this can't overshoot wanted value (inputdirection.normalized * velocityCap)
			Vector2 cache = inputDirection.Normalised() * velocityCap;
			if ((cache - velocity).MagnitudeSquared() > accelerationCap * deltaTime * accelerationCap * deltaTime)
				inputVelocity = (cache - velocity).Normalised() * (accelerationCap * deltaTime);
			else
			{
				inputVelocity = (cache - velocity);
			}
			inputVelocity = velocityCap > accelerationCap * deltaTime ? inputVelocity : cache;
			AddVelocity(inputVelocity);
			base.Update(deltaTime);
		}

		public override void Draw()
		{
			base.Draw();
		}
	}
}
