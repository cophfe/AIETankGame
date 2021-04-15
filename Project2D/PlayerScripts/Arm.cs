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
		//unused inverse kinematics arm. add it as a child to the player object to use
		//not commented because its not being used

		float firstLength = 40;
		float secondLength = 40;
		float firstAngle;
		float secondAngle;

		GameObject second;

		Player player;

		public Arm(TextureName image, Vector2 offset, float scale, float rotation, Player player) : base(image, offset, scale, rotation, player)
		{
			firstAngle = rotation;
			secondAngle = rotation;
			firstLength = GetSprite().CurrentTexture().width/2;
			secondLength = firstLength;
			second = new GameObject(image, offset + new Vector2((float)Math.Cos(rotation), (float)Math.Sin(rotation)) * firstLength, scale, secondAngle, this);
			position = offset;
			this.player = player;
		}

		public override void Update()
		{
			Vector2 mousePos = player.GetTiedCamera().GetMouseWorldPosition();
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
				secondAngle = Num.pi - sign *(float)Math.Acos(((secondLength * secondLength) + (firstLength * firstLength) - (distance * distance)) / (2 * secondLength * firstLength));
			}
			GlobalRotation = firstAngle;
			second.LocalRotation = -secondAngle;

			if (float.IsNaN(rotation))
				Console.WriteLine("AAAAAHHHH");

			base.Update();
		}
	}
}
