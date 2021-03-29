using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Raylib;
using static Raylib.Raylib;
using Mlib;

namespace Project2D
{
	class Arm : GameObject
	{
		const float armSpeed = 15;

		public Arm(TextureName image, Vector2 offset, Vector2 scale, float rotation, GameObject player) : base(image, offset, scale, rotation, player)
		{

		}

		public override void Update(float deltaTime)
		{
			

			Vector2 targetVector = (Game.camera.GetMouseWorldPosition() - GlobalPosition).Normalised();
			float angleBetween = (float)Math.Atan2(Vector2.Dot(targetVector, globalTransform.GetForwardVector()), Vector2.Dot(targetVector, globalTransform.GetRightVector()));
			LocalRotation -= angleBetween * Math.Min(deltaTime * armSpeed, 1);

			//GlobalRotation = EaseCircInOut(deltaTime,  )
			//-toMouse.GetAngle(Vector2.Right);
			if (float.IsNaN(rotation))
				Console.WriteLine("AAAAAHHHH");

			base.Update(deltaTime);
		}
	}
}
