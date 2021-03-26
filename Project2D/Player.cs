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

		const float movementFasterAddition = 100f;

		public Player(string fileName, Vector2 position, Vector2 scale, float rotation, Scene scene) : base(fileName, position, scale, null, 0f, 3f, 0, 0, scene)
		{

		}

		Vector2 inputVelocity;
		Vector2 inputDirection = Vector2.Zero;
		bool isMoving = false;

		public override void Update(float deltaTime)
		{
			isMoving = false;
			inputDirection = Vector2.Zero;

			if (IsKeyDown(KeyboardKey.KEY_W))
			{
				inputDirection -= Vector2.Up;
				
				isMoving = true;
			}
			if (IsKeyDown(KeyboardKey.KEY_S))
			{
				inputDirection += Vector2.Up;
				isMoving = true;
			}
			if (IsKeyDown(KeyboardKey.KEY_D))
			{
				inputDirection += Vector2.Right;
				isMoving = true;
			}
			if (IsKeyDown(KeyboardKey.KEY_A))
			{
				inputDirection -= Vector2.Right;
				isMoving = true;
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
				velocityCap += movementFasterAddition;
			}
			if (IsKeyReleased(KeyboardKey.KEY_LEFT_SHIFT))
			{
				velocityCap -= movementFasterAddition;
			}

			inputVelocity = (inputDirection.Normalised() * velocityCap - velocity).Normalised() * (accelerationCap * deltaTime);
			velocity += inputVelocity;

			base.Update(deltaTime);
		}
	}
}
