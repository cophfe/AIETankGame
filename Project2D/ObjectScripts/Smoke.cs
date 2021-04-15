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
	class Smoke : PhysicsObject //could have inherited from Gameobject but whatever
	{
		Vector2 target; //the target position for the smoke to move towards
		Vector2 startPosition; //the position the smoke started at
		float speed;
		GameObject targetObject; //the target is set to be the position of this object + the offset + the randomness vector
		float existanceTimer = 0;
		Vector2 offset = new Vector2(30, 24); //the amount the target is offset from the targetobjects local position. this should ABSOLUTELY be set in the constuctor my god
		Vector2 randomnessVector; //smoke's target has a random offset tacked on so not all smoke particles go to the same place

		public Smoke(Vector2 position, float rotation, float scale, float suckSpeed, GameObject targetObj, Vector2 randomVector, float angularVelocity, GameObject parent) : base(TextureName.Pupil, position, scale, null, 1f, 0.5f, 0, rotation, parent)
		{
			AddAngularVelocity(angularVelocity);
			
			spriteManager.SetLayer(SpriteLayer.Background);

			target = targetObj.GlobalPosition + offset + randomVector;
			targetObject = targetObj;
			speed = suckSpeed;
			startPosition = position;
			randomnessVector = randomVector;
			spriteManager.SetTint(RLColor.DARKGRAY);
		}

		public override void Update()
		{
			//offset is flipped whenever the target object is flipped. This only works in the very specific situation this gameobject was created for, another bad piece of code
			offset.x = Math.Abs(offset.x) * -Math.Sign(targetObject.LocalPosition.x);
			target = targetObject.GlobalPosition + offset + randomnessVector;
			if (existanceTimer >= 0.5f) //once the smoke has existed for this long it slowly disappears
			{
				scale -= new Vector2(Game.deltaTime, Game.deltaTime) * 0.5f;
				if (scale.x <= 0)
				{
					Delete();
				}
			}
			if (existanceTimer < 1) //the timer also functions as t in this easing function
			{
				//squaring existanceTimer makes it ease in a bit
				position = Vector2.Lerp(startPosition, target, existanceTimer * existanceTimer);
			}
			existanceTimer += Game.deltaTime * speed;

			base.Update();
		}
	}
}

