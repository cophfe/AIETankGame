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
		float firstLength = 40;
		float secondLength = 40;
		float firstAngle;
		float secondAngle;

		GameObject second;

		public Arm(TextureName image, Vector2 offset, float scale, float rotation, GameObject player) : base(image, offset, scale, rotation, player)
		{
			firstAngle = rotation;
			secondAngle = rotation;
			firstLength = GetSprite().CurrentTexture().width/2;
			secondLength = firstLength;
			second = new GameObject(image, offset + new Vector2((float)Math.Cos(rotation), (float)Math.Sin(rotation)) * firstLength, scale, secondAngle, this);
			position = offset;
		}

		public override void Update(float deltaTime)
		{
			Vector2 mousePos = Game.camera.GetMouseWorldPosition();
			int sign = Math.Sign(mousePos.x - GlobalPosition.x);
			if (sign == 0)
				sign = 1;

			Vector2 targetVector = (mousePos - GlobalPosition);
			float angleToTarget = (float)Math.Atan2(targetVector.y, targetVector.x);
			float distance = targetVector.Magnitude() / GlobalScale.x;

			if (distance <= 0)
				distance = GlobalScale.x;

			if (distance >= (firstLength + secondLength))
			{
				firstAngle = angleToTarget;
				secondAngle = 0;
			}
			else
			{
				firstAngle = angleToTarget - sign *(float)Math.Acos(((distance * distance) + (firstLength * firstLength) - (secondLength * secondLength)) / (2 * distance * firstLength));
				secondAngle = Trig.pi - sign *(float)Math.Acos(((secondLength * secondLength) + (firstLength * firstLength) - (distance * distance)) / (2 * secondLength * firstLength));
			}
			GlobalRotation = firstAngle;
			second.LocalRotation = -secondAngle;
			//float angleBetween = (float)Math.Atan2(Vector2.Dot(targetVector, globalTransform.GetForwardVector()), Vector2.Dot(targetVector, globalTransform.GetRightVector()));
			//LocalRotation -= angleBetween * Math.Min(deltaTime * armSpeed, 1);

			if (float.IsNaN(rotation))
				Console.WriteLine("AAAAAHHHH");

			base.Update(deltaTime);
		}
	}
}
