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
			

			Vector2 toMouse = Game.camera.GetMouseWorldPosition() - GlobalPosition;
			float armRot = GlobalRotation;

			GlobalRotation = -toMouse.GetAngle(Vector2.Right); //GlobalRotation + toMouse.GetAngle(new Vector2(globalTransform.m11, globalTransform.m12).Normalised());

			base.Update(deltaTime);
		}
	}
}
