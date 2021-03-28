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
		public Arm(TextureName image, Vector2 offset, Vector2 scale, float rotation, GameObject player) : base(image, offset, scale, rotation, player)
		{

		}

		public override void Update(float deltaTime)
		{
			

			Vector2 toMouse = (Game.camera.GetMouseWorldPosition() - GlobalPosition).Normalised();
			
			GlobalRotation = (-GlobalRotation - toMouse.GetAngle(globalTransform.GetRightVector()));
			//-toMouse.GetAngle(Vector2.Right);
			if (float.IsNaN(rotation))
				Console.WriteLine("AAAAAHHHH");

			base.Update(deltaTime);
		}
	}
}
