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
		static float velocityCap = 200f;
		static float rotateSpeedCap = 4f;
		static float accelerationCap = 1500;
		static float rotateAcceleration = 10;

		const float velocityFasterAddition = 100f;
		const float accelerationFasterAddition = 500f;

		public Player(TextureName fileName, TextureName armName, Vector2 armOffset, Vector2 armScale, Vector2 position, Vector2 scale, float rotation, Scene scene, PolygonCollider collider) : base(fileName, position, scale, collider, 0f, 3f, 0f, 0f, scene, 1)
		{
			new Arm(armName, armOffset, armScale, 0, this);
		}

		Vector2 inputVelocity;
		Vector2 inputDirection = Vector2.Zero;

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
			}
			if (IsKeyDown(KeyboardKey.KEY_D))
			{
				inputDirection += Vector2.Right;
			}
			if (IsKeyDown(KeyboardKey.KEY_E))
			{
				if (angularVelocity > -rotateSpeedCap)
					angularVelocity -= rotateAcceleration * deltaTime;
			}
			if (IsKeyDown(KeyboardKey.KEY_Q))
			{
				if (angularVelocity < rotateSpeedCap)
					angularVelocity += rotateAcceleration * deltaTime;
			}

			if (IsKeyPressed(KeyboardKey.KEY_LEFT_SHIFT))
			{
				velocityCap += velocityFasterAddition;
				accelerationCap += accelerationFasterAddition;
			}
			if (IsKeyReleased(KeyboardKey.KEY_LEFT_SHIFT))
			{
				velocityCap -= velocityFasterAddition;
				accelerationCap -= accelerationFasterAddition;
			}			

			inputVelocity = (inputDirection.Normalised() * velocityCap - velocity).Normalised() * (accelerationCap * deltaTime);
			AddVelocity(inputVelocity);

			base.Update(deltaTime);
		}
	}
}
